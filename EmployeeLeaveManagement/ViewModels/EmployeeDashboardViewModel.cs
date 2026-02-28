namespace EmployeeLeaveManagement.ViewModels
{
    public class EmployeeDashboardViewModel
    {
        public string EmployeeName { get; set; } = string.Empty;
        public int TotalLeaveRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public int TotalApprovedDays { get; set; }
        public List<LeaveRequestViewModel> RecentLeaveRequests { get; set; } = new();
    }
}