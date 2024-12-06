using Newtonsoft.Json;

namespace outofoffice.Models
{
    public class UpdateTimeOff
    {
        public DateTime? startDate { get; set; }
        public DateTime? endDate { get; set; }
        public string Description { get; set; }
        public string Id { get; set; }
        public string ZohoTimeOffID { get; set; }
        public string Reason { get; set; }
        public string TimeOffType { get; set; }
    }
}
