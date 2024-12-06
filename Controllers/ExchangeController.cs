using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph.Models;
using MySqlX.XDevAPI.Common;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using outofoffice.Graph;
using outofoffice.Models;
using outofoffice.Repositories.Implementations;
using outofoffice.Repositories.Interfaces;

namespace outofoffice.Controllers
{
    public class ExchangeController : Controller
    {
        private readonly IExchangeClient _exchangeClient;
        private readonly AuthService _authservice;
        private readonly IConfiguration _configuration;

        public ExchangeController(IExchangeClient exchangeClient, AuthService authservice, IConfiguration configuration)
        {
            _exchangeClient = exchangeClient;
            _authservice = authservice;
            _configuration = configuration;
        }

        public IActionResult Index()
        {
            return View();
        }

        public async Task<IActionResult> SetAutomaticReplies( string ScheduledStartDateTime_Date, 
                                                              string ScheduledStartDateTime_Time,
                                                              string ScheduledEndDateTime_Date, 
                                                              string ScheduledEndDateTime_Time, 
                                                              string Content)
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    accessToken = await _authservice.GetAccessTokenAsync();

                }
                catch (UnauthorizedAccessException ex)
                {
                    if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            redirect = Url.Action("Login", "Auth")
                        });
                    }

                    return RedirectToAction("Login", "Auth");
                }
            }

            var requestBody = new
            {
                automaticRepliesSetting = new
                {
                    status = "Scheduled",
                    scheduledStartDateTime = new
                    {
                        dateTime = ScheduledStartDateTime_Date.ToString(), 
                        timeZone = "UTC"
                    },
                    scheduledEndDateTime = new
                    {
                        dateTime = ScheduledEndDateTime_Date.ToString(), 
                        timeZone = "UTC"
                    },
                    externalReplyMessage = Content,
                    internalReplyMessage = Content
                }
            };

            var response = await _exchangeClient.SetAutomaticRepliesAsync(accessToken, requestBody);

            if (response.IsSuccessStatusCode)
            {
                var appToPublish = new AppToPublish
                {
                    Application = "Outlook",
                    Channel = new List<string>(),
                    Status = "Published",
                    Message = Content,

                    Publish_Schedule = new PublishSchedule
                    {
                        From_Date = DateTime.TryParse(ScheduledStartDateTime_Date, out DateTime start) ? start : throw new InvalidDataException("There is no \"From date.\"") ,
                        To_Date = DateTime.TryParse(ScheduledEndDateTime_Date, out DateTime end) ? end : throw new InvalidDataException("There is no \"To date.\"")
                    },
                    Created_Date = DateTime.Now

                };

                return Ok(new
                {
                    status = StatusCodes.Status200OK,
                    result = JsonConvert.SerializeObject(appToPublish),
                    message = "Mailbox Setting Updated Successfully!"
                });
            }

            return BadRequest($"Error: {await response.Content.ReadAsStringAsync()}");
        }

        public async Task<IActionResult> GetSites(DateTimeOffset startDate, DateTimeOffset endDate, string message)
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");
            if (string.IsNullOrEmpty(accessToken))
            {
                try
                {
                    accessToken = await _authservice.GetAccessTokenAsync();

                }
                catch (UnauthorizedAccessException ex)
                {
                    if (HttpContext.Request.Headers["X-Requested-With"] == "XMLHttpRequest")
                    {
                        return Json(new
                        {
                            redirect = Url.Action("Login", "Auth")
                        });
                    }

                    return RedirectToAction("Login", "Auth");
                }
            }

            var siteId = _configuration["Sharepoint:SiteId"];
            var ListId = _configuration["Sharepoint:ListId"];

            if( siteId is null || ListId is null) throw new ArgumentNullException("Site id or List id is empty or null");

            var response = await _exchangeClient.GetSites(accessToken, siteId: siteId, listId: ListId);

            var result = await response.Content.ReadAsStringAsync();

            response.EnsureSuccessStatusCode();

            var resultObj = JsonConvert.DeserializeObject<JObject>(result);

            var values = resultObj?["value"]?.ToList();

            var fields = values?.Select(f => f["fields"]).ToList();


            var userMail = HttpContext.Session.GetString("GraphMail");

            if (userMail is null)
            {
                return RedirectToAction("Index", "Profile");
            }

            var filteredFields = fields?
                                 .Where(f => f["Title"] != null &&
                                             f["Title"]?.ToString().ToLower().Trim() == userMail.ToLower().Trim()
                                             )
                                 .FirstOrDefault();

            if( filteredFields is null || !filteredFields.Any())
            {
                BadRequest("Item not Found!");
            }



            var leaveUpdates = new SharePointModel();

            

            if (filteredFields is JObject filteredFieldsObject)
            {
                var data = filteredFieldsObject.ToObject<Dictionary<string, object>>();
                leaveUpdates.MapFromDictionary(data);
                leaveUpdates.Id = data?["id"]?.ToString();
            }

            string month = startDate.ToString("MMM"); 

            var startMonth = startDate.Month;
            var endMonth = endDate.Month;

            var startDay = startDate.Day;
            var endDay = endDate.Day;

            string dayRange = $"{startMonth}/{startDay} - {endMonth}/{endDay}";

            var property = typeof(SharePointModel).GetProperty(month);
            if (property != null)
            {
                string currentValue = property.GetValue(leaveUpdates) as string;

                string newValue = string.IsNullOrEmpty(currentValue)
                    ? dayRange
                    : $"{currentValue}, {dayRange}";

                // Update the property value
                property.SetValue(leaveUpdates, newValue);
            }
            else
            {
                throw new InvalidOperationException($"Property '{month}' not found on SharePointModel.");
            }


            return PartialView("_SharePoint",leaveUpdates);
        }

        public async Task<IActionResult> addSites(SharePointModel sharePointModel)
        {
            var accessToken = HttpContext.Session.GetString("AccessToken");

            var data = sharePointModel.ToDictionary();

            if (string.IsNullOrEmpty(accessToken))
            {
                accessToken = await _authservice.GetAccessTokenAsync();
            }

            var response = await _exchangeClient.UpdateItemFieldsAsync(accessToken, "48144a5f-1f50-4279-b6a0-acaa27031b29", "0d760bdb-e0e5-439c-895c-93351b064525", sharePointModel.Id, data);

            var result = response.Content.ReadAsStringAsync().Result;

            response.EnsureSuccessStatusCode();

            var model = new SharePointModel();

            model.MapFromDictionary(data);

            return Ok(new
            {
                status = StatusCodes.Status200OK,
                result = JsonConvert.SerializeObject(model),
                message = "SharePoint Updated Successfully!"
            });
        }
    }
}
