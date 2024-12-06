using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using outofoffice.App_code;
using outofoffice.Service;
using outofoffice.Servicess;

namespace outofoffice.Controllers
{
    public class PartialViewController : Controller
    {
        private readonly ISlackService _slackService;

        private readonly OOODbContext _context;

        
        private readonly TeamsServices _teamsServices; // Updated to follow naming conventions and use an interface

        // Constructor to inject the service
        public PartialViewController(TeamsServices teamsServices, ISlackService slackService, OOODbContext context)
        {
            _teamsServices = teamsServices;
             _slackService = slackService;
            _context = context;
        }

        public async Task<IActionResult> slackView()
        {
            // Ensure that header/footer is hidden by default for partial views
            ViewData["ShowHeaderFooter"] = false;
            //string oauthtoken = GlobalVariables.SharedValue;

            //var OAuth_Token = await _context.MessageAppLists;

            // Fetch Access_Token_User_ID for Slack
            var OAuth_Token = await _context.MessageAppLists
                .Where(m => m.App_Nm == "Slack")
                .Select(m => m.Access_Token_User_ID)
                .FirstOrDefaultAsync();

            //string oauthtoken = ViewData["Slack_OAuthToken"]?.ToString(); //xoxp-8000046665106-8022844823168-8041647985735-57296bf0186b43461e5379bda7953e5a
            var result = await _slackService.getChannels(OAuth_Token);
                ViewData["SlackChannelData"] = result;
                return View();
           
        }
        public async Task<IActionResult> TeamsView()
        {
            ViewData["ShowHeaderFooter"] = false;


            string? userEmail = HttpContext.Session.GetString("UserEmail");

            string? Teams_Token = await _context.MessageAppLists
                                   .Where(m => m.App_Nm == "Microsoft" && m.UserEmail == userEmail)
                                   .Select(m => m.Access_Token_User_ID)
                                   .FirstOrDefaultAsync();

            // Fetch channel list asynchronously from the service
            var channelDictionary = await _teamsServices.GetAllChannelsForTeamsAsync(Teams_Token);

            // Pass the key-value dictionary to the view
            ViewData["ChannelDictionary"] = channelDictionary;

            return View();
        }

        public IActionResult zohoView()
        {
            ViewData["ShowHeaderFooter"] = false;
            return View();
        }

        public IActionResult outlookView()
        {
            ViewData["ShowHeaderFooter"] = false;
            return View();

        }
        public IActionResult sharepointView()
        {
            ViewData["ShowHeaderFooter"] = false;
            return View();
        }

    }
}
