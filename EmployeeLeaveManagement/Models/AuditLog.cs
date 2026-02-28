using System.ComponentModel.DataAnnotations;

namespace EmployeeLeaveManagement.Models
{
    public class AuditLog
    {
        [Key]
        public int AuditLogId { get; set; }

        public int LeaveRequestId { get; set; }

        public int ActionBy { get; set; }

        [Required]
        [StringLength(50)]
        public string Action { get; set; } // "Approved", "Rejected", "Created"

        public DateTime ActionDate { get; set; } = DateTime.Now;

        public string? Comments { get; set; }

        // Navigation Properties
        public virtual LeaveRequest LeaveRequest { get; set; }
        public virtual User User { get; set; }
    }
}