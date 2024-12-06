using Microsoft.AspNetCore.Mvc;
using outofoffice.Servicess;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text;
using outofoffice.Models;
using System.IO;
using outofoffice.Dto;
using System.ComponentModel.Design;
using System.Net.Http;
using outofoffice.Repositories.Interfaces;


namespace outofoffice.Controllers
{
    //[Route("ex")]
    public class SlackController : Controller
    {

        private readonly IMessageAppListRepository _messageAppListRepository;

        public readonly ISlackService sslackService;

        private readonly slackAppConfig _slackConfig;

        private readonly IAppSettingsManager _appSettingsManager;
        private readonly IConfiguration _configuration;


        public SlackController(IMessageAppListRepository messageAppListController, ISlackService sslackService, IOptions<slackAppConfig> slackConfig, IAppSettingsManager appSettingsManager, IConfiguration configuration)
        {

            _messageAppListRepository = messageAppListController;
            this.sslackService = sslackService;
            _slackConfig = slackConfig.Value;
            _appSettingsManager = appSettingsManager;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> OAuthurl(UpdateTimeOff updateTimeOff)
        {
            string clientId = _slackConfig.Client_Id;
            string clientSecret = _slackConfig.Client_Secret;
            string RedirectURI = _slackConfig.Redirect_Uri;
            string scope = _slackConfig.Scopes;
            string user_ID = _slackConfig.User_Id;

            // Serialize the model properties as query parameters
            var queryString = $"?startDate={Uri.EscapeDataString(updateTimeOff.startDate?.ToString("o") ?? string.Empty)}" +
                              $"&endDate={Uri.EscapeDataString(updateTimeOff.endDate?.ToString("o") ?? string.Empty)}" +
                              $"&Id={Uri.EscapeDataString(user_ID ?? string.Empty)}" +
                              $"&Description={Uri.EscapeDataString(updateTimeOff.Description ?? string.Empty)}";

            // Store the query string temporarily in session
            HttpContext.Session.SetString("TimeOffQueryString", queryString);

            string oauthUrl = $"https://slack.com/oauth/v2/authorize?client_id={clientId}&redirect_uri={RedirectURI}&scope={scope}";

            return Redirect(oauthUrl);
        }

        [HttpGet]
        public async Task<IActionResult> stackAuthUrl()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Accesscode(string code)
        {


            if (string.IsNullOrEmpty(code))
            {
                return BadRequest("No authorization code received.");
            }

            // Retrieve the stored query string from the session
            var queryString = HttpContext.Session.GetString("TimeOffQueryString");

            if (string.IsNullOrEmpty(queryString))
            {
                return BadRequest("Missing query string data.");
            }

            // Parse the query string to get the values
            var queryParams = Microsoft.AspNetCore.WebUtilities.QueryHelpers.ParseQuery(queryString);

            DateTime? startDate = null;
            DateTime? endDate = null;
            string id = _slackConfig.User_Id;
            string? description = queryParams["Description"];

            // Try parsing the dates
            if (DateTime.TryParse(queryParams["startDate"], out DateTime parsedStartDate))
            {
                startDate = parsedStartDate;
            }

            if (DateTime.TryParse(queryParams["endDate"], out DateTime parsedEndDate))
            {
                endDate = parsedEndDate;
            }

            var updateTimeOff = new UpdateTimeOff
            {
                startDate = startDate,
                endDate = endDate,
                Id = id,
                Description = description
            };

            string slackAccessToken = await getSlackAccessToken(code);

            return View();
        }

        private async Task<string> getSlackAccessToken(string code)
        {
            using (var client = new HttpClient())
            {
                var values = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", _slackConfig.Client_Id },
                    { "client_secret", _slackConfig.Client_Secret },
                    { "redirect_uri", _slackConfig.Redirect_Uri }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://slack.com/api/oauth.v2.access", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);
                return jsonResponse.authed_user.access_token;
            }
        }

        // get slack channels - start
        [HttpPost]
        public async Task<IActionResult> Slack_singin()
        {
            string clientId = _configuration["slackAppDetails:Client_Id"] ?? throw new ArgumentNullException("Client Id is not configured.");
            string clientSecret = _configuration["slackAppDetails:Client_Secret"] ?? throw new ArgumentNullException("Client Secret is not configured.");
            string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl") ?? throw new ArgumentNullException("FullUrl setting is not configured.");
            string RedirectURI = updatedHost != null ? $"{updatedHost}Slack/Getcode" : throw new ArgumentNullException("Redirect URI cannot be created because FullUrl is null.");
            string scope = _configuration["slackAppDetails:Scopes"] ?? throw new ArgumentNullException("Scopes are not configured.");


            string oauthUrl = $"https://slack.com/oauth/v2/authorize?client_id={clientId}&redirect_uri={RedirectURI}&scope={scope}";


            return Json(new { redirectUrl = oauthUrl });
        }

        [HttpGet]
        public async Task<IActionResult> Getcode(string code)
        {
            using (var client = new HttpClient())
            {
                string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");

                var values = new Dictionary<string, string>
                {
                    { "code", code },
                    { "client_id", _configuration["slackAppDetails:Client_Id"]},
                    { "client_secret", _configuration["slackAppDetails:Client_Secret"] },
                    { "redirect_uri", $"{updatedHost}Slack/Getcode" }
                };
                var content = new FormUrlEncodedContent(values);
                var response = await client.PostAsync("https://slack.com/api/oauth.v2.access", content);
                var responseString = await response.Content.ReadAsStringAsync();

                dynamic jsonResponse = JsonConvert.DeserializeObject(responseString);

                string oauthtoken = jsonResponse.authed_user.access_token;
                ViewData["Slack_Logedin"] = "true";

                // Map string values to DTO
                var messageAppListDto = new MessageAppListDTO
                {

                    MAID = new Guid(),
                    Company_ID =  new Guid(), 
                    Group_ID = "IT",                 
                    App_Nm = "Slack",                   
                    App_Desc = "Slack",                  
                    App_Channels = "appChannels",         
                    Access_Token_User_ID = oauthtoken, 
                    Access_Token_Txt = "oauthtoken",      
                    Publish_Immd_Flag = true             
                };

                // Call the CreateMessageApp method directly
                await _messageAppListRepository.AddAsync(messageAppListDto);

                GlobalVariables.SharedValue = oauthtoken;

                return RedirectToAction("slackNew");
            }
        }

        [HttpGet]
        public async Task<IActionResult> slackNew()
        {
            ViewData["Slack_Logedin"] = "true";
            return View();
        }


    }
}
