using AutoMapper;
using Hangfire;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using outofoffice.Dto;
using outofoffice.Jobs;
using outofoffice.Models;
using outofoffice.Repositories.Interfaces;
using outofoffice.Services;
using System.Diagnostics;
using System.Globalization;
using System.Text;
using System.Text.Json.Nodes;
using System.Text.Json;
using Microsoft.Graph.Models;
using Microsoft.EntityFrameworkCore;

using outofoffice.Repositories.Implementations;



using outofoffice.Service;
using outofoffice.Servicess;
using outofoffice.Helper;
using outofoffice.App_code;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace outofoffice.Controllers

{
    public class HomeController : Controller
    {
        public readonly HttpClient Client;
        private readonly IHistoryValues _historyValues;
        private readonly IUserAppMessageRepository _UserAppMessage;
        private readonly IMapper _mapper;
        private readonly ISlackService _slackService;
        private readonly IAppSettingsManager _appSettingsManager;
        private readonly GetTeamsToken _gettoken;
        private readonly IMessageAppListRepository _messageAppListRepository;
        private readonly OOODbContext _context;
        private readonly AuthService _authService;
        private readonly IHangfireJobHandler _hangfireJobHandler;
        private readonly IZohoTimeOffService _timeOffService;

        public HomeController(HttpClient client, IHistoryValues historyValues, IMapper mapper, ISlackService slackService, IAppSettingsManager appSettingsManager, GetTeamsToken gettoken, IMessageAppListRepository messageAppListRepository, OOODbContext context, AuthService authService, IHangfireJobHandler hangfireJobHandler, IZohoTimeOffService timeOffService)
        {
            Client = client;
            _historyValues = historyValues ?? throw new ArgumentNullException(nameof(historyValues));
            _mapper = mapper;
            _slackService = slackService;
            _appSettingsManager = appSettingsManager;
            _gettoken = gettoken;
            _messageAppListRepository = messageAppListRepository;
            _context = context;
            _authService = authService;
            _hangfireJobHandler = hangfireJobHandler;
            _timeOffService = timeOffService;
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult _preview()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ReceiveJson()
        {
            using (var reader = new StreamReader(Request.Body))
            {
                var jsonString = await reader.ReadToEndAsync();

                // Store the received JSON string in session
                HttpContext.Session.SetString("StoredJson", jsonString);

                // Return a success message
                return Json(new { message = "Data received and stored in session!" });
            }
        }

        public async Task<IActionResult> Recent()
        {

            string UAID = Request.Query["value"];
            // Retrieve the scheme (http/https), host, and port
            string scheme = HttpContext.Request.Scheme;
            string host = HttpContext.Request.Host.Value;

            string slackOAuth_Token = await _context.MessageAppLists
            .Where(m => m.App_Nm == "Slack")
            .Select(m => m.Access_Token_User_ID)
            .FirstOrDefaultAsync();

            GlobalVariables.Slack_Token = slackOAuth_Token;


            // Construct the full URL
            string fullUrl = $"{scheme}://{host}/";

            string key = "FullUrl";  // This will hold the key, e.g., "FullUrl"
            string value = fullUrl;  // This will hold the value, e.g., "https://newurl.com"

            await _appSettingsManager.UpdateAppSettingsAsync(key, value);
            //string updatedHost = await _appSettingsManager.GetAppSettingAsync(key);

            string User_ID = HttpContext?.Session.GetString("UserEmail");

            //var UserAppConfig = await _UserAppConfigRepository.GetUserAppConfigList(User_ID);

            //// Serialize the list into a JSON string
            //var jsonString = JsonConvert.SerializeObject(UserAppConfig, Formatting.Indented);

            //// Store the JSON string in session
            //HttpContext.Session.SetString("UserAppConfigDTOs", jsonString);

            bool responseCheck = false;
            var responseBody = "";


            if (UAID == null)
            {
                // Example URL: Currenthost/api/UserAppMessage/Recent
                var apiUrlRecent = $"{value}api/UserAppMessage/Recent/{User_ID}";
                var responseRecent = await Client.GetAsync(apiUrlRecent);

                if (responseRecent.IsSuccessStatusCode)
                {
                    responseBody = await responseRecent.Content.ReadAsStringAsync();
                    responseCheck = true;
                }
                else
                {
                    Console.WriteLine("Error: " + responseRecent.StatusCode);
                    var errorResponse = await responseRecent.Content.ReadAsStringAsync();
                    Console.WriteLine("Error Response: " + errorResponse);
                }
            }
            else
            {
                // If UAID is not null, proceed with matching the UAID
                responseBody = GetMatchedValueByUaid(UAID); // Call function to get matched value

                if (!string.IsNullOrEmpty(responseBody))
                {
                    Console.WriteLine("Matched Value: " + responseBody);
                    responseCheck = true;
                }
                else
                {
                    Console.WriteLine("No match found for UAID: " + UAID);
                }
            }


            string fromDate = "";
            string toDate = "";
            string message = "";
            string applicationsCommaSeparated = "";
            string UAID_ID = "";

            if (responseCheck)
            {
                //var responseBody = await response.Content.ReadAsStringAsync();

                // Deserialize the main response to a UserAppMessage object
                var responseTemp = JsonConvert.DeserializeObject<UserAppMessage>(responseBody);

                // Check if apps_To_Publish is not null or empty before deserializing
                List<AppToPublish> appsToPublish = null;
                if (!string.IsNullOrEmpty(responseTemp?.Apps_To_Publish))
                {
                    appsToPublish = JsonConvert.DeserializeObject<List<AppToPublish>>(responseTemp.Apps_To_Publish);

                    // Serialize the JObject to a string
                    string serializedJson = JsonConvert.SerializeObject(appsToPublish);

                    // Save the value to the session
                    HttpContext.Session.SetString("getRecentValues", serializedJson);

                    // Pass it to the view using ViewBag
                    ViewBag.Recent_Value_Stored = serializedJson;


                    if (appsToPublish != null)
                    {
                        // Collect all application names into a list
                        List<string> applicationNames = new List<string>();

                        foreach (var app in appsToPublish)
                        {
                            if (app.Publish_Schedule != null)
                            {

                                string application = app.Application;

                                // Add the application name to the list
                                applicationNames.Add(application);

                                Console.WriteLine("From Date: " + fromDate);
                                Console.WriteLine("To Date: " + toDate);
                                Console.WriteLine("Application: " + application);
                            }
                        }

                        // Create a comma-separated string from the application names
                        applicationsCommaSeparated = string.Join(", ", applicationNames);
                        Console.WriteLine("Applications: " + applicationsCommaSeparated);
                    }
                    fromDate = responseTemp.OOO_From_Dt.ToString("yyyy-MM-dd HH:mm");
                    toDate = responseTemp.OOO_To_Dt.ToString("yyyy-MM-dd HH:mm");

                    message = responseTemp.Message_Txt;
                }

                Guid uaid = responseTemp.UAID;
                UAID_ID = uaid.ToString();
            }

            var recent = new OutOfOfficeRecent
            {
                Application = applicationsCommaSeparated, // Add the selected values using "," only
                StartDate = fromDate, // Adjust date format as needed
                EndDate = toDate,
                Message = message,
                UAID_ID = UAID_ID
            };

            // Create a dictionary to store application status and dynamic messages
            var appMessages = new Dictionary<string, string>();

            var applications = recent.Application.Split(',');

            // Define the application names
            var appNames = new[] { "slack", "teams", "zoho", "outlook", "sharepoint" };

            // Check if StartDate or EndDate are empty, and if so, apply the default behavior
            if (string.IsNullOrEmpty(recent.StartDate) || string.IsNullOrEmpty(recent.EndDate) || string.IsNullOrEmpty(recent.EndDate))
            {
                // Default behavior when either start date, end date, or message box is empty
                foreach (var appName in appNames)
                {
                    // Default message for apps not found in the list
                    if (appName.ToLower() == "zoho")
                    {
                        appMessages[appName] = $"Time Off request published to {appName} on your Out of Office day.";
                    }
                    else
                    {
                        appMessages[appName] = $"Out Of Office Message published to {appName} on your Out of Office day.";
                    }
                }
            }
            else
            {
                if (toDate != "")
                {
                    // Parse the input string to DateTime
                    DateTime parsedDate = DateTime.ParseExact(recent.StartDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);
                    DateTime parsedEndDate = DateTime.ParseExact(recent.EndDate, "yyyy-MM-dd HH:mm", CultureInfo.InvariantCulture);

                    // Separate date and time
                    string formattedDate = parsedDate.ToString("MM/dd/yyyy"); // Date format
                    string formattedTime = parsedDate.ToString("hh:mm tt");  // Time format (12-hour format with AM/PM)

                    // Get the current date and time
                    DateTime currentDate = DateTime.Now;

                    // Iterate over each application name
                    foreach (var appName in appNames)
                    {
                        // Compare the current time with the start date
                        if (currentDate >= parsedEndDate) // If expired (current time is greater than or equal to the End time)
                        {
                            // Check if the app is found in the list
                            if (applications.Any(app => string.Equals(app.Trim(), appName, StringComparison.OrdinalIgnoreCase)))
                            {
                                if (appName.ToLower() == "zoho")
                                {
                                    appMessages[appName] = $"Time Off request Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day)";
                                }
                                else
                                {
                                    // If the app is found in the list, format the message with date and time
                                    appMessages[appName] = $"Out Of Office Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day)";
                                }
                            }
                            else
                            {
                                // Default message for apps not found in the list
                                string defaultMessage;

                                if (appName.ToLower() == "zoho")
                                {
                                    defaultMessage = $"Time Off request will be published to {appName} immediately.";
                                }
                                else if (appName.ToLower() == "sharepoint")
                                {
                                    defaultMessage = $"Out Of Office Message will be published to {appName} immediately.";
                                }
                                else
                                {
                                    defaultMessage = $"Out Of Office Message will be published to {appName} on your Out of Office day.";
                                }

                                appMessages[appName] = defaultMessage;
                            }
                        }
                        else // If the current time is before the start time (not expired)
                        {
                            // Check if the app is found in the list for the non-expired case
                            if (applications.Any(app => string.Equals(app.Trim(), appName, StringComparison.OrdinalIgnoreCase)))
                            {
                                // Assign specific messages for each app with the new format
                                switch (appName.ToLower()) // .ToLower() ensures case-insensitive comparison
                                {
                                    case "slack":
                                        appMessages[appName] = $"Out Of Office Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day). Click <a href=\"#\" onclick=\"setConfig('slack', 'edit'); return false;\">here</a> to change when this message is published.";
                                        break;

                                    case "teams":
                                        appMessages[appName] = $"Out Of Office Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day). Click <a href=\"#\" onclick=\"setConfig('teams', 'edit'); return false;\">here</a> to change when this message is published.";
                                        break;

                                    case "zoho":
                                        appMessages[appName] = $"Time Off request will be published to {appName}. Click <a href=\"#\" onclick=\"setConfig('zoho', 'edit'); return false;\">here</a> to change when this message is published.";
                                        break;

                                    case "outlook":
                                        appMessages[appName] = $"Out Of Office Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day). Click <a href=\"#\" onclick=\"setConfig('outlook', 'edit'); return false;\">here</a> to change when this message is published.";
                                        break;

                                    case "sharepoint":
                                        appMessages[appName] = $"Out Of Office Message will be published to {appName} on {formattedDate}, at {formattedTime} (your Out of Office day). Click <a href=\"#\" onclick=\"setConfig('sharepoint', 'edit'); return false;\">here</a> to change when this message is published.";
                                        break;

                                    // If the app name doesn't match any of the cases
                                    default:
                                        appMessages[appName] = $"Out Of Office Message will be published to {appName} on your Out of Office day.";
                                        break;
                                }
                            }
                            else
                            {
                                if (appName.ToLower() == "zoho")
                                {
                                    appMessages[appName] = $"Time Off request published to {appName} on your Out of Office day.";
                                }
                                else
                                {
                                    // Default message for apps not found in the list (before the start time)
                                    appMessages[appName] = $"Out Of Office Message published to {appName} on your Out of Office day.";
                                }
                            }
                        }
                    }
                }
            }
            // Convert the dictionary to a single string
            recent.AppStatus = string.Join("<br />", appMessages.Values);

            var apiUrl = $"{value}api/UserAppMessage/ListOfScheduledItems/{User_ID}";
            var response = await Client.GetAsync(apiUrl);

            if (response.IsSuccessStatusCode)
            {
                responseBody = await response.Content.ReadAsStringAsync();

                // Save the value to the session
                HttpContext.Session.SetString("ListOfScheduledItems", responseBody);


                List<ResponseItem> responseItems = JsonConvert.DeserializeObject<List<ResponseItem>>(responseBody);

                List<DropdownItemWithTooltip> dropdownItems = new List<DropdownItemWithTooltip>();

                foreach (var item in responseItems)
                {
                    // Check if both ooO_From_Dt and ooO_To_Dt are not empty or null
                    if (!string.IsNullOrEmpty(item.ooO_From_Dt) && !string.IsNullOrEmpty(item.ooO_To_Dt))
                    {
                        DateTime from_Date;
                        DateTime to_Date;

                        // Attempt to parse the from and to dates
                        bool fromParsed = DateTime.TryParse(item.ooO_From_Dt, out from_Date);
                        bool toParsed = DateTime.TryParse(item.ooO_To_Dt, out to_Date);

                        if (fromParsed && toParsed)
                        {
                            // Format the date range in MM/dd/yyyy - MM/dd/yyyy format
                            string dateRange = $"{from_Date:MM/dd/yyyy} - {to_Date:MM/dd/yyyy}";

                            string app_Names = string.Empty;
                            if (!string.IsNullOrEmpty(item.apps_To_Publish))
                            {
                                var appsToPublish = JsonConvert.DeserializeObject<List<AppToPublish>>(item.apps_To_Publish);

                                var applicationNames = appsToPublish?.Select(app => app.Application).ToList();
                                if (applicationNames != null && applicationNames.Any())
                                {
                                    app_Names = string.Join(", ", applicationNames);
                                }
                            }

                            // Add the item to the dropdown list
                            dropdownItems.Add(new DropdownItemWithTooltip
                            {
                                Text = dateRange,
                                Value = item.uaid.ToString(),
                                Tooltip = app_Names
                            });
                        }
                    }
                }
                // Assign the dynamically created dropdown items to ViewBag
                ViewBag.DropdownItems = dropdownItems;
            }
            // Pass the updated model to the view
            return View(recent);
        }



        public IActionResult New()
        {

            // Retrieve the value from Session
            string getRecentValues = HttpContext.Session.GetString("getRecentValues");
            // Pass it to the view using ViewBag
            ViewBag.Recent_Value_Stored = getRecentValues;
            ViewBag.SlackMessage = GlobalVariables.SharedValue;

            return View();
        }

        [HttpPost]
        public async Task<IActionResult> responseStatus(OutOfOfficeRecent updateTimeOff)
        {
            bool isSuccess = false; // logic to check success or failure;
            string updatedJson = "";
            string uniqId = string.Empty;
            string usermail = HttpContext?.Session.GetString("UserEmail");

            var createdResponse = new UserAppMessageCreatedDTO();
            if (updateTimeOff.mode == "new")
            {
                string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");

                string tokenUrl = $"{updatedHost}api/UserAppMessage";
                //var start_dt = DateTimeOffset(updateTimeOff.StartDate).ToUnixTimeSeconds();
                //// Schedule a recurring job every 10 minutes
                //RecurringJob.AddOrUpdate(
                //    "update-slack-status",
                //    () => _slackService.update_Profilestatus("In a meeting", GlobalVariables.SharedValue),
                //    Cron.MinuteInterval(start_dt)
                //);


                ////// Don't delete this Zoho Scheduler added -Start-
                ////var zohoTimeOff = new UpdateTimeOff
                ////{
                ////    Description = updateTimeOff.Message,
                ////    endDate = DateTime.Parse(updateTimeOff.EndDate),
                ////    startDate = DateTime.Parse(updateTimeOff.StartDate)
                ////};

                ////BackgroundJob.Schedule<HangfireJobService>((service) => service.ScheduleZohoTimeOff(zohoTimeOff), TimeSpan.FromMinutes(1));
                ////// Don't delete this Zoho Scheduler added -End-

                // Value Get from Model -Start-
                var applications = updateTimeOff.Application.Split(',').Select(app => app.Trim()).ToArray();

                // slack scheduler
                // BackgroundJob.Schedule<HangfireJobService>((service) => service.ScheduleSlackStatus(updateTimeOff.Message, ":spiral_calendar_pad:", GlobalVariables.SharedValue, updateTimeOff.EndDate), DateTimeOffset.Parse(updateTimeOff.StartDate));




                // Create the JSON structure for each application
                var jsonObjects = applications.Select(app => new AppToPublish
                {
                    Application = app, // Each object will have a single application
                    Channel = new List<string> { "Chn1", "Chn2" },  // Hardcoded channels
                    Status = "Scheduled",  // Hardcoded status
                    Message = updateTimeOff.Message,
                    Publish_Schedule = new PublishSchedule
                    {
                        From_Date = DateTime.TryParse(updateTimeOff.StartDate, out DateTime start) ? start : DateTime.Now,
                        To_Date = DateTime.TryParse(updateTimeOff.EndDate, out DateTime end) ? end : DateTime.Now
                    },
                    Created_Date = DateTime.Now
                }).ToList();

                foreach (var item in jsonObjects)
                {
                    if (item.Application.Trim().ToLower() == "sharepoint")
                    {
                        item.Message = $"{item.Publish_Schedule.From_Date.Month}/{item.Publish_Schedule.From_Date.Day} - {item.Publish_Schedule.To_Date.Month}/{item.Publish_Schedule.To_Date.Day}";
                    }
                }

                // Convert to JSON string
                string modelValue = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);
                // Value Get from Model -End-

                // Value Get from session (Update Settings) -Start-
                // Retrieve the JSON string from session 
                var GlobalJsonData = HttpContext.Session.GetString("StoredJson") ?? "";

               

                // Deserialize updated session data
                var updatedConfigValueObj = JsonConvert.DeserializeObject<List<JObject>>(GlobalJsonData);

                // Deserialize model value
                var modelValueObj = JsonConvert.DeserializeObject<List<JObject>>(modelValue);

                if (updatedConfigValueObj != null && modelValueObj != null)
                {
                    foreach (var item1 in updatedConfigValueObj)
                    {
                        var appName = item1["Application"]?.ToString();

                        if (!string.IsNullOrEmpty(appName))
                        {
                            // Find the matching entry in modelValueObj
                            var matchingItem = modelValueObj.FirstOrDefault(v2 => v2["Application"]?.ToString() == appName);

                            if (matchingItem != null)
                            {
                                // Replace values from item1 to matchingItem
                                foreach (var property in item1.Properties())
                                {
                                    matchingItem[property.Name] = property.Value;
                                }
                            }
                        }
                    }
                }

                // Serialize the updated modelValueObj back to JSON
                updatedJson = JsonConvert.SerializeObject(modelValueObj, Formatting.Indented);

                if(updatedJson == "[]")
                {
                    return View("new").WithSuccess("Error: Invalid input. Please try again.");
                }

                bool isZohoAvailable = applications.Any(app => app.Equals("Zoho", StringComparison.OrdinalIgnoreCase));
                string timeOffID = "";

                if (isZohoAvailable)
                {
                    string listType = "modules";
                    string accessTokenUser = await _timeOffService.getAccessCode("Read", listType);

                    string Service_Resources_ID = await _timeOffService.GetServiceResourceListAsync(accessTokenUser, usermail);


                    string accessToken = await _timeOffService.getAccessCode("Create", listType);

                    // Deserialize updatedJson into a JArray
                    var objArray = JsonConvert.DeserializeObject<JArray>(updatedJson);

                    // Filter the objects where "Application" is "Zoho"
                    var zohoObjects = objArray
                                        .Where(obj => obj["Application"] != null && obj["Application"].ToString().Equals("Zoho", StringComparison.OrdinalIgnoreCase))
                                        .ToList();

                    if (zohoObjects.Any())
                    {
                        // Process each "Zoho" object
                        foreach (var obj in zohoObjects)
                        {
                            string message = obj["Message"]?.ToString() ?? "Default Message";
                            string fromDate = obj["Publish_Schedule"]?["From_Date"]?.ToString() ?? "N/A";
                            string toDate = obj["Publish_Schedule"]?["To_Date"]?.ToString() ?? "N/A";
                            string timeOffType = obj["TimeOffType"]?.ToString() ?? "Default TimeOffType";
                            string zohoTimeOffID = obj["ZohoTimeOffID"]?.ToString() ?? "Default ID";
                            string reason = obj["Reason"]?.ToString() ?? "No Reason";

                            // Handle Start and End Date
                            string StartDate = string.IsNullOrEmpty(fromDate) ? "N/A" : fromDate;
                            string EndDate = string.IsNullOrEmpty(toDate) ? "N/A" : toDate;

                            // Populate UpdateTimeOff
                            var updateTimeOffTemp = new UpdateTimeOff
                            {
                                Description = message,
                                startDate = DateTime.Parse(StartDate),
                                endDate = DateTime.Parse(EndDate),
                                ZohoTimeOffID = zohoTimeOffID,
                                Reason = reason,
                                TimeOffType = timeOffType
                            };

                            // You can now use updateTimeOffTemp to perform further actions, like saving to a database or passing to a service
                            timeOffID = await _timeOffService.CreateTimeOffAsync(updateTimeOffTemp, accessToken, Service_Resources_ID);
                        }
                    }
                    else
                    {
                        // Handle case where no Zoho application data is found
                        Console.WriteLine("No Zoho applications found in the input.");
                    }



                    if (timeOffID.Contains("Error"))
                    {
                        applications = updateTimeOff.Application
                        .Split(',') // Split the string by commas
                        .Select(app => app.Trim()) // Trim any extra spaces
                        .Where(app => app != "Zoho") // Filter out "Zoho"
                        .ToArray(); // Convert back to an array

                        Console.WriteLine("Zoho TimeOff Request not created. " + timeOffID);

                    }
                    else
                    {
                        // Proceed with normal logic
                        Console.WriteLine("Time Off ID: " + timeOffID);
                    }
                }


                if (!string.IsNullOrEmpty(updatedJson))
                {
                    // Deserialize JSON into a list of JObjects
                    var jsonArray = JsonConvert.DeserializeObject<List<JObject>>(updatedJson);

                    foreach (var item in jsonArray)
                    {
                        if (item["Application"]?.ToString() == "Zoho")
                        {
                            // Add or update the ZohoTimeOffID
                            item["ZohoTimeOffID"] = timeOffID; // Replace with the actual value
                        }
                    }

                    // Serialize back to a formatted JSON string and update the session
                    updatedJson = JsonConvert.SerializeObject(jsonArray, Formatting.Indented);
                    HttpContext.Session.SetString("StoredJson", updatedJson);
                }






                var data = new Dictionary<string, object>
            {
                { "company_ID", "3FA85F64-5717-4562-B3FC-2C963F66AFA6" },
                { "group_ID", "IT" },
                { "user_ID", usermail },
                { "message_Type", "string" },
                { "ooO_From_Dt", updateTimeOff.StartDate },
                { "ooO_To_Dt", updateTimeOff.EndDate },
                { "message_Txt", updateTimeOff.Message },
                { "apps_To_Publish", updatedJson },
                { "publish_Status", "No" },
                { "CreatedDate", DateTime.Now },
            };

                StringContent content = new StringContent(JsonConvert.SerializeObject(data), Encoding.UTF8, "application/json");
                //var content = new FormUrlEncodedContent(data);

                try
                {
                    var response = await Client.PostAsync(tokenUrl, content);
                    if (response.IsSuccessStatusCode)
                    {
                        isSuccess = true;
                        var result  = await response.Content.ReadAsStringAsync();                      
                        createdResponse = JsonConvert.DeserializeObject<UserAppMessageCreatedDTO>(result);
                    }
                    else
                    {
                        Console.WriteLine("Error: " + response.StatusCode);
                        var errorResponse = await response.Content.ReadAsStringAsync();
                        Console.WriteLine("Error Response: " + errorResponse);
                        isSuccess = false;
                    }
                    //ViewBag.response = result;
                }
                catch (Exception ex)
                {
                    Console.WriteLine("Exception: " + ex.Message);
                    isSuccess = false;
                }

            }
            if (updateTimeOff.mode == "recent")
            {

                var applications = updateTimeOff.Application.Split(',').Select(app => app.Trim()).ToArray();

                string User_ID = HttpContext?.Session.GetString("getRecentValues");

                string zohoTimeOffID = null;

                // Check if User_ID is not null or empty
                if (!string.IsNullOrEmpty(User_ID))
                {
                    // Parse the JSON data to extract ZohoTimeOffID
                    var userObjects = JArray.Parse(User_ID); // Assuming User_ID is a JSON array
                    var zohoObject = userObjects.FirstOrDefault(obj => obj["Application"]?.ToString() == "Zoho");
                    if (zohoObject != null)
                    {
                        zohoTimeOffID = zohoObject["ZohoTimeOffID"]?.ToString();
                    }
                }

                //// Create the JSON structure for each application
                var jsonObjects = applications.Select(app => new
                {
                    Application = app, // Each object will have a single application
                    Channel = new[] { "Chn1", "Chn2" },  // Hardcoded channels
                    Status = "Scheduled",  // Hardcoded status
                    Message = updateTimeOff.Message,
                    Publish_Schedule = new
                    {
                        From_Date = updateTimeOff.StartDate.ToString(),
                        To_Date = updateTimeOff.EndDate.ToString()
                    },
                    Created_Date = DateTime.Now.ToString(),
                    ZohoTimeOffID = app == "Zoho" ? zohoTimeOffID : null
                }).ToList();
                // Convert to JSON string
                string modelValue = JsonConvert.SerializeObject(jsonObjects, Formatting.Indented);


                // Value Get from session (Update Settings) -Start-
                // Retrieve the JSON string from session 
                var GlobalJsonData = HttpContext.Session.GetString("StoredJson") == null ? "" : HttpContext.Session.GetString("StoredJson");

                var updatedConfigValue = JsonConvert.SerializeObject(JsonConvert.DeserializeObject(GlobalJsonData), Formatting.Indented);
                // Value Get from session (Update Settings) -End-

                // Deserialize both JSON strings into lists of JObjects
                var updatedConfigValueObj = JsonConvert.DeserializeObject<List<JObject>>(updatedConfigValue);
                var modelValueObj = JsonConvert.DeserializeObject<List<JObject>>(modelValue);

                if (updatedConfigValueObj != null)
                {
                    foreach (var item1 in updatedConfigValueObj)
                    {
                        var appName = item1["Application"]?.ToString();

                        // Find the matching entry in Value 2
                        var matchingItem = modelValueObj.FirstOrDefault(v2 => v2["Application"]?.ToString() == appName);

                        if (matchingItem != null)
                        {
                            // Replace values from Value 1 to Value 2
                            foreach (var property in item1.Properties())
                            {
                                matchingItem[property.Name] = property.Value;
                            }
                        }
                    }

                }
                // Compare and update Value 2 with values from Value 1

                // Serialize the updated Value 2 list back into JSON
                updatedJson = JsonConvert.SerializeObject(modelValueObj, Formatting.Indented);

                if (updatedJson == "[]")
                {
                    return View("Recent").WithSuccess("Error: Invalid input. Please try again.");
                }

                bool isZohoAvailable = applications.Any(app => app.Equals("Zoho", StringComparison.OrdinalIgnoreCase));
                string timeOffID = "";

                if (isZohoAvailable)
                {
                    string listType = "modules";
                    string accessToken = await _timeOffService.getAccessCode("Update", listType);

                    // Deserialize JSON to list of objects
                    var ZohoValues = JsonConvert.DeserializeObject<List<AppToPublish>>(updatedJson);

                    // Filter single object where Application is "Zoho"
                    var zohoValue = ZohoValues
                        .Where(x => x.Application == "Zoho")
                        .Select(x => new
                        {
                            x.Message,
                            x.Publish_Schedule.From_Date,
                            x.Publish_Schedule.To_Date,
                            x.TimeOffType,
                            x.Reason,
                            x.ZohoTimeOffID
                        })
                        .FirstOrDefault();

                    // Check if a matching object was found and output its values
                    if (zohoValue != null)
                    {
                        Console.WriteLine($"Message: {zohoValue.Message}");
                        Console.WriteLine($"From_Date: {zohoValue.From_Date}");
                        Console.WriteLine($"To_Date: {zohoValue.To_Date}");
                        Console.WriteLine($"TimeOffType: {zohoValue.TimeOffType}");
                        Console.WriteLine($"Reason: {zohoValue.Reason}");
                    }
                    if (zohoValue != null)
                    {
                        // Don't delete this Zoho Scheduler added -Start-
                        var updateTimeOffTemp = new UpdateTimeOff
                        {
                            Description = zohoValue.Message,
                            endDate = zohoValue.To_Date,
                            startDate = zohoValue.From_Date,
                            TimeOffType = zohoValue.TimeOffType,
                            Reason = zohoValue.Reason,
                            ZohoTimeOffID = zohoValue.ZohoTimeOffID

                        };
                        string ZohoTimeOffID = zohoValue.ZohoTimeOffID;
                        timeOffID = await _timeOffService.UpdateTimeOffAsync(updateTimeOffTemp, accessToken, ZohoTimeOffID, usermail);


                    }
                    if (timeOffID.Contains("Error"))
                    {
                        applications = updateTimeOff.Application
                        .Split(',') // Split the string by commas
                        .Select(app => app.Trim()) // Trim any extra spaces
                        .Where(app => app != "Zoho") // Filter out "Zoho"
                        .ToArray(); // Convert back to an array
                        Console.WriteLine("Zoho TimeOff Request not created. " + timeOffID);
                    }
                    else
                    {
                        // Proceed with normal logic
                        Console.WriteLine("Time Off ID: " + timeOffID);
                    }
                }


                var companyId = "3FA85F64-5717-4562-B3FC-2C963F66AFA6";
                var groupId = "IT";
                var UAID = updateTimeOff.UAID_ID;

                var userAppMessageDto = new
                {
                    Company_ID = companyId,
                    Group_ID = groupId,
                    User_ID = usermail,
                    message_Type = "string",
                    ooO_From_Dt = updateTimeOff.StartDate,
                    ooO_To_Dt = updateTimeOff.EndDate,
                    message_Txt = updateTimeOff.Message,
                    apps_To_Publish = updatedJson,
                    publish_Status = "No",
                    UAID = UAID,
                    CreatedDate = DateTime.Now

                };

                var jsonContent = JsonConvert.SerializeObject(userAppMessageDto);
                var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

                string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");

                string tokenUrl = $"{updatedHost}api/UserAppMessage/{companyId}/{groupId}/{UAID}";
                // Make the PUT request to the API
                var response = await Client.PutAsync(tokenUrl, content);

                if (response.IsSuccessStatusCode)
                {
                    isSuccess = true;
                }
                else
                {
                    Console.WriteLine("Error: " + response.StatusCode);
                    var errorResponse = await response.Content.ReadAsStringAsync();
                    Console.WriteLine("Error Response: " + errorResponse);
                    isSuccess = false;
                }
            }

            if (isSuccess)
            {

                var data_List = JsonConvert.DeserializeObject<List<dynamic>>(updatedJson);

                Dictionary<string, string> jobIds = new Dictionary<string, string>();

                // Loop through each object and get the Application value
                foreach (var item in data_List)
                {
                    Console.WriteLine("Application: " + item.Application);
                    if (item.Application == "Slack")
                    {
                        string? slackOAuth_Token = await _context.MessageAppLists
                                .Where(m => m.App_Nm == "Slack")
                                .Select(m => m.Access_Token_User_ID)
                                .FirstOrDefaultAsync();

                        // slack scheduler
                        string slack_sts = item.Status;
                        string slack_presence = item.SlackPresence;
                        string slack_expiry = item.SlackExpireDuration;//Channel
                        var slack_chann = item.Channel;
                        string slack_channels = string.Join(",", slack_chann);

                        //var slack_chan = Contents.join(", ");




                        string slack_message = item.Message;


                        // Slack scheduler
                        string jobId = BackgroundJob.Schedule<HangfireJobService>((service) => service.ScheduleSlackStatus(slack_sts, slack_presence, slack_expiry, slack_message, slack_channels, ":spiral_calendar_pad:", slackOAuth_Token, updateTimeOff.EndDate), DateTimeOffset.Parse(updateTimeOff.StartDate));

                        jobIds["Slack"] = jobId;


                    }
                    if (item.Application == "Teams")
                    {

                        string? userEmail = HttpContext.Session.GetString("UserEmail");

                        if (string.IsNullOrEmpty(userEmail))
                        {
                            // Handle the case where userEmail is null or empty.
                            throw new InvalidOperationException("User email is not available in the session.");
                        }

                        string? Teams_RefreshToken = await _context.MessageAppLists
                            .Where(m => m.App_Nm == "Microsoft" && m.UserEmail == userEmail)
                            .Select(m => m.Access_Token_Txt)
                            .FirstOrDefaultAsync();

                        string Teams_RefreshTokenNonNull = Teams_RefreshToken ?? throw new InvalidOperationException("Refresh token not found.");

                        string? Teams_Token = await _context.MessageAppLists
                            .Where(m => m.App_Nm == "Microsoft" && m.UserEmail == userEmail)
                            .Select(m => m.Access_Token_User_ID)
                            .FirstOrDefaultAsync();

                        string Teams_TokenNonNull = Teams_Token ?? throw new InvalidOperationException("Token not found.");

                        string? user_ID = await _context.MessageAppLists
                            .Where(m => m.App_Nm == "Microsoft" && m.UserEmail == userEmail)
                            .Select(m => m.UserID)
                            .FirstOrDefaultAsync();

                        string user_IDNonNull = user_ID ?? throw new InvalidOperationException("User ID not found.");


                        string availability = item.Status;
                        string activity = item.Activity;
                        string StatuMsg = item.TeamsStatusMessage;
                        string expiry = item.TeamsExpireDuration;
                        string Channals = JsonConvert.SerializeObject(item.Channel);
                        string ChannalMsg = item.ChannalMessage;

                        // Team scheduler
                        string JobId = BackgroundJob.Schedule<HangfireJobService>((service) => service.ScheduleTeamsStatus(availability, activity, expiry, StatuMsg, Teams_Token,ChannalMsg, Channals, Teams_RefreshToken, user_ID), DateTimeOffset.Parse(updateTimeOff.StartDate));

                        jobIds["Teams"] = JobId;

                    }
                    
                }

                if (jobIds.Count > 0)
                {
                    if (updateTimeOff.mode == "new")
                    {
                        await _hangfireJobHandler.AddJobIds(jobIds, createdResponse!.UAID);
                    }
                    if (updateTimeOff.mode == "recent")
                    {
                        Guid RecentUAID = Guid.Parse(updateTimeOff.UAID_ID);

                        await _hangfireJobHandler.AddJobIds(jobIds, RecentUAID);

                    }

                }

                ViewBag.Message = "Details have been successfully added to the Scheduler.";
                ViewBag.IsSuccess = true;
                // Clear all session data
                HttpContext.Session.SetString("StoredJson", "");
            }
            else
            {
                ViewBag.Message = "Error adding details to the Scheduler.";
                ViewBag.IsSuccess = false;
                // Clear all session data
                HttpContext.Session.SetString("StoredJson", "");
            }



            return View();
        }

        public async Task<IActionResult> History()
        {
            try
            {
                string updatedHost = await _appSettingsManager.GetAppSettingAsync("FullUrl");
                string User_ID = HttpContext?.Session.GetString("UserEmail");

                string apiUrl = $"{updatedHost}api/UserAppMessage/History/{User_ID}"; // Replace with your actual API URL
                var historyData = await _historyValues.GetHistoryValues(apiUrl);

                // Use ILogger for logging (replace Console.WriteLine with proper logging)
                foreach (var record in historyData)
                {
                    Console.WriteLine($"Application: {record.Application}, Status: {record.Status}, Start Date: {record.StartDate}, End Date: {record.EndDate}, Message: {record.Message}");
                }

                return View(historyData);
            }
            catch (Exception ex)
            {
                // Log the exception with more details
                Console.WriteLine($"Error occurred while retrieving history data: {ex.Message} - {ex.StackTrace}");

                // Return an error view or return an error message to the user
                return View().WithError(ex.Message);
            }
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

    
        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        [AllowAnonymous]
        public IActionResult ErrorWithMessage(string message, string debug)
        {
            return View("Index").WithError(message, debug);
        }

        [HttpPost]
        public async Task<IActionResult> TeamsLogin()
        {
            string code = await _gettoken.GetAuthorizationCodeAsync();
            var tokens = await _gettoken.GetTokensAsync(code);

            try
            {
                string accessToken = tokens.AccessToken;
                string refershToken = tokens.RefreshToken;
                string expiry = tokens.Expiry;
                Guid companyGuid = Guid.Parse("3FA85F64-5717-4562-B3FC-2C963F66AFA6");

                var userProfile = await _authService.CallMicrosoftGraphAsync(accessToken);

                // Map string values to DTO
                var messageAppListDto = new MessageAppListDTO
                {
                    MAID = new Guid(),
                    Company_ID = companyGuid,
                    Group_ID = "IT",
                    App_Nm = "Microsoft",
                    App_Desc = "Microsoft",
                    App_Channels = "One,two,three",
                    Access_Token_User_ID = accessToken,
                    Access_Token_Txt = refershToken,
                    Publish_Immd_Flag = true,
                    UserEmail = userProfile.Mail,
                    UserID = userProfile.Id
                };

                HttpContext.Session.SetString("AccessToken", accessToken);
                HttpContext.Session.SetString("RefreshToken", refershToken);
                HttpContext.Session.Sets("TokenExpiry", DateTime.UtcNow.AddSeconds(int.Parse(expiry)));
                HttpContext.Session.SetString("GraphUserId", userProfile.Id);
                HttpContext.Session.SetString("UserPrincipalName", userProfile.UserPrincipalName);
                HttpContext.Session.SetString("GraphMail", userProfile.Mail);


                // Call the CreateMessageApp method directly
                await _messageAppListRepository.UpdateAsync(messageAppListDto);

                return Ok();
                //return RedirectToAction("profile","index");
            }
            catch (Exception ex)
            {
                // Log or print the exception message for debugging
                Console.WriteLine($"An error occurred while saving tokens: {ex.Message}");
                return Json(new { error = ex.Message });
            }
        }
        [HttpGet]
        public async Task<IActionResult> teams()
        {
            ViewData["Slack_Logedin"] = "true";
            return View();
        }

        // Function to get matched value by UAID
        public string GetMatchedValueByUaid(string uaid)
        {
            var jsonString = HttpContext?.Session.GetString("ListOfScheduledItems");

            if (string.IsNullOrEmpty(jsonString))
            {
                return "No data available";
            }

            // Parse the JSON string into a JArray
            JArray items = JArray.Parse(jsonString);

            // Find the matching item by UAID
            var matchedItem = items.FirstOrDefault(item => item["uaid"]?.ToString() == uaid);

            if (matchedItem == null)
            {
                return "No matching UAID found";
            }

            // Convert the matched item back to a string (pretty-printing it)
            return matchedItem.ToString(Formatting.Indented);
        }

    }
}