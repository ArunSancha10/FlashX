using Azure.Identity;
using Microsoft.Graph;
using Microsoft.Graph.Models;
using Microsoft.Identity.Web;
using Microsoft.Graph.Users.Item.Presence.SetPresence;
using outofoffice.Repositories.Interfaces;
using Mysqlx.Crud;
using outofoffice.Models;
using outofoffice.Servicess;

using outofoffice.Service;
using outofoffice.Controllers;
using System.Text.Json.Nodes;

namespace outofoffice.Jobs
{
    /// <summary>
    /// 
    /// </summary>
    public class HangfireJobService
    {
        private readonly ILogger<HangfireJobService> _logger;
        private readonly GraphServiceClient graphClient;
        private readonly ITokenAcquisition tokenAcquisition;
        private readonly IConfiguration configuration;
        private readonly IZohoTimeOffService _zohoService;
        private readonly ISlackService _slackService;
        private readonly TeamsServices _teamService;

       

        public HangfireJobService(ILogger<HangfireJobService> logger, GraphServiceClient graphClient, ITokenAcquisition tokenAcquisition, IConfiguration configuration, IZohoTimeOffService zohoService , ISlackService slackService, TeamsServices teamService)
        {
            _logger = logger;
            this.graphClient = graphClient;
            this.tokenAcquisition = tokenAcquisition;
            this.configuration = configuration;
            _zohoService = zohoService;
            _slackService = slackService;
            _teamService = teamService;
            
        }

        public void LogErrorJob()
        {
            _logger.LogError("Scheduling Check Job for Hangfire");
        }

        public void ScheduleZohoTimeOff(UpdateTimeOff updateTimeOff)
        {
            //var accessToken = _zohoService.getAccessCode().GetAwaiter().GetResult();

            //_zohoService.UpdateTimeOffAsync(updateTimeOff, accessToken);
        }

        public void ScheduleSlackStatus( string sts , string presence, string expiry, string slack_message, string slack_channels, string emoji, string authToken, string endDate)
        {

            var accessToken = _slackService.update_Profilestatus( sts, presence, expiry, slack_message, slack_channels, emoji, authToken, endDate).GetAwaiter().GetResult();

        }

        public void ScheduleTeamsStatus(string availability,string activity,string expiry,string StatuMsg,string Teams_Token,string ChannalMsg,string Channals, string Teams_RefreshToken,string user_ID)
        {
            Console.WriteLine("in Hangfire");

            // _teamService.SetUserPresenceAsync(availability, activity, Teams_Token).GetAwaiter().GetResult();            
            _teamService.CallApis(availability, activity, expiry, StatuMsg, ChannalMsg, Channals,Teams_Token, Teams_RefreshToken, user_ID).GetAwaiter().GetResult();

        }


    }


}
