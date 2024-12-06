using Microsoft.Graph.Models;
using outofoffice.Models;

namespace outofoffice.Repositories.Interfaces
{
    public interface IExchangeClient
    {
        Task<HttpResponseMessage> SetAutomaticRepliesAsync(string accessToken, object automaticRepliesSetting);
        Task<HttpResponseMessage> SetWorkingHoursAsync(string accessToken, string endTime, string[] daysOfWeek, object timeZoneDetails);

        Task<HttpResponseMessage> GetSites(string accessToken, string siteId, string listId);

        Task<HttpResponseMessage> UpdateItemFieldsAsync(
                                                        string accessToken,
                                                        string siteId,
                                                        string listId,
                                                        string itemId,
                                                        Dictionary<string, object> fields);
    }
}
