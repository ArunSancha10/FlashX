using outofoffice.Models;

namespace outofoffice.Servicess
{
    public interface ISlackService
    {
        Task<string> update_Profilestatus( string sts, string presence, string expiry, string slack_message, string slack_channels, string emoji, string token, string endDate);

        Task<Dictionary<string,string>> getChannels(string token);
    }

    public static class GlobalVariables
    {
        public static string SharedValue { get; set; }
        public static string Slack_Token { get; set; }
    }

    //public class slackAppConfig
    //{
    //    public string Client_Id { get; set; }
    //    public string Client_Secret { get; set; }
    //    public string Scopes { get; set; }
    //    public string Redirect_Uri { get; set;}
    //    public string User_Id { get; set; }

    //}
}
