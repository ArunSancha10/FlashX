using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using outofoffice.App_code;
using outofoffice.Dto;
using outofoffice.Graph;
using outofoffice.Helper;
using outofoffice.Models;
using System.Net.Http.Headers;

namespace outofoffice.Repositories.Implementations
{
    public class AuthService
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly OOODbContext _dbContext;
        private readonly IMapper _mapper;

        public AuthService(IConfiguration configuration, HttpClient httpClient, IHttpContextAccessor httpContextAccessor, OOODbContext dbContext, IMapper mapper)
        {
            _configuration = configuration;
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _dbContext = dbContext;
            _mapper = mapper;
        }

        public async Task<string> GetAuthorizationUrl(string state)
        {
            var clientId = _configuration["AzureAd:ClientId"];
            var redirectUri = _configuration["AzureAd:RedirectUri"];
            var tenantId = /*"cb27f668-adbd-4d21-9a6b-d45b41977e5d"*/_configuration["AzureAd:TenantId"];
            var scopes = String.Join(",",GraphConstants.Scopes).Replace(",", " ");
            //string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");
            var request = _httpContextAccessor?.HttpContext?.Request;

            redirectUri = $"{request.Scheme}://{request.Host}{redirectUri}";


            return $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/authorize" +
                   $"?client_id={clientId}" +
                   $"&response_type=code" +
                   $"&redirect_uri={Uri.EscapeDataString($"{redirectUri}")}" +
                   $"&response_mode=query" +
                   $"&scope={Uri.EscapeDataString(scopes)}" +
                   $"&state={state}";
        }

        public async Task<TokenResponse> GetTokenAsync(string code)
        {
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            var redirectUri = _configuration["AzureAd:RedirectUri"];
            var tenantId = _configuration["AzureAd:TenantId"];

            var request = _httpContextAccessor?.HttpContext?.Request;

            redirectUri = $"{request.Scheme}://{request.Host}{redirectUri}";

            //string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");

            var content = new FormUrlEncodedContent(new[]
            {
                new KeyValuePair<string, string>("client_id", clientId),
                new KeyValuePair<string, string>("client_secret", clientSecret),
                new KeyValuePair<string, string>("redirect_uri", redirectUri),
                new KeyValuePair<string, string>("grant_type", "authorization_code"),
                new KeyValuePair<string, string>("code", code)
            });

            var response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", content);
            //response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(json);
        }

        public async Task<TokenResponse> RefreshTokenAsync(string refreshToken)
        {
            var clientId = _configuration["AzureAd:ClientId"];
            var clientSecret = _configuration["AzureAd:ClientSecret"];
            var tenantId = _configuration["AzureAd:TenantId"];

            var content = new FormUrlEncodedContent(new[]
            {
            new KeyValuePair<string, string>("client_id", clientId),
            new KeyValuePair<string, string>("client_secret", clientSecret),
            new KeyValuePair<string, string>("grant_type", "refresh_token"),
            new KeyValuePair<string, string>("refresh_token", refreshToken)
        });

            var response = await _httpClient.PostAsync($"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token", content);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<TokenResponse>(json);
        }

        public async Task<string> GetAccessTokenAsync()
        {
            var session = _httpContextAccessor.HttpContext.Session;

            // Check if access token exists and is valid
            var accessToken = session.GetString("AccessToken");
            var expiry = session.Gets<DateTime>("TokenExpiry");

            if (!string.IsNullOrEmpty(accessToken) && DateTime.UtcNow < expiry)
            {
                return accessToken;
            }

            var refreshToken = session.GetString("RefreshToken");

            var userMail = _httpContextAccessor.HttpContext.Session.GetString("GraphMail");

            if (string.IsNullOrWhiteSpace(userMail))
            {
                throw new UnauthorizedAccessException("User needs to log in again.");

            }
            var user = await _dbContext.MessageAppLists.FirstOrDefaultAsync(u => u.UserEmail.Trim().ToLower() == userMail.Trim().ToLower());

            if(string.IsNullOrEmpty(refreshToken))
            {
                refreshToken = user?.Access_Token_Txt ?? throw new UnauthorizedAccessException("User needs to log in again.");
            }

            var tokenResponse = await RefreshTokenAsync(refreshToken);

            // Save new tokens in session
            session.SetString("AccessToken", tokenResponse.AccessToken);
            session.SetString("RefreshToken", tokenResponse.RefreshToken);
            session.Sets("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(tokenResponse.ExpiresIn)));

            user.Access_Token_User_ID = tokenResponse.AccessToken;
            user.Access_Token_Txt = tokenResponse.RefreshToken;

            await _dbContext.SaveChangesAsync();

            return tokenResponse.AccessToken;
        }

        public async Task<UserProfile> CallMicrosoftGraphAsync(string accessToken)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            var response = await _httpClient.GetAsync("https://graph.microsoft.com/v1.0/me");
            var profile = await response.Content.ReadFromJsonAsync<UserProfile>();
            response.EnsureSuccessStatusCode();

            return profile;
        }

        public async Task AddOrUpdateAsync(MessageAppListDTO messageAppList)
        {
            var user = await _dbContext.MessageAppLists.FirstOrDefaultAsync(u => u.UserEmail == messageAppList.UserEmail);



            if (user == null)
            {
                var entity = _mapper.Map<MessageAppList>(messageAppList);
                await _dbContext.MessageAppLists.AddAsync(entity);
            }
            else
            {
                user.UserEmail = messageAppList.UserEmail;
                user.Access_Token_User_ID = messageAppList.Access_Token_User_ID;
                user.Access_Token_Txt = messageAppList.Access_Token_Txt;
            }

            await _dbContext.SaveChangesAsync();
        }
    }
}
public class TokenResponse
{
    [JsonProperty("token_type")]
    public string TokenType { get; set; }

    [JsonProperty("scope")]
    public string Scope { get; set; }

    [JsonProperty("expires_in")]
    public string ExpiresIn { get; set; }

    [JsonProperty("ext_expires_in")]
    public string ExtExpiresIn { get; set; }

    [JsonProperty("access_token")]
    public string AccessToken { get; set; }

    [JsonProperty("refresh_token")]
    public string RefreshToken { get; set; }
}

public class UserProfile
{
    public string Id { get; set; }
    public string DisplayName { get; set; }
    public string Mail { get; set; }
    public string UserPrincipalName { get; set; }
}


