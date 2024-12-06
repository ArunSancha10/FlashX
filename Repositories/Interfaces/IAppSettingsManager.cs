
namespace outofoffice.Repositories.Interfaces
{
    public interface IAppSettingsManager
    {
        Task UpdateAppSettingsAsync(string key, string value);
        Task<string> GetAppSettingAsync(string key);
    }
}
