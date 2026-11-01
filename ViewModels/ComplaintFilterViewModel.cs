using Hostel_hub.Models;

namespace Hostel_hub.ViewModels
{
    public class ComplaintFilterViewModel
    {
        public string? Search { get; set; }
        public ComplaintStatus? Status { get; set; }
        public ComplaintCategory? Category { get; set; }
        public ComplaintPriority? Priority { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int? StaffId { get; set; }

        public bool HasActiveFilters =>
            !string.IsNullOrWhiteSpace(Search) ||
            Status.HasValue ||
            Category.HasValue ||
            Priority.HasValue ||
            FromDate.HasValue ||
            ToDate.HasValue ||
            StaffId.HasValue;

        public bool IsDateRangeInvalid =>
            FromDate.HasValue && ToDate.HasValue && FromDate.Value.Date > ToDate.Value.Date;
    }
}
