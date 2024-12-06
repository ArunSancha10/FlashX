using System.ComponentModel.DataAnnotations;

namespace outofoffice.Models
{
    public class UserAppMessage
    {
 
        public Guid UAID { get; set; }

        public Guid Company_ID { get; set; }
        public string Group_ID { get; set; }
        public string User_ID { get; set; }
        public string Message_Type { get; set; }
        public DateTime OOO_From_Dt { get; set; }
        public DateTime OOO_To_Dt { get; set; }
        public string Message_Txt { get; set; }
        public string Apps_To_Publish { get; set; }
        public string Publish_Status { get; set; }
        public DateTime CreatedDate { get; set; }

        // Navigation properties
        public Group Group { get; set; }
        public bool IsDeleted { get; set; } = false;
    }

    public class AppToPublish
    {
        public string Application { get; set; }
        public List<string> Channel { get; set; }
        public string Status { get; set; }
        public string Message { get; set; }
        public PublishSchedule Publish_Schedule { get; set; }
        public DateTime Created_Date { get; set; }
        public string TimeOffType { get; set; }
        public string ZohoTimeOffID { get; set; }
        public string Reason { get; set; }
        public string SlackPresence { get; set; }
        public string SlackExpireDuration { get; set; }
        public string TeamsStatusMessage { get; set; }
        public string TeamsExpireDuration { get; set; }
        //public SharePointColumn SharePointColumns { get; set; }

        public string JobId { get; set; } = string.Empty;

    }

    public class PublishSchedule
    {
        public DateTime From_Date { get; set; }
        public DateTime To_Date { get; set; }
    }

    public class ResponseItem
    {
        public string uaid { get; set; }
        public string company_ID { get; set; }
        public string group_ID { get; set; }
        public string user_ID { get; set; }
        public string message_Type { get; set; }
        public string ooO_From_Dt { get; set; }
        public string ooO_To_Dt { get; set; }
        public string message_Txt { get; set; }
        public string apps_To_Publish { get; set; }
        public string publish_Status { get; set; }
        public string createdDate { get; set; }
        public object group { get; set; }
        public List<AppToPublish> AppsToPublish { get; set; }
    }

    //public class SharePointColumn
    //{
    //    public string Jan { get; set; }
    //    public string Feb { get; set; }
    //    public string Mar { get; set; }
    //    public string Apr { get; set; }
    //    public string May { get; set; }
    //    public string Jun { get; set; }
    //    public string Jul { get; set; }
    //    public string Aug { get; set; }
    //    public string Sep { get; set; }
    //    public string Oct { get; set; }
    //    public string Nov { get; set; }
    //    public string Dce { get; set; }
    //}
}
