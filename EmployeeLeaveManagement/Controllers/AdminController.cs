using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Helpers;
using EmployeeLeaveManagement.Hubs;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Controllers
{
    public class AdminController : BaseController
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificationHub> _hubContext;

        public AdminController(ApplicationDbContext context, IHubContext<NotificationHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        // Override to check if user is admin
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!IsAdmin)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }

        // GET: Admin/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalEmployees = await _context.Users.Where(u => u.Role == "Employee").CountAsync(),
                ActiveEmployees = await _context.Users.Where(u => u.Role == "Employee" && u.IsActive).CountAsync(),
                TotalLeaveRequests = await _context.LeaveRequests.CountAsync(),
                PendingRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Pending").CountAsync(),
                ApprovedRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Approved").CountAsync(),
                RejectedRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Rejected").CountAsync(),
                RecentLeaveRequests = await _context.LeaveRequests
                    .Include(lr => lr.User)
                    .OrderByDescending(lr => lr.RequestedDate)
                    .Take(10)
                    .Select(lr => new LeaveRequestViewModel
                    {
                        LeaveRequestId = lr.LeaveRequestId,
                        EmployeeName = lr.User.FullName,
                        FromDate = lr.FromDate,
                        ToDate = lr.ToDate,
                        Reason = lr.Reason,
                        Status = lr.Status,
                        RequestedDate = lr.RequestedDate,
                        TotalDays = lr.TotalDays
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        // GET: Admin/Employees
        public async Task<IActionResult> Employees(string searchTerm)
        {
            var query = _context.Users.Where(u => u.Role == "Employee");

            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                query = query.Where(u => u.FullName.Contains(searchTerm) || u.Email.Contains(searchTerm));
                ViewBag.SearchTerm = searchTerm;
            }

            var employees = await query
                .OrderBy(u => u.FullName)
                .Select(u => new EmployeeViewModel
                {
                    UserId = u.UserId,
                    FullName = u.FullName,
                    Email = u.Email,
                    IsActive = u.IsActive,
                    CreatedDate = u.CreatedDate
                })
                .ToListAsync();

            return View(employees);
        }

        // GET: Admin/CreateEmployee
        [HttpGet]
        public IActionResult CreateEmployee()
        {
            return View();
        }

        // POST: Admin/CreateEmployee
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateEmployee(EmployeeViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                if (await _context.Users.AnyAsync(u => u.Email == model.Email))
                {
                    ModelState.AddModelError("Email", "Email already exists.");
                    return View(model);
                }

                var user = new User
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Password = model.Password ?? "emp123",
                    Role = "Employee",
                    IsActive = model.IsActive,
                    CreatedDate = DateTime.Now
                };

                _context.Users.Add(user);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee created successfully!";
                return RedirectToAction(nameof(Employees));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while creating employee.");
                return View(model);
            }
        }

        // POST: Admin/DeactivateEmployee/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeactivateEmployee(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != "Employee")
                {
                    return NotFound();
                }

                user.IsActive = false;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee deactivated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while deactivating employee.";
            }

            return RedirectToAction(nameof(Employees));
        }

        // POST: Admin/ActivateEmployee/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ActivateEmployee(int id)
        {
            try
            {
                var user = await _context.Users.FindAsync(id);
                if (user == null || user.Role != "Employee")
                {
                    return NotFound();
                }

                user.IsActive = true;
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Employee activated successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while activating employee.";
            }

            return RedirectToAction(nameof(Employees));
        }

        // GET: Admin/LeaveRequests
        public async Task<IActionResult> LeaveRequests(string status)
        {
            var query = _context.LeaveRequests.Include(lr => lr.User).AsQueryable();

            if (!string.IsNullOrWhiteSpace(status))
            {
                query = query.Where(lr => lr.Status == status);
                ViewBag.Status = status;
            }

            var leaveRequests = await query
                .OrderByDescending(lr => lr.RequestedDate)
                .Select(lr => new LeaveRequestViewModel
                {
                    LeaveRequestId = lr.LeaveRequestId,
                    UserId = lr.UserId,
                    EmployeeName = lr.User.FullName,
                    FromDate = lr.FromDate,
                    ToDate = lr.ToDate,
                    Reason = lr.Reason,
                    Status = lr.Status,
                    RequestedDate = lr.RequestedDate,
                    ProcessedDate = lr.ProcessedDate,
                    AdminComments = lr.AdminComments,
                    TotalDays = lr.TotalDays
                })
                .ToListAsync();

            return View(leaveRequests);
        }

        // GET: Admin/LeaveDetails/5
        public async Task<IActionResult> LeaveDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.User)
                .Include(lr => lr.ProcessedByAdmin)
                .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            var viewModel = new LeaveRequestViewModel
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
                UserId = leaveRequest.UserId,
                EmployeeName = leaveRequest.User.FullName,
                FromDate = leaveRequest.FromDate,
                ToDate = leaveRequest.ToDate,
                Reason = leaveRequest.Reason,
                Status = leaveRequest.Status,
                RequestedDate = leaveRequest.RequestedDate,
                ProcessedDate = leaveRequest.ProcessedDate,
                ProcessedByName = leaveRequest.ProcessedByAdmin?.FullName,
                AdminComments = leaveRequest.AdminComments,
                TotalDays = leaveRequest.TotalDays
            };

            return View(viewModel);
        }

        // POST: Admin/ApproveLeave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApproveLeave(int id, string adminComments)
        {
            try
            {
                var leaveRequest = await _context.LeaveRequests
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id);

                if (leaveRequest == null)
                {
                    return NotFound();
                }

                leaveRequest.Status = "Approved";
                leaveRequest.ProcessedDate = DateTime.Now;
                leaveRequest.ProcessedBy = CurrentUserId;
                leaveRequest.AdminComments = adminComments;

                // Add audit log
                var auditLog = new AuditLog
                {
                    LeaveRequestId = id,
                    ActionBy = CurrentUserId.Value,
                    Action = "Approved",
                    Comments = adminComments,
                    ActionDate = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);

                await _context.SaveChangesAsync();

                
                var message = $"Your leave request from {leaveRequest.FromDate:dd-MMM-yyyy} to {leaveRequest.ToDate:dd-MMM-yyyy} has been APPROVED!";
                await _hubContext.Clients.Group($"user_{leaveRequest.UserId}").SendAsync("ReceiveNotification", message, "success");

                TempData["SuccessMessage"] = "Leave request approved successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while approving leave request.";
            }

            return RedirectToAction(nameof(LeaveRequests));
        }

        // POST: Admin/RejectLeave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejectLeave(int id, string adminComments)
        {
            try
            {
                var leaveRequest = await _context.LeaveRequests
                    .Include(lr => lr.User)
                    .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id);

                if (leaveRequest == null)
                {
                    return NotFound();
                }

                leaveRequest.Status = "Rejected";
                leaveRequest.ProcessedDate = DateTime.Now;
                leaveRequest.ProcessedBy = CurrentUserId;
                leaveRequest.AdminComments = adminComments;

                // Add audit log
                var auditLog = new AuditLog
                {
                    LeaveRequestId = id,
                    ActionBy = CurrentUserId.Value,
                    Action = "Rejected",
                    Comments = adminComments,
                    ActionDate = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);

                await _context.SaveChangesAsync();

                // Send SignalR notification to the employee
                var message = $"Your leave request from {leaveRequest.FromDate:dd-MMM-yyyy} to {leaveRequest.ToDate:dd-MMM-yyyy} has been REJECTED.";
                await _hubContext.Clients.Group($"user_{leaveRequest.UserId}").SendAsync("ReceiveNotification", message, "danger");

                TempData["SuccessMessage"] = "Leave request rejected successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while rejecting leave request.";
            }

            return RedirectToAction(nameof(LeaveRequests));
        }

        
        public async Task<IActionResult> Reports()
        {
            var viewModel = new AdminDashboardViewModel
            {
                TotalEmployees = await _context.Users.Where(u => u.Role == "Employee").CountAsync(),
                ActiveEmployees = await _context.Users.Where(u => u.Role == "Employee" && u.IsActive).CountAsync(),
                TotalLeaveRequests = await _context.LeaveRequests.CountAsync(),
                PendingRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Pending").CountAsync(),
                ApprovedRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Approved").CountAsync(),
                RejectedRequests = await _context.LeaveRequests.Where(lr => lr.Status == "Rejected").CountAsync()
            };

            return View(viewModel);
        }
    }
}