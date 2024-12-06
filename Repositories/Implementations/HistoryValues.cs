using Microsoft.EntityFrameworkCore.Migrations;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using outofoffice.App_code;
using outofoffice.Models;
using outofoffice.Repositories.Interfaces;

namespace outofoffice.Repositories.Implementations
{
    public class HistoryValues : IHistoryValues
    {
        private readonly HttpClient _httpClient;

        public HistoryValues(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<List<OutOfOfficeHistory>> GetHistoryValues(string apiUrl)
        {
            HttpResponseMessage response = await _httpClient.GetAsync(apiUrl);
            if (response.IsSuccessStatusCode)
            {
                var jsonResponse = await response.Content.ReadAsStringAsync();
                JArray jsonArray = JArray.Parse(jsonResponse); // Parse response as JArray

                var historyData = new List<OutOfOfficeHistory>();

                foreach (var item in jsonArray)
                {
                    var itemObj = item as JObject;
                    if (itemObj != null)
                    {
                        var appsToPublishToken = itemObj["apps_To_Publish"];
                        var publishStatus = itemObj["publish_Status"].ToString();

                        if (appsToPublishToken.Type == JTokenType.String)
                        {
                            var appsToPublishString = appsToPublishToken.ToString();
                            AddApplicationHistory(appsToPublishString, itemObj, historyData, publishStatus);
                        }
                        else if (appsToPublishToken.Type == JTokenType.Array)
                        {
                            foreach (var appItem in appsToPublishToken)
                            {
                                var appItemObj = appItem as JObject;
                                if (appItemObj != null)
                                {
                                    AddApplicationHistory(appItemObj.ToString(), itemObj, historyData, publishStatus);
                                }
                                else
                                {
                                    throw new InvalidCastException("Expected JObject but got a different type.");
                                }
                            }
                        }
                    }
                    else
                    {
                        throw new InvalidCastException("Expected JObject but got a different type.");
                    }
                }

                // Sort historyData by 'Created_Date' in descending order
                historyData = historyData
                    .OrderByDescending(h => h.CreatedDate)
                    .ToList();
                return historyData;
            }
            else
            {
                throw new Exception($"Failed to fetch data. Status code: {response.StatusCode}");
            }
        }

        private void AddApplicationHistory(string appsToPublishString, JObject itemObj, List<OutOfOfficeHistory> historyData, string publishStatus)
        {
            // Parse the JSON string to a JArray
            var appsArray = JArray.Parse(appsToPublishString);

            // Loop through the array and extract the application data
            foreach (var appItem in appsArray)
            {
                var applicationValue = appItem["Application"]?.ToString() ?? string.Empty;

                // Extract the publish schedule details dynamically from the appItem
                var publishSchedule = appItem["Publish_Schedule"];
                var status = publishStatus;
                var message = appItem["Message"]?.ToString() ?? string.Empty;
                var fromDate = publishSchedule?["From_Date"]?.ToString() ?? string.Empty;
                var toDate = publishSchedule?["To_Date"]?.ToString() ?? string.Empty;
                var orderBy = appItem?["Created_Date"]?.ToString() ?? string.Empty;

                // Create the history object dynamically based on the application value
                var history = new OutOfOfficeHistory
                {
                    Application = applicationValue,
                    Status = status,
                    StartDate = fromDate,
                    EndDate = toDate,
                    Message = message,
                    CreatedDate = orderBy
                };

                // Add the history object to the list
                historyData.Add(history);
            }
        }
    }
}
