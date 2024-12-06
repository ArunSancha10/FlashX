using Newtonsoft.Json;
using outofoffice.Repositories.Interfaces;

namespace outofoffice.Repositories.Implementations
{
    public class AppSettingsManager : IAppSettingsManager
    {
        private readonly string _appSettingsFilePath;
        private readonly IConfiguration _configuration;

        public AppSettingsManager(IConfiguration configuration)
        {
            // Get the path of the appsettings.json file
            _appSettingsFilePath = Path.Combine(Directory.GetCurrentDirectory(), "appsettings.json");
            _configuration = configuration;
        }

        // Method to add or update a setting in appsettings.json
        public async Task UpdateAppSettingsAsync(string key, string value)
        {
            // Read the existing content of appsettings.json
            var json = File.ReadAllText(_appSettingsFilePath);
            dynamic jsonObj = JsonConvert.DeserializeObject(json);

            // Add or update the value
            jsonObj.AppSettings[key] = value;

            // Serialize the updated object and write it back to the file
            string output = JsonConvert.SerializeObject(jsonObj, Formatting.Indented);
            File.WriteAllText(_appSettingsFilePath, output);
        }

        public async Task<string> GetAppSettingAsync(string key)
        {
            return _configuration.GetValue<string>($"AppSettings:{key}");
        }
    }
}
