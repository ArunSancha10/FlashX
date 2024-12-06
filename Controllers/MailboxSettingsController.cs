using Microsoft.AspNetCore.Mvc;
using Microsoft.Graph;
using Microsoft.Identity.Web;
using Microsoft.Graph.Models;
using Newtonsoft.Json;
using outofoffice.Models;
using Microsoft.AspNetCore.Authentication;
using Hangfire;
using System.Linq.Expressions;
using System;
using outofoffice.Jobs;
using outofoffice.Repositories.Interfaces;
using outofoffice.Graph;


namespace outofoffice.Controllers
{
    public class MailboxSettingsController : Controller
    {
        private readonly GraphServiceClient graphClient;
        private readonly ILogger<MailboxSettingsController> logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MailboxSettingsController"/> class.
        /// </summary>
        /// <param name="graphClient">An authenticated <see cref="GraphServiceClient"/>.</param>
        /// <param name="logger">An <see cref="ILogger"/> to use for logging.</param>
        public MailboxSettingsController(
            GraphServiceClient graphClient,
            ILogger<MailboxSettingsController> logger)
        {
            this.graphClient = graphClient;
            this.logger = logger;
        }

        /// <summary>
        /// Loads the mailbox settings view.
        /// </summary>
        /// <returns>A view with current mailbox settings.</returns>
        [AuthorizeForScopes(Scopes = ["MailboxSettings.ReadWrite"])]
        public async Task<IActionResult> Index()
        {
            try
            {
                var mailboxSettings = await graphClient.Me
                    .MailboxSettings
                    .GetAsync();

                var result = JsonConvert.SerializeObject(mailboxSettings, Formatting.Indented);

                ViewBag.MailBoxSettings = result;

                return View(mailboxSettings);
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View();
            }
        }


        /// <summary>
        /// Displays the form for updating automatic replies settings.
        /// </summary>
        /// <returns>The form view.</returns>
        /// 
        [HttpGet]
        [AuthorizeForScopes(Scopes = new[] { "MailboxSettings.ReadWrite" })]
        public async Task<IActionResult> Edit()
        {
            try
            {
                var mailboxSettings = await graphClient.Me
                    .MailboxSettings
                    .GetAsync();

                var model = new ViewMailBoxSettings
                {
                    automaticRepliesStatus = mailboxSettings?.AutomaticRepliesSetting?.Status ?? AutomaticRepliesStatus.Disabled,

                    ScheduledStartDateTime = DateTimeOffset.Parse(mailboxSettings?.AutomaticRepliesSetting?.ScheduledStartDateTime?.DateTime),

                    ScheduledEndDateTime = DateTimeOffset.Parse(mailboxSettings?.AutomaticRepliesSetting?.ScheduledEndDateTime?.DateTime),

                    TimeZone = mailboxSettings?.AutomaticRepliesSetting?.ScheduledStartDateTime?.TimeZone ?? "UTC"
                };

                ViewBag.Response = JsonConvert.SerializeObject(mailboxSettings, Formatting.Indented);

                // Pass the model to the view
                return View(model);
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View();
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("Edit") });
            }
            catch (Exception ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View().WithError(ex.Message);
            }
        }


        /// <summary>
        /// Updates mailbox settings such as automatic replies.
        /// </summary>
        /// <param name="mailboxSettings">The <see cref="MailboxSettings"/> instance with updated values.</param>
        /// <returns>A redirect to the settings view with the result.</returns>
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = ["MailboxSettings.ReadWrite"])]
        public async Task<IActionResult> Edit(ViewMailBoxSettings mailboxSettings)
        {
            try
            {
                var requestBody = new MailboxSettings
                {
                    AutomaticRepliesSetting = new AutomaticRepliesSetting
                    {
                        Status = (AutomaticRepliesStatus)mailboxSettings.automaticRepliesStatus,
                        ScheduledStartDateTime = new DateTimeTimeZone
                        {
                            DateTime = mailboxSettings.ScheduledStartDateTime.ToString(),
                            TimeZone = mailboxSettings.TimeZone,
                        },
                        ScheduledEndDateTime = new DateTimeTimeZone
                        {
                            DateTime = mailboxSettings.ScheduledEndDateTime.ToString(),
                            TimeZone = mailboxSettings.TimeZone,
                        },
                    },
                    AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "@odata.context" , "https://graph.microsoft.com/beta/$metadata#Me/mailboxSettings"
                        },
                    },
                };

                var respnse = await graphClient.Me.MailboxSettings.PatchAsync(requestBody);

                var userId = User?.GetUserGraphId();

                //var jobId = BackgroundJob.Schedule<HangfireJobService>(service => service.UpdateMailBox(userId), TimeSpan.FromMinutes(1));

                var viewModel = new ViewMailBoxSettings
                {
                    automaticRepliesStatus = respnse?.AutomaticRepliesSetting?.Status ?? AutomaticRepliesStatus.Disabled,
                    TimeZone = respnse.AutomaticRepliesSetting.ScheduledStartDateTime.TimeZone,
                    ScheduledStartDateTime = DateTimeOffset.Parse(respnse.AutomaticRepliesSetting.ScheduledStartDateTime.DateTime),
                    ScheduledEndDateTime = DateTimeOffset.Parse(respnse.AutomaticRepliesSetting.ScheduledEndDateTime.DateTime)
                };

                return View(viewModel).WithSuccess("Success,Updated");
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error updating mailbox settings: {ex.Message}");
                return View(mailboxSettings).WithError(ex.Message);
            }
            catch (Exception ex)
            {
                logger.LogError($"Error updating mailbox settings: {ex.Message}");
                return View(mailboxSettings).WithError(ex.Message);
            }
        }
        public async Task<IActionResult> sites()
        {
            try
            {
                //var allList = await graphClient.Sites["48144a5f-1f50-4279-b6a0-acaa27031b29"].Lists.GetAsync();

                var ListCalendar = await graphClient.Sites["48144a5f-1f50-4279-b6a0-acaa27031b29"].Lists["340b5239-2f33-4de0-837a-3ceba116e1fe"].Columns.GetAsync(

                   (requestConfiguration) =>
                   {
                       requestConfiguration.QueryParameters.Select = new string[] { "DisplayName", "Id" };
                   }

                );



                ViewBag.Response = JsonConvert.SerializeObject(ListCalendar, Formatting.Indented);

                // Pass the model to the view
                return View();
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View();
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("sites") });
            }
            catch (Exception ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View().WithError(ex.Message);
            }
        }

        [HttpGet]        
        [AuthorizeForScopes(Scopes = ["Sites.ReadWrite.All", "Sites.Manage.All"])]
        public async Task<IActionResult> addSites()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [AuthorizeForScopes(Scopes = ["Sites.ReadWrite.All", "Sites.Manage.All"])]
        public async Task<IActionResult> addSites(UpdateSites updateSites)
        {

            try
            {
                var requestBody = new ListItem
                {
                    Fields = new FieldValueSet
                    {
                        AdditionalData = new Dictionary<string, object>
                    {
                        {
                            "Title" , updateSites.Title
                        },
                        {
                            "Duration" , updateSites.Duration
                        },
                        {
                            "Reason" , updateSites.Reason
                        },
                        {
                            "Status" , updateSites.Status
                        },
                    },
                    },
                };


                var result = await graphClient.Sites["48144a5f-1f50-4279-b6a0-acaa27031b29"].Lists["340b5239-2f33-4de0-837a-3ceba116e1fe"].Items.PostAsync(requestBody);


                return View();
            }
            catch (ServiceException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View();
            }
            catch (MicrosoftIdentityWebChallengeUserException ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return Challenge(new AuthenticationProperties { RedirectUri = Url.Action("addSites") });
            }
            catch (Exception ex)
            {
                logger.LogError($"Error fetching mailbox settings: {ex.Message}");
                return View().WithError(ex.Message);
            }

           
        }


    }
}
