using Microsoft.Graph.Models;
using Newtonsoft.Json;
using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using System.Net.Http.Headers;
using System.Text;

namespace outofoffice.Repositories.Implementations
{
    public class ExchangeClient : IExchangeClient
    {
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://graph.microsoft.com/v1.0";

        public ExchangeClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
        }

        public async Task<HttpResponseMessage> SetAutomaticRepliesAsync(string accessToken, object automaticRepliesSetting)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.PatchAsJsonAsync(BaseUrl + "/me/mailboxSettings", automaticRepliesSetting);
            return response;
        }

        public async Task<HttpResponseMessage> SetWorkingHoursAsync(string accessToken, string endTime, string[] daysOfWeek, object timeZoneDetails)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var payload = new
            {
                workingHours = new
                {
                    endTime,
                    daysOfWeek,
                    timeZone = timeZoneDetails
                }
            };

            var response = await _httpClient.PatchAsJsonAsync(BaseUrl, payload);
            return response;
        }

        public async Task<HttpResponseMessage> GetSites(string accessToken,string siteId , string listId)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var response = await _httpClient.GetAsync(BaseUrl + $"/sites/{siteId}/lists/{listId}/items?$expand=fields");

            return response;
        }

        public async Task<HttpResponseMessage> UpdateItemFieldsAsync(
                                                        string accessToken,
                                                        string siteId,
                                                        string listId,
                                                        string itemId,
                                                        Dictionary<string, object> fields)
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);

            var url = $"{BaseUrl}/sites/{siteId}/lists/{listId}/items/{itemId}/fields";
            var jsonContent = new StringContent(JsonConvert.SerializeObject(fields), Encoding.UTF8, "application/json");

            var request = new HttpRequestMessage(HttpMethod.Patch, url)
            {
                Content = jsonContent
            };

            var response = await _httpClient.SendAsync(request);


            return response;
        }
    }
}
