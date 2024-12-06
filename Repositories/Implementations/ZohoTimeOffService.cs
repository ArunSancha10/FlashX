using outofoffice.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using outofoffice.Repositories.Interfaces;
using Microsoft.Extensions.Options;
using MySqlX.XDevAPI;
using Newtonsoft.Json;
using outofoffice.Dto;

namespace outofoffice.Services
{
    public class ZohoTimeOffService : IZohoTimeOffService
    {
        private readonly ZohoFSMUserDetails _zohoFSMUserDetails;
        private readonly HttpClient Client;
        private readonly IHttpContextAccessor httpContextAccessor;
        private readonly IMessageAppListRepository _messageAppListRepository;

        public ZohoTimeOffService(IOptions<ZohoFSMUserDetails> zohoFSMUserDetails, HttpClient client, IHttpContextAccessor httpContextAccessor, IMessageAppListRepository messageAppListRepository)
        {
            _zohoFSMUserDetails = zohoFSMUserDetails.Value;
            Client = client;
            this.httpContextAccessor = httpContextAccessor;
            _messageAppListRepository = messageAppListRepository;
        }

        public async Task<string> UpdateTimeOffAsync(UpdateTimeOff timeoff, string accessToken, string ZohoTimeOffID, string usermail)
        {
            try
            {
                Debug.WriteLine("#####UpdateTimeOffAsync() -Start-#####");

                DateTime startdate = timeoff.startDate ?? throw new ArgumentNullException(nameof(timeoff.startDate), "Start date is required");
                DateTime enddate = timeoff.endDate ?? throw new ArgumentNullException(nameof(timeoff.endDate), "End date is required");
                string formattedStartDate;
                string formattedEndDate;
                string description = timeoff.Description;
                bool datewithTime = false;

                if(timeoff.Reason == "")
                {
                    timeoff.Reason = "Other work";
                }
                if(timeoff.TimeOffType == "" || timeoff.TimeOffType == "DateTime")
                {
                    timeoff.TimeOffType = "DateTime";
                    datewithTime = true;
                }

                if (datewithTime)
                {
                    formattedStartDate = startdate.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    formattedEndDate = enddate.ToString("yyyy-MM-ddTHH:mm:sszzz");
                }
                else
                {
                    formattedStartDate = startdate.ToString("yyyy-MM-dd");
                    formattedEndDate = enddate.ToString("yyyy-MM-dd");
                    //dateType = "Date";
                }

                string jsonBody = $@"
                {{
                    ""data"": [
                        {{
                            ""Time_Off_Type"": ""{timeoff.TimeOffType}"",
                            ""Start_Date_Time"": ""{formattedStartDate}"",
                            ""End_Date_Time"": ""{formattedEndDate}"",
                            ""Description"": ""{description}"",
                            ""Reason"": ""{timeoff.Reason}"",
                        }}
                    ]
                }}";

                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await Client.PutAsync($"https://fsm.zoho.in/fsm/v1/Time_Off/{ZohoTimeOffID}", content);
                var result = response.Content.ReadAsStringAsync().Result;

                if (response.IsSuccessStatusCode)
                {

                    JObject responseJson = JObject.Parse(result);
                    string status = responseJson["status"]?.ToString();
                    string message = responseJson["result"]?.ToString();

                    if (responseJson["status"]?.ToString() == "success")
                    {
                        string timeOffId = responseJson["data"]?["Time_Off"]?[0]?["id"]?.ToString();
                        Debug.WriteLine("#####UpdateTimeOffAsync() -End-#####");
                        return $"{timeOffId}";
                    }
                    else
                    {
                        string errorMessage = responseJson["message"]?.ToString();
                        Debug.WriteLine($"#####UpdateTimeOffAsync() -Error:{errorMessage}-#####");
                        return $"Error: {errorMessage}";
                    }
                }
                else
                {
                    string listType = "modules";
                    string accessTokenUser = await getAccessCode("Read", listType);

                    string Service_Resources_ID = await GetServiceResourceListAsync(accessTokenUser, usermail);


                    accessToken = await getAccessCode("Create", listType);

                    // Don't delete this Zoho Scheduler added -Start-
                    var updateTimeOffTemp = new UpdateTimeOff
                    {
                        Description = timeoff.Description,
                        endDate = timeoff.endDate,
                        startDate = timeoff.startDate,
                        Reason = timeoff.Reason,
                        TimeOffType = timeoff.TimeOffType,
                    };

                    return await CreateTimeOffAsync(updateTimeOffTemp, accessToken, Service_Resources_ID);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"#####UpdateTimeOffAsync() -Error:{ex.Message}-#####");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> getAccessCode(string scope, string type)
        {
            try
            {
                Debug.WriteLine("#####getAccessCode() -Start-#####");
                Debug.WriteLine("scope:- " + scope);
                string accessToken = "";
                var scopeType = scope.ToUpper();
                string finalScope = "";

                if (type == "modules")
                {
                    finalScope = _zohoFSMUserDetails.scope_modules + scopeType;
                }
                if (type == "users")
                {
                    finalScope = _zohoFSMUserDetails.scope_users + scopeType;
                }

                var data = new Dictionary<string, string>
                {
                    { "client_id", _zohoFSMUserDetails.client_id },
                    { "client_secret", _zohoFSMUserDetails.client_secret },
                    { "scope", finalScope },
                    { "grant_type", _zohoFSMUserDetails.grant_type }
                };

                var content = new FormUrlEncodedContent(data);
                var response = await Client.PostAsync("https://accounts.zoho.in/oauth/v2/token", content);

                if (response.IsSuccessStatusCode)
                {
                    var responseString = await response.Content.ReadAsStringAsync();
                    JObject jsonResponse = JObject.Parse(responseString);
                    accessToken = jsonResponse["access_token"]?.ToString();

                    if (!string.IsNullOrWhiteSpace(accessToken))
                    {
                        Debug.WriteLine("Access token is valid.");
                        Guid companyGuid = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");
                        var messageAppListDto = new MessageAppListDTO
                        {
                            MAID = new Guid(),
                            Company_ID = companyGuid,
                            Group_ID = "IT",
                            App_Nm = "Zoho",
                            App_Desc = "Zoho",
                            App_Channels = "appChannels",
                            Access_Token_User_ID = accessToken,
                            Access_Token_Txt = "oauthtoken",
                            Publish_Immd_Flag = true
                        };

                        await _messageAppListRepository.AddAsync(messageAppListDto);
                    }
                    else
                    {
                        Debug.WriteLine("Access token not found in the response.");
                    }
                }
                else
                {
                    Debug.WriteLine("Error: " + response.StatusCode);
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Debug.WriteLine("Error Response: " + errorResponse);
                }

                Debug.WriteLine("#####getAccessCode() -End-#####");
                return accessToken;
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"#####getAccessCode() -Error:{ex.Message}-#####");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> CreateTimeOffAsync(UpdateTimeOff timeoff, string accessToken, string Service_Resources_ID)
        {
            try
            {
                Debug.WriteLine("#####CreateTimeOffAsync() -Start-#####");

                DateTime startdate = timeoff.startDate ?? throw new ArgumentNullException(nameof(timeoff.startDate), "Start date is required");
                DateTime enddate = timeoff.endDate ?? throw new ArgumentNullException(nameof(timeoff.endDate), "End date is required");
                string formattedStartDate;
                string formattedEndDate;
                string description = timeoff.Description;
                bool datewithTime = false;

                if (string.IsNullOrEmpty(timeoff.Reason))
                {
                    timeoff.Reason = "Other work";
                }

                if (string.IsNullOrEmpty(timeoff.TimeOffType) || timeoff.TimeOffType == "DateTime")
                {
                    timeoff.TimeOffType = "DateTime";
                    datewithTime = true;
                }

                if (datewithTime)
                {
                    formattedStartDate = startdate.ToString("yyyy-MM-ddTHH:mm:sszzz");
                    formattedEndDate = enddate.ToString("yyyy-MM-ddTHH:mm:sszzz");
                }
                else
                {
                    formattedStartDate = startdate.ToString("yyyy-MM-dd");
                    formattedEndDate = enddate.ToString("yyyy-MM-dd");
                }

                string jsonBody = $@"
                {{
                    ""data"": [
                    {{
                        ""Service_Resource"": ""{Service_Resources_ID}"",
                        ""Time_Off_Type"": ""{timeoff.TimeOffType}"",
                        ""Start_Date_Time"": ""{formattedStartDate}"",
                        ""End_Date_Time"": ""{formattedEndDate}"",
                        ""Reason"": ""{timeoff.Reason}"",
                        ""Description"": ""{description}""
                    }}
                ]
                }}";

                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
                StringContent content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                var response = await Client.PostAsync("https://fsm.zoho.in/fsm/v1/Time_Off", content);
                string result = await response.Content.ReadAsStringAsync();
                JObject responseJson = JObject.Parse(result);
                string status = responseJson["status"]?.ToString();
                string message = responseJson["result"]?.ToString();

                if (responseJson["status"]?.ToString() == "success")
                {
                    string timeOffId = responseJson["data"]?["Time_Off"]?[0]?["id"]?.ToString();
                    Debug.WriteLine("#####CreateTimeOffAsync() -End-#####");
                    return $"{timeOffId}";
                }
                else
                {
                    string errorMessage = responseJson["message"]?.ToString();
                    Debug.WriteLine($"#####CreateTimeOffAsync() -Error:{errorMessage}-#####");
                    return $"Error: {errorMessage}";
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"#####CreateTimeOffAsync() -Error:{ex.Message}-#####");
                return $"Error: {ex.Message}";
            }
        }

        public async Task<string> GetServiceResourceListAsync(string accessToken, string emailID)
        {
            try
            {
                Debug.WriteLine("#####GetServiceResourceListAsync() -Start-#####");
                // Set authorization header
                Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                // Make the GET request to the API
                var response = await Client.GetAsync("https://fsm.zoho.in/fsm/v1/Time_Off");

                // Check if the response was successful
                if (!response.IsSuccessStatusCode)
                {
                    // Log the status code if not successful
                    Debug.WriteLine($"Request failed with status code: {response.StatusCode}");
                    return $"Error: Request failed with status code {response.StatusCode}.";
                }

                // Read the response content as a string
                string result = await response.Content.ReadAsStringAsync();


                if(result != "")
                {
                    // Parse the response as JSON
                    JObject responseJson = JObject.Parse(result);

                    // Extract the "data" array
                    JArray timeOffData = (JArray)responseJson["data"];

                    // Check if data exists
                    if (timeOffData != null)
                    {
                        // Loop through the "data" array
                        foreach (var item in timeOffData)
                        {
                            var Owner = item["Owner"];
                            if (Owner != null)
                            {
                                // Extract the email from the "Owner"
                                var resourceEmail = Owner["email"]?.ToString();

                                // Debug log the email being checked
                                Debug.WriteLine($"Checking email: {resourceEmail}");

                                // If the email matches, return the service resource ID
                                if (resourceEmail == emailID)
                                {
                                    Debug.WriteLine("#####GetServiceResourceListAsync() -End- #####");
                                    return item["Service_Resource"]["id"]?.ToString();
                                }
                            }
                        }
                        result = "";
                    }
                    else
                    {
                        result = "";
                    }
                }

                bool createNew = false;
                if (result == "")
                {
                    string listType = "users";
                    accessToken = await getAccessCode("Read", listType);
                    Client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

                    //var firstTimeOff = getAccessCode()
                    var firstTimeOff = await Client.GetAsync("https://fsm.zoho.in/fsm/v1/users");

                    if (!firstTimeOff.IsSuccessStatusCode)
                    {
                        // Log the status code if not successful
                        Debug.WriteLine($"Request failed with status code: {firstTimeOff.StatusCode}");
                        result = await firstTimeOff.Content.ReadAsStringAsync();
                        return $"Error: Request failed with status code {firstTimeOff.StatusCode}.";
                    }

                    // Read the response content as a string
                    result = await firstTimeOff.Content.ReadAsStringAsync();
                    createNew = true;
                }


                // Log the response for debugging purposes
                Debug.WriteLine($"Response: {result}");

                if (createNew)
                {
                    // Parse the response as JSON
                    JObject responseJsonNewUser = JObject.Parse(result);

                    // Extract the "data" array
                    JArray timeOffData = (JArray)responseJsonNewUser["users"];

                    // Check if data exists
                    if (timeOffData != null)
                    {
                        // Loop through the "data" array
                        foreach (var item in timeOffData)
                        {
                            var resourceEmail = item["email"]?.ToString();
                            
                                // If the email matches, return the service resource ID
                                if (resourceEmail == emailID)
                                {
                                    Debug.WriteLine("#####GetServiceResourceListAsync() -End- #####");
                                    return item["Service_Resources"]["id"]?.ToString();
                                }
                            
                        }

                        Debug.WriteLine("#####GetServiceResourceListAsync() Error: No Service Resource found with email #####");
                        return $"Error: No Service Resource found with email {emailID}";
                    }
                    else
                    {
                        Debug.WriteLine("#####GetServiceResourceListAsync() Error: No data found in response #####");
                        return "Error: No data found in response.";
                    }
                }
                else
                {
                    Debug.WriteLine("#####GetServiceResourceListAsync() Error: No data found in response #####");
                    return "Error: No data found in response.";
                }

            }
            catch (Exception ex)
            {
                // Catch any exceptions that occur during the process
                Debug.WriteLine($"#####GetServiceResourceListAsync() -Error:{ex.Message}-#####");
                return $"Error: An exception occurred - {ex.Message}";
            }
        }

    }
}
