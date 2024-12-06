using Microsoft.Graph.Models;
using System.ComponentModel;

namespace outofoffice.Models
{
    public class ViewMailBoxSettings
    {
        public AutomaticRepliesStatus automaticRepliesStatus { get; set; }

        public DateTimeOffset? ScheduledStartDateTime { get; set; }
        public DateTimeOffset? ScheduledEndDateTime { get; set; }

        public string TimeZone { get; set; }
        public string ExternalReplyMessage { get; set; }
    }

    public class UpdateMailBox
    {
        public AutomaticRepliesStatus Status { get; set; }

        public DateTimeTimeZone? ScheduledStartDateTime { get; set; }
        public DateTimeTimeZone? ScheduledEndDateTime { get; set; }

        public string ExternalReplyMessage { get; set; }
        public string InternalReplyMessage { get; set; }

    }
}
