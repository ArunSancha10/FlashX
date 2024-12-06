using System.ComponentModel.DataAnnotations;

namespace outofoffice.Models
{
    public class Group
    {
        [Key]
        public string Group_ID { get; set; }
        
        public Guid Company_ID { get; set; }
        public string Group_Nm { get; set; }

        // Navigation properties
        public Company Company { get; set; }
        public ICollection<MessageAppList> MessageApps { get; set; }
        public ICollection<UserAppMessage> UserMessages { get; set; }
    }
}
