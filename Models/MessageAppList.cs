using System.ComponentModel.DataAnnotations;

namespace outofoffice.Models
{
    public class MessageAppList
    {
       
        public Guid MAID { get; set; }
        public Guid Company_ID { get; set; }
        public string Group_ID { get; set; }
        public string App_Nm { get; set; }
        public string App_Desc { get; set; }
        public string App_Channels { get; set; }
        public string Access_Token_User_ID { get; set; }
        public string Access_Token_Txt { get; set; }
        public bool Publish_Immd_Flag { get; set; }

        public string UserID { get; set; }
        public string UserEmail { get; set; }
        public Group Group { get; set; }
    }
}
