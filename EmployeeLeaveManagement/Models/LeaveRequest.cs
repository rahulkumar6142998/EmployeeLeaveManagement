using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace EmployeeLeaveManagement.Models
{
    public class LeaveRequest
    {
        [Key]
        public int LeaveRequestId { get; set; }

        [Required]
        public int UserId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime FromDate { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime ToDate { get; set; }

        [Required]
        [StringLength(500)]
        public string Reason { get; set; }

        [Required]
        [StringLength(20)]
        public string Status { get; set; } = "Pending"; 

        public DateTime RequestedDate { get; set; } = DateTime.Now;

        public DateTime? ProcessedDate { get; set; }

        public int? ProcessedBy { get; set; }

        public string? AdminComments { get; set; }

 
        [ForeignKey("UserId")]
        public virtual User User { get; set; }

        [ForeignKey("ProcessedBy")]
        public virtual User? ProcessedByAdmin { get; set; }

    
        [NotMapped]
        public int TotalDays => (ToDate - FromDate).Days + 1;
    }
}