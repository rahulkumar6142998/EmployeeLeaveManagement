namespace EmployeeLeaveManagement.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalEmployees { get; set; }
        public int ActiveEmployees { get; set; }
        public int TotalLeaveRequests { get; set; }
        public int PendingRequests { get; set; }
        public int ApprovedRequests { get; set; }
        public int RejectedRequests { get; set; }
        public List<LeaveRequestViewModel> RecentLeaveRequests { get; set; } = new();
    }
}