using System.ComponentModel.DataAnnotations;

namespace outofoffice.Models
{
    public class UpdateSites
    {
        [Required(ErrorMessage = "Title is required")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Duration is required")]
        public string Duration { get; set; }

        [Required(ErrorMessage = "End Date is required")]
        public string EndDate { get; set; }

        [Required(ErrorMessage = "Reason is required")]
        public string Reason { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public string Status { get; set; }
    }
}
