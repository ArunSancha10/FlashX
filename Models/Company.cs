using System.ComponentModel.DataAnnotations;

namespace outofoffice.Models
{
    public class Company
    {
        [Key]
        public Guid Company_ID { get; set; }
        public string Company_Nm { get; set; }

        // Navigation property
        public ICollection<Group> Groups { get; set; }
    }
}
