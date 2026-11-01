using Hostel_hub.Models;
using System.ComponentModel.DataAnnotations;
using static Hostel_hub.Models.Complaint;

namespace Hostel_hub.ViewModels
{
    public class ComplaintCreateViewModel
    {
        [Required, MaxLength(150)]
        public string Title { get; set; } = string.Empty;

        [Required, MaxLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public ComplaintCategory Category { get; set; }
    }
}