using Hostel_hub.Models;
using System.ComponentModel.DataAnnotations;
using static Hostel_hub.Models.Complaint;

namespace Hostel_hub.ViewModels
{
    public class ComplaintEditViewModel
    {
        public int ComplaintId { get; set; }

        [Required(ErrorMessage = "Title is required.")]
        [MaxLength(150, ErrorMessage = "Title cannot exceed 150 characters.")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Description is required.")]
        [MaxLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        public string Description { get; set; } = string.Empty;

        [Required(ErrorMessage = "Category is required.")]
        public ComplaintCategory Category { get; set; }
    }
}
