using Azure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Newtonsoft.Json;
using outofoffice.Graph;
using outofoffice.Models;
using TimeZoneConverter;

namespace outofoffice.Controllers
{
    /// <summary>
    /// [Authorize][AuthorizeForScopes(Scopes = new[] { GraphConstants.SitesManageAll, GraphConstants.SitesReadWriteAll, GraphConstants.MailboxSettingsReadWrite })]

    /// </summary>
    public class GlobalController : GraphController
    {
        private readonly string[] _sharePointScopes =
            new[] { GraphConstants.SitesManageAll , GraphConstants.SitesReadWriteAll, GraphConstants.MailboxSettingsReadWrite };

        public GlobalController(
            GraphServiceClient graphClient,
            ITokenAcquisition tokenAcquisition,
            ILogger<HomeController> logger) : base(graphClient, tokenAcquisition, logger)
        {
        }


        public async Task<IActionResult> Index()
        {
            await EnsureScopes(_sharePointScopes);

            try
            {

                var mailboxSetting = await GetViewMailBoxSettingsAsync();
                var SharePointModel = await GetSharePointModelAsync();


                var graphModel = new GraphModel
                {
                    MailBoxSettings = mailboxSetting,
                    SharePoint = SharePointModel
                };

                return View(graphModel);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return View()
                    .WithError("Error getting calendar view", ex.Message);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("addsites") });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View().WithError(ex.Message);
            }

        }

        [HttpPost]
        public async Task<IActionResult> Mail(string ScheduledStartDateTime_Date, string ScheduledStartDateTime_Time,
                                       string ScheduledEndDateTime_Date, string ScheduledEndDateTime_Time,string Content)
        {
            try
            {
                DateTimeOffset ScheduledStartDateTime = DateTimeOffset.Parse($"{ScheduledStartDateTime_Date}T{ScheduledStartDateTime_Time}");

                DateTimeOffset ScheduledEndDateTime = DateTimeOffset.Parse($"{ScheduledEndDateTime_Date}T{ScheduledEndDateTime_Time}");

                var requestBody = new MailboxSettings
                {
                    AutomaticRepliesSetting = new AutomaticRepliesSetting
                    {
                        Status = AutomaticRepliesStatus.Scheduled,
                        ScheduledStartDateTime = ScheduledStartDateTime.ToDateTimeTimeZone(),
                        ScheduledEndDateTime = ScheduledEndDateTime.ToDateTimeTimeZone(),
                        ExternalReplyMessage = Content,
                        InternalReplyMessage = Content
                    },
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "@odata.context" , "https://graph.microsoft.com/beta/$metadata#Me/mailboxSettings"
                        },
                    },
                };

                var response = await _graphClient.Me.MailboxSettings.PatchAsync(requestBody);

                //ViewBag.ScheduledStartDateTime = response?.AutomaticRepliesSetting?.ScheduledStartDateTime.ToDateTimeOffset();
                //ViewBag.ScheduledEndDateTime = response?.AutomaticRepliesSetting?.ScheduledEndDateTime.ToDateTimeOffset();

                var model = new ViewMailBoxSettings
                {
                    ScheduledStartDateTime = response?.AutomaticRepliesSetting?.ScheduledStartDateTime.ToDateTimeOffset(),
                    ScheduledEndDateTime = response?.AutomaticRepliesSetting?.ScheduledEndDateTime.ToDateTimeOffset(),
                    ExternalReplyMessage = response?.AutomaticRepliesSetting?.ExternalReplyMessage,
                };

                var appToPublish = new AppToPublish
                {
                    Application = "Outlook",
                    Channel = new List<string>(),
                    Status = "Published",
                    Message = response?.AutomaticRepliesSetting?.ExternalReplyMessage,

                    Publish_Schedule = new PublishSchedule
                    {
                        From_Date = response?.AutomaticRepliesSetting?.ScheduledStartDateTime.ToDateTime() ?? throw new InvalidDataException("There is no \"From date.\""),
                        To_Date = response?.AutomaticRepliesSetting?.ScheduledEndDateTime.ToDateTime() ?? throw new InvalidDataException("There is no \"To date.\"")
                    },
                    Created_Date = DateTime.Now

                };

                return Ok( new
                {
                    status = StatusCodes.Status200OK,
                    result = JsonConvert.SerializeObject(appToPublish),
                    message = "Mailbox Setting Updated Successfully!"   
                });
            }
            catch (ServiceException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }

        }

        private async Task<ViewMailBoxSettings> GetViewMailBoxSettingsAsync()
        {
            var mailBox = await _graphClient.Me.MailboxSettings.GetAsync();

            var mailboxSetting = new ViewMailBoxSettings
            {
                ScheduledStartDateTime = mailBox?.AutomaticRepliesSetting?.ScheduledStartDateTime.ToDateTimeOffset(),
                ScheduledEndDateTime = mailBox?.AutomaticRepliesSetting?.ScheduledEndDateTime.ToDateTimeOffset(),
                ExternalReplyMessage = mailBox?.AutomaticRepliesSetting?.ExternalReplyMessage

            };

            return mailboxSetting;

        }

        private async Task<SharePointModel> GetSharePointModelAsync()
        {
            try
            {

                var result = await _graphClient.Sites["48144a5f-1f50-4279-b6a0-acaa27031b29"]
                                    .Lists["0d760bdb-e0e5-439c-895c-93351b064525"]
                                    .Items
                                    .GetAsync(requestConfiguration =>
                                    {
                                        requestConfiguration.QueryParameters.Expand = new string[] { "Fields" };
                                    });

                var values = result?.Value?.ToList();

                var fields = values?.Select(f => f.Fields).ToList();

                var fielsData = fields?.Select(f => f.AdditionalData).ToList();

                var userMail = User.GetUserGraphEmail();

                var filteredFields = fields?
         .Where(f => f.AdditionalData != null &&
                     f.AdditionalData.ContainsKey("Title") &&
                     f.AdditionalData["Title"]?.ToString().Trim().ToLower() == userMail.Trim().ToLower())
         .FirstOrDefault();


                var leaveUpdates = new SharePointModel();

                leaveUpdates.Id = filteredFields?.Id!;

                leaveUpdates.MapFromDictionary(filteredFields!.AdditionalData);

                return leaveUpdates;
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        [HttpPost]
        public async Task<IActionResult> addSites(SharePointModel sharePointModel)
        {

            try
            {

                var additionalData = sharePointModel.ToDictionary();

                var requestBody = new FieldValueSet
                {
                    AdditionalData = additionalData,
                };

                var result = await _graphClient.Sites["48144a5f-1f50-4279-b6a0-acaa27031b29"].Lists["0d760bdb-e0e5-439c-895c-93351b064525"].Items[sharePointModel.Id].Fields.PatchAsync(requestBody);

                var data = result?.AdditionalData;

                var model = new SharePointModel();

                model.MapFromDictionary(data);

                return Ok(new
                {
                    status = StatusCodes.Status200OK,
                    result = JsonConvert.SerializeObject(model),
                    message = "SharePoint Updated Successfully!"
                });
            }
            catch (ServiceException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return RedirectToAction("Index");
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("Index") });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return RedirectToAction("Index").WithError(ex.Message);
            }


        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Mail(DateTimeOffset startDate,DateTimeOffset endDate, string message)
        {
            //await EnsureScopes(_sharePointScopes);

            try
            {
                //var mailboxSetting = await GetViewMailBoxSettingsAsync();

                var mailboxSetting = new ViewMailBoxSettings
                {
                    ScheduledStartDateTime = startDate,
                    ScheduledEndDateTime = endDate,
                    ExternalReplyMessage = message
                };

                return PartialView("_MailSettings", mailboxSetting);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return PartialView("_MailSettings")
                    .WithError("Error getting calendar view", ex.Message);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("addsites") });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return PartialView("_MailSettings");
            }
        }

        [HttpGet]
        public async Task<IActionResult> Sharepoint(DateTimeOffset startDate, DateTimeOffset endDate, string message)
        {
            await EnsureScopes(_sharePointScopes);

            try
            {
                string month = startDate.ToString("MMM"); // e.g., "Jan", "Feb"

                var startMonth = startDate.Month;
                var endMonth = endDate.Month;

                //string startDay = GetDayWithSuffix(startDate.Day);
                //string endDay = GetDayWithSuffix(endDate.Day); 

                var startDay = startDate.Day;
                var endDay = endDate.Day;

                string dayRange = $"{startMonth}/{startDay} - {endMonth}/{endDay}";

                var sharePointModel = await GetSharePointModelAsync();

                var property = typeof(SharePointModel).GetProperty(month);
                if (property != null)
                {
                    string currentValue = property.GetValue(sharePointModel) as string;

                    string newValue = string.IsNullOrEmpty(currentValue)
                        ? dayRange
                        : $"{currentValue}, {dayRange}";

                    // Update the property value
                    property.SetValue(sharePointModel, newValue);
                }
                else
                {
                    throw new InvalidOperationException($"Property '{month}' not found on SharePointModel.");
                }

                //typeof(SharePointModel).GetProperty(month)?.SetValue(sharePointModel, dayRange);

                return PartialView("_SharePoint", sharePointModel);
            }
            catch (ServiceException ex)
            {
                InvokeAuthIfNeeded(ex);

                return BadRequest(ex.Message);
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("addsites") });
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return BadRequest(ex.Message);
            }



        }

        private string GetDayWithSuffix(int day)
        {
            if (day % 10 == 1 && day % 100 != 11) return $"{day}st";
            if (day % 10 == 2 && day % 100 != 12) return $"{day}nd";
            if (day % 10 == 3 && day % 100 != 13) return $"{day}rd";
            return $"{day}th";
        }

    }
}
