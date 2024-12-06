using Microsoft.Graph;
using Microsoft.Graph.Me.Presence.SetStatusMessage;
using Microsoft.Graph.Me.Presence.SetUserPreferredPresence;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using System;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using outofoffice.Models;
using System.Net.Http.Headers;
using outofoffice.Repositories.Interfaces;
using System.Text.Json.Nodes;



namespace outofoffice.Service
{
    public class TeamsServices
    {

        #region Global Variabels

        private readonly GraphServiceClient _graphServiceClient;

        private readonly TeamTokenResponse _teamTokenResponse;

        private readonly HttpClient _httpClient;

        public string accessToken = null;

        private readonly IConfiguration _configuration;

        #endregion

        #region Constructor to initialize GraphServiceClient.
        public TeamsServices(GraphServiceClient graphServiceClient, TeamTokenResponse teamTokenResponse, HttpClient httpClient, IConfiguration configuration)
        {
            _graphServiceClient = graphServiceClient;
            _teamTokenResponse = teamTokenResponse;
            _httpClient = httpClient;
            _configuration = configuration;
        }
        #endregion


        #region Gendric Method For call All P0st Methods

        public async Task<string> CallApis(string availability, string activity, string expiry, string StatuMsg, string ChannalMsg, string Channals, string Teams_Token, string Teams_RefreshToken, string user_ID)
        {

            var newAccessToken = await GetAccessTokenUsingRefreshToken(Teams_RefreshToken);

            if (!string.IsNullOrEmpty(newAccessToken))
            {
                Console.WriteLine("New Access Token: " + newAccessToken);
                Teams_Token = newAccessToken;
            }

            #region Call SetPresence Method
            if (!string.IsNullOrEmpty(availability) &&
                !string.IsNullOrEmpty(activity) &&
                !string.IsNullOrEmpty(expiry) &&
                !string.IsNullOrEmpty(Teams_Token))
            {
                await SetUserPresenceAsync(availability, activity, expiry, Teams_Token, user_ID);
            }
            #endregion

            #region Call Set Status Method

            if (!string.IsNullOrEmpty(StatuMsg) &&
                !string.IsNullOrEmpty(Teams_Token))
            {
                await SetStatusMessageAsync(Teams_Token, StatuMsg,user_ID);
            }
            #endregion

            #region Call Send Message To Channal Method
            if (!string.IsNullOrEmpty(ChannalMsg) &&
                !string.IsNullOrEmpty(Channals) &&
               !string.IsNullOrEmpty(Teams_Token))
            {
                //Channals = JsonConvert.SerializeObject(Channals);
                await SendMessagesToChannels(Channals, ChannalMsg, Teams_Token);
            }
            #endregion
            return "Teams updated successfully.";
        }

        #endregion

        // Post Methods

        #region [POST] Method to set the user's presence status in Microsoft Teams.
        public async Task SetUserPresenceAsync(string availability, string activity, string expiry, string Teams_Token, string user_ID)
        {
            Console.WriteLine("SetUserPresenceAsync");

            // API URL for setting presence
            string url = "https://graph.microsoft.com/v1.0/users/$$User_ID$$/presence/setUserPreferredPresence";

            url = url.Replace("$$User_ID$$", user_ID);


            // HTTP Client
            using (var client = new HttpClient())
            {
                // Set Authorization Header
                client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Teams_Token);

                // Request payload
                var requestBody = new
                {
                    sessionId = "370fef53-0f51-426b-970b-31b21f16dd15",// Guid.NewGuid().ToString(), // Unique session ID
                    availability,
                    activity,
                    expirationDuration = "PT1H" // ISO 8601 duration (1 hour)
                };

                // Serialize payload
                string jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);

                // Create HTTP content
                var content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                try
                {
                    // Send POST request
                    var response = await client.PostAsync(url, content);

                    // Check response status
                    if (response.IsSuccessStatusCode)
                    {
                        Console.WriteLine("Presence updated successfully!");
                    }
                    else
                    {

                        string errorDetails = await response.Content.ReadAsStringAsync();
                        Console.WriteLine($"Error updating presence: {response.StatusCode} - {errorDetails}");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Exception: {ex.Message}");
                }
            }

        }
        #endregion

        #region [POST] Method to set a custom status message for the user.
        public async Task SetStatusMessageAsync(string accessToken, string message,string user_ID)
        {
            Console.WriteLine("SetStatusMessageAsync");
            var requestUrl = $"https://graph.microsoft.com/v1.0/users/$$user_id$$/presence/setStatusMessage";
            //$"https://graph.microsoft.com/v1.0/users/{userId}/presence/microsoft.graph.setStatusMessage";
            requestUrl = requestUrl.Replace("$$user_id$$", user_ID);
            var requestBody = new
            {
                statusMessage = new
                {
                    message = new
                    {
                        content = message,
                        contentType = "text" // Set as plain text
                    }
                }
            };

            var jsonContent = JsonConvert.SerializeObject(requestBody);
            var httpRequest = new HttpRequestMessage(HttpMethod.Post, requestUrl)
            {
                Content = new StringContent(jsonContent, Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.SendAsync(httpRequest);
            if (response.IsSuccessStatusCode)
            {
                Console.WriteLine("Status message updated successfully.");
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error: {response.StatusCode}, {error}");
            }
        }
        #endregion

        #region [POST] Method to send messages to multiple channels based on input format
        public async Task SendMessagesToChannels(string Channals, string messageContent, string accessToken)
        {
            // Deserialize the JSON array into a list of team-channel identifiers
            var teamChannelList = System.Text.Json.JsonSerializer.Deserialize<List<string>>(Channals);

            using (HttpClient client = new HttpClient())
            {
                // Set the Authorization header
                client.DefaultRequestHeaders.Authorization =
                    new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", accessToken);

                // Loop through each item in the list (team:channel format)
                foreach (var teamChannel in teamChannelList)
                {
                    // Split the input string using "CYGTEAM" as the separator
                    var parts = teamChannel.Split(new string[] { "CYGTEAM" }, StringSplitOptions.None);

                    // Ensure the parts contain exactly two parts (TeamId and ChannelId)
                    if (parts.Length != 2)
                    {
                        Console.WriteLine($"Invalid team-channel format: {teamChannel}");
                        continue;
                    }

                    string teamId = parts[0].Trim();  // Left side: Team ID
                    string channelId = parts[1].Trim();  // Right side: Channel ID

                    // Create the message content
                    var requestBody = new
                    {
                        body = new
                        {
                            content = messageContent
                        }
                    };

                    // Serialize the request body to JSON
                    string jsonPayload = System.Text.Json.JsonSerializer.Serialize(requestBody);

                    // API endpoint for posting messages to a channel
                    string url = $"https://graph.microsoft.com/v1.0/teams/{teamId}/channels/{channelId}/messages";

                    try
                    {
                        // Send the POST request
                        var response = await client.PostAsync(url, new StringContent(jsonPayload, Encoding.UTF8, "application/json"));

                        if (response.IsSuccessStatusCode)
                        {
                            Console.WriteLine($"Message sent to Team: {teamId}, Channel: {channelId}");
                        }
                        else
                        {
                            string errorDetails = await response.Content.ReadAsStringAsync();
                            Console.WriteLine($"Error sending message to Team {teamId}, Channel {channelId}: {response.StatusCode} - {errorDetails}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Exception while sending message to Team {teamId}, Channel {channelId}: {ex.Message}");
                    }
                }
            }

        }
        #endregion

        // GetMethods

        #region [GET] Method to get all teams the user is part of and store their IDs in an array
        public async Task<string[]> GetAllTeamIdsAsync(string Teams_Token)
        {
            try
            {
                var getteamsurl = "https://graph.microsoft.com/v1.0/me/joinedTeams";

                using (HttpClient httpClient = new HttpClient())
                {
                    // Set the Authorization header with a valid bearer token
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Teams_Token);

                    // Make the GET request to the Microsoft Graph API
                    HttpResponseMessage response = await httpClient.GetAsync(getteamsurl);

                    if (!response.IsSuccessStatusCode)
                    {
                        Console.WriteLine($"Failed to fetch teams. Status Code: {response.StatusCode}");
                        return Array.Empty<string>(); // Return an empty array if the request fails
                    }

                    // Parse the response JSON
                    var responseContent = await response.Content.ReadAsStringAsync();
                    using var document = JsonDocument.Parse(responseContent);
                    var teamIds = document.RootElement
                                           .GetProperty("value")
                                           .EnumerateArray()
                                           .Select(team => team.GetProperty("id").GetString())
                                           .ToArray();

                    return teamIds;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred while fetching teams: {ex.Message}");
                return Array.Empty<string>(); // Return empty array if an error occurs
            }
        }

        #endregion

        #region [GET] Method to get all teams and Channel the user is part of and store their IDs in an array
        public async Task<Dictionary<string, string>> GetAllChannelsForTeamsAsync(string Teams_Token)
        {
            try
            {
                // Fetch all team IDs
                string[] teamIds = await GetAllTeamIdsAsync(Teams_Token);

                if (teamIds == null || teamIds.Length == 0)
                {
                    Console.WriteLine("No teams found for the user.");
                    return new Dictionary<string, string>(); // Return empty dictionary if no teams are found
                }

                var teamChannelDictionary = new Dictionary<string, string>();
                var baseGraphUrl = "https://graph.microsoft.com/v1.0";

                using (HttpClient httpClient = new HttpClient())
                {
                    httpClient.DefaultRequestHeaders.Authorization =
                        new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", Teams_Token);

                    foreach (var teamId in teamIds)
                    {
                        try
                        {
                            // Fetch team details to get the team name
                            var teamUrl = $"{baseGraphUrl}/teams/{teamId}";
                            var teamResponse = await httpClient.GetAsync(teamUrl);

                            if (!teamResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Failed to fetch team details for Team ID: {teamId}. Status Code: {teamResponse.StatusCode}");
                                continue; // Skip this team if the request fails
                            }

                            var teamContent = await teamResponse.Content.ReadAsStringAsync();
                            using var teamDocument = JsonDocument.Parse(teamContent);
                            string teamName = teamDocument.RootElement.GetProperty("displayName").GetString();

                            // Fetch channels for the team
                            var channelsUrl = $"{baseGraphUrl}/teams/{teamId}/channels";
                            var channelsResponse = await httpClient.GetAsync(channelsUrl);

                            if (!channelsResponse.IsSuccessStatusCode)
                            {
                                Console.WriteLine($"Failed to fetch channels for Team ID: {teamId}. Status Code: {channelsResponse.StatusCode}");
                                continue; // Skip this team if the request fails
                            }

                            var channelsContent = await channelsResponse.Content.ReadAsStringAsync();
                            using var channelsDocument = JsonDocument.Parse(channelsContent);

                            foreach (var channel in channelsDocument.RootElement.GetProperty("value").EnumerateArray())
                            {
                                string channelId = channel.GetProperty("id").GetString();
                                string channelName = channel.GetProperty("displayName").GetString();

                                // Add to dictionary
                                var key = $"{teamId}CYGTEAM{channelId}";
                                var value = $"{teamName}/{channelName}";
                                teamChannelDictionary[key] = value;
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error occurred while processing Team ID {teamId}: {ex.Message}");
                        }
                    }
                }

                return teamChannelDictionary; // Return the dictionary of teams and channels
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error occurred in GetAllChannelsForTeamsAsync: {ex.Message}");
                return new Dictionary<string, string>(); // Return an empty dictionary in case of an error
            }
        }
        #endregion

        #region [GET] Get Access Token Using RefreshToken
        public async Task<string> GetAccessTokenUsingRefreshToken(string Teams_RefreshToken)
        {
            var clientId = _configuration["AzureAd:ClientId"];
            var ClientSecret = _configuration["AzureAd:ClientSecret"];
            using (HttpClient client = new HttpClient())
            {
                // Prepare the request body
                var requestBody = new StringContent(
                    $"client_id={clientId}" +
                    $"&client_secret={ClientSecret}" +
                    $"&refresh_token={Teams_RefreshToken}" +
                    $"&grant_type=refresh_token",
                    Encoding.UTF8, "application/x-www-form-urlencoded");

                try
                {
                    string tenantId = "common";
                    string tokenEndpoint = $"https://login.microsoftonline.com/{tenantId}/oauth2/v2.0/token";
                    // Send POST request
                    var response = await client.PostAsync(tokenEndpoint, requestBody);

                    if (response.IsSuccessStatusCode)
                    {
                        // Parse the response JSON
                        var responseJson = await response.Content.ReadAsStringAsync();
                        var tokenResponse = System.Text.Json.JsonSerializer.Deserialize<TeamTokenResponse>(responseJson);

                        // Return the new access token
                        return tokenResponse?.access_token;
                    }
                    else
                    {
                        Console.WriteLine("Error: " + await response.Content.ReadAsStringAsync());
                        return null;
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    return null;
                }
            }
        }
        #endregion


    }
}