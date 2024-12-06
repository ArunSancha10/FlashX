using outofoffice.Models;

namespace outofoffice.Repositories.Interfaces
{
    public interface IZohoTimeOffService
    {
        Task<string> UpdateTimeOffAsync(UpdateTimeOff timeoff, string AccessToken, string ZohoTimeOffID, string usermail);
        Task<string> getAccessCode(string scope, string type);
        Task<string> CreateTimeOffAsync(UpdateTimeOff timeoff, string accessToken, string Service_Resources_ID);
        Task<string> GetServiceResourceListAsync(string accessToken, string emailID);
    }
}
