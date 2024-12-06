using outofoffice.Models;
using System.Diagnostics;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Net;
using outofoffice.Repositories.Interfaces;
using Newtonsoft.Json;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Text.Json;
using Microsoft.Extensions.Options;


namespace outofoffice.Servicess
{
    public class SlackService : ISlackService
    {
        //private readonly HttpClient _httpClient = new HttpClient();
        private readonly slackAppConfig _slackConfig;
        private readonly HttpClient _httpClient = new HttpClient();
        //private readonly HttpClient Client;

        public SlackService(IOptions<slackAppConfig> slackConfig)
        {
            _slackConfig = slackConfig.Value;



        }



        public async Task<string> update_Profilestatus( string sts, string presence, string expiry, string slack_message, string slack_channels, string emoji, string token, string endDate)
        {
            Console.WriteLine("**update_Profilestatus**");

            // status update code
            var url = "https://slack.com/api/users.profile.set";

            var endTime = new DateTimeOffset(DateTime.Parse(endDate)).ToUnixTimeSeconds();
            var payload = new
            {
                profile = new
                {
                    status_text = sts,
                    status_emoji = emoji,
                    status_expiration = endTime
                }
            };

            // // slack message sent - start
            // string presence = "auto";
            // //string channels = await getChannels(token);
             string presence_sts = await SetPresenceAsync(presence, token);
            string send_msg = await Send_message(slack_message, slack_channels, token);

            // // slack message sent
            // string userId = "C08001CLM0A";
            //// string message = await Send_message(userId, token, timeOff.Description);

            // var url = "https://slack.com/api/chat.postMessage";
            // var payload = new
            // {
            //     channel = userId,
            //     text = timeOff.Description
            // };
            // // Set up the request
            // var request = new HttpRequestMessage(HttpMethod.Post, url);
            // request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
            // request.Content = JsonContent.Create(payload);

            // // Send the request
            // var response = await _httpClient.SendAsync(request);
            // return await HandleResponse(response, "Message sent successfully.");
            // // slack message sent - end


            // status update code start
            var request = new HttpRequestMessage(HttpMethod.Post, url)
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token) },
                Content = JsonContent.Create(payload)
            };

            var response = await _httpClient.SendAsync(request);
            return await HandleResponse(response, "Slack status updated successfully.");
            // status update code end
        }
        private async Task<string> HandleResponse(HttpResponseMessage response, string successMessage)
        {
            if (response.IsSuccessStatusCode)
            {
                return successMessage;
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return $"Failed to update Slack status. Error: {errorContent}";
            }
        }

        private async Task<string> SetPresenceAsync(string presence, string token)
        {

            var presenceUrl = "https://slack.com/api/users.setPresence";
            var presencePayload = new
            {
                presence = presence // or "away" depending on your requirement
            };

            var presenceRequest = new HttpRequestMessage(HttpMethod.Post, presenceUrl)
            {
                Headers = { Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token) },
                Content = JsonContent.Create(presencePayload)
            };

            var presenceResponse = await _httpClient.SendAsync(presenceRequest);
            await HandleResponse(presenceResponse, "Slack presence status updated successfully.");

            return "Profile and presence updated successfully.";
        }

        private async Task<string> Send_message(string sts, string channels, string token)
        {
            // // slack message sent
            
            // string message = await Send_message(userId, token, timeOff.Description);
            string[] slack_channels = channels.Split(',');
            foreach (string channel in slack_channels)
            {

                string userId = channel;
                var url = "https://slack.com/api/chat.postMessage";
                var payload = new
                {
                    channel = userId,
                    text = sts
                };
                // Set up the request
                var request = new HttpRequestMessage(HttpMethod.Post, url);
                request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);
                request.Content = JsonContent.Create(payload);

                // Send the request
                var response = await _httpClient.SendAsync(request);
                //return await HandleResponse(response, "Message sent successfully.");
                // // slack message sent - end
            }
            return "Message sent successfully.";
        }

        public async Task<Dictionary<string, string>> getChannels(string token)
        {
            var url = "https://slack.com/api/conversations.list";
            using var client = new HttpClient();

            client.DefaultRequestHeaders.Authorization =
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

            var response = await client.GetAsync(url);
            var content = await response.Content.ReadAsStringAsync();
            var jsonDoc = JsonDocument.Parse(content);
            var channels = jsonDoc.RootElement.GetProperty("channels");
            var channelDict = new Dictionary<string, string>();
            foreach (var channel in channels.EnumerateArray())
            {
                var channelId = channel.GetProperty("id").GetString();
                var channelName = channel.GetProperty("name").GetString();
                channelDict.Add(channelId, channelName);
                Console.WriteLine($"Channel ID: {channelId}, Name: {channelName}");
            }
           
            return channelDict;
        }

    }
}
