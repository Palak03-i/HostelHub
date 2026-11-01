using System.ComponentModel.DataAnnotations;
using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class HostelViewModel
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public HostelType Type { get; set; }
    }
}