using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.ViewModels
{
    public class LeaveRequestViewModel
    {
        public int LeaveRequestId { get; set; }

        public int UserId { get; set; }

        [Required(ErrorMessage = "From date is required")]
        [Display(Name = "From Date")]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "To date is required")]
        [Display(Name = "To Date")]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; } = DateTime.Today;

        [Required(ErrorMessage = "Reason is required")]
        
        [Display(Name = "Reason for Leave")]
        public string Reason { get; set; } = string.Empty;

        public string Status { get; set; } = "Pending";

        [Display(Name = "Requested Date")]
        public DateTime RequestedDate { get; set; }

        [Display(Name = "Processed Date")]
        public DateTime? ProcessedDate { get; set; }

        [Display(Name = "Processed By")]
        public string? ProcessedByName { get; set; }

        [Display(Name = "Admin Comments")]
        [StringLength(500)]
        public string? AdminComments { get; set; }

        [Display(Name = "Employee Name")]
        public string? EmployeeName { get; set; }

        public int TotalDays { get; set; }
    }
}