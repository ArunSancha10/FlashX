using outofoffice.Dto;
using outofoffice.Repositories.Interfaces;
using System.Diagnostics;
using System.Net;
using System.Web;

namespace outofoffice.Helper
{
    public class GetTeamsToken
    {
        private readonly IMessageAppListRepository _messageAppListRepository;
        private readonly IConfiguration _configuration;
        public GetTeamsToken(IMessageAppListRepository messageAppListRepository, IConfiguration configuration)
        {
            _messageAppListRepository = messageAppListRepository;
            _configuration = configuration;
           
        }

        #region Global Variables

        private static string clientId = "370fef53-0f51-426b-970b-31b21f16dd15";
        private static string tenantId = "common";
        private static string clientSecret = "ztg8Q~oPKn9~zeDtMXlohFQK8mzAtc3HfsZqwbJ1";
        private static string redirectUri = "http://localhost:12345/";
        private static string[] scopes = {
        "offline_access",
        "user.read",
        "mailboxSettings.readWrite",
        "sites.readWrite.all",
        "team.readBasic.all",
        "channel.readBasic.all",
        "sites.manage.all",
        "presence.readWrite"        
    };

        #endregion

        #region Construct Authorization URL
        public async Task<string> GetAuthorizationCodeAsync()
        {
            try
            {
                // Construct Authorization URL
                string authUrl = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                                 $"?client_id={clientId}" +
                                 $"&response_type=code" +
                                 $"&redirect_uri={Uri.EscapeDataString(redirectUri)}" +
                                 $"&response_mode=query" +
                                 $"&scope={Uri.EscapeDataString(string.Join(" ", scopes))}" +
                                 $"&state=125";

                // Open the browser for user authentication
                var process = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = authUrl,
                        UseShellExecute = true
                    }
                };
                process.Start();

                // Set up a local HTTP listener to capture the redirect
                using (var listener = new HttpListener())
                {
                    listener.Prefixes.Add(redirectUri);
                    listener.Start();
                    Console.WriteLine("Waiting for authorization code...");
                    var context = await listener.GetContextAsync();

                    // Extract authorization code from the query string
                    string code = HttpUtility.ParseQueryString(context.Request.Url.Query).Get("code");
                    if (string.IsNullOrEmpty(code))
                    {
                        throw new Exception("Authorization code not received.");
                    }
                    Console.WriteLine($"Authorization code received: {code}");

                    // Respond to the browser
                    using (var writer = new StreamWriter(context.Response.OutputStream))
                    {
                        writer.WriteLine("<html><body>You can close this window now.</body></html>");
                    }
                    listener.Stop();

                    // Close the browser window after receiving the code
                    process.Kill();

                    return code;
                }
            }
            catch (HttpListenerException httpEx)
            {
                Console.WriteLine($"HTTP Listener error: {httpEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred: {ex.Message}");
                throw;
            }
        }

        #endregion

        #region Get Access Token and Refresh Token
        public async Task<(string AccessToken, string RefreshToken , string Expiry)> GetTokensAsync(string code)
        {
            try
            {
                using (var httpClient = new HttpClient())
                {
                    var tokenRequest = new FormUrlEncodedContent(new[]
                    {
                    new KeyValuePair<string, string>("client_id", clientId),
                    new KeyValuePair<string, string>("scope", string.Join(" ", scopes)),
                    new KeyValuePair<string, string>("code", code),
                    new KeyValuePair<string, string>("redirect_uri", redirectUri),
                    new KeyValuePair<string, string>("grant_type", "authorization_code"),
                    new KeyValuePair<string, string>("client_secret", clientSecret)
                });

                    HttpResponseMessage response = await httpClient.PostAsync(
                        $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token",
                        tokenRequest);

                    response.EnsureSuccessStatusCode(); // Throws an exception if not 200-299

                    string responseContent = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Token Response JSON:\n{responseContent}");

                    // Extract tokens from JSON response
                    var tokenData = System.Text.Json.JsonDocument.Parse(responseContent);
                    string accessToken = tokenData.RootElement.GetProperty("access_token").GetString();
                    string refreshToken = tokenData.RootElement.GetProperty("refresh_token").GetString();
                    var expiryElement = tokenData.RootElement.GetProperty("expires_in");
                    string expiry = expiryElement.ValueKind == System.Text.Json.JsonValueKind.String
                        ? expiryElement.GetString()
                        : expiryElement.GetInt32().ToString();

                    return (AccessToken: accessToken, RefreshToken: refreshToken,Expiry: expiry);
                }
            }
            catch (HttpRequestException httpEx)
            {
                Console.WriteLine($"HTTP request error: {httpEx.Message}");
                throw;
            }
            catch (System.Text.Json.JsonException jsonEx)
            {
                Console.WriteLine($"JSON parsing error: {jsonEx.Message}");
                throw;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while retrieving tokens: {ex.Message}");
                throw;
            }
        }
        #endregion

        //#region Save Tokens In Database
        //public async Task SaveTokensAsync()
        //{
        //    try
        //    {
        //        string code = await GetAuthorizationCodeAsync();
        //        var tokens = await GetTokensAsync(code);

        //        string accessToken = tokens.AccessToken;
        //        string refershToken = tokens.RefreshToken;
        //        Guid companyGuid = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
        //        // Map string values to DTO
        //        var messageAppListDto = new MessageAppListDTO
        //        {

        //            MAID = new Guid(),
        //            Company_ID = companyGuid,
        //            Group_ID = "IT",
        //            App_Nm = "Teams",
        //            App_Desc = "Teams",
        //            App_Channels = "appChannels",
        //            Access_Token_User_ID = accessToken,
        //            Access_Token_Txt = refershToken,
        //            Publish_Immd_Flag = true,
        //        };
        //        // Call the CreateMessageApp method directly
        //        await _messageAppListRepository.AddAsync(messageAppListDto);
        //    }
        //    catch (Exception ex)
        //    {
        //        Console.WriteLine($"An error occurred while saving tokens: {ex.Message}");
        //    }
        //}
        //#endregion
    }
}
