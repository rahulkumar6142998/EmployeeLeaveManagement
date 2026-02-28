using EmployeeLeaveManagement.Data;
using EmployeeLeaveManagement.Helpers;
using EmployeeLeaveManagement.Models;
using EmployeeLeaveManagement.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EmployeeLeaveManagement.Controllers
{
    public class EmployeeController : BaseController
    {
        private readonly ApplicationDbContext _context;

        public EmployeeController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Override to check if user is employee
        public override void OnActionExecuting(Microsoft.AspNetCore.Mvc.Filters.ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            if (!IsEmployee)
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Account", null);
            }
        }

        // GET: Employee/Dashboard
        public async Task<IActionResult> Dashboard()
        {
            var userId = CurrentUserId.Value;

            var leaveRequests = await _context.LeaveRequests
                .Where(lr => lr.UserId == userId)
                .ToListAsync();

            var viewModel = new EmployeeDashboardViewModel
            {
                EmployeeName = CurrentUserName ?? "",
                TotalLeaveRequests = leaveRequests.Count,
                PendingRequests = leaveRequests.Count(lr => lr.Status == "Pending"),
                ApprovedRequests = leaveRequests.Count(lr => lr.Status == "Approved"),
                RejectedRequests = leaveRequests.Count(lr => lr.Status == "Rejected"),
                TotalApprovedDays = leaveRequests
                    .Where(lr => lr.Status == "Approved")
                    .Sum(lr => lr.TotalDays),
                RecentLeaveRequests = leaveRequests
                    .OrderByDescending(lr => lr.RequestedDate)
                    .Take(10)
                    .Select(lr => new LeaveRequestViewModel
                    {
                        LeaveRequestId = lr.LeaveRequestId,
                        FromDate = lr.FromDate,
                        ToDate = lr.ToDate,
                        Reason = lr.Reason,
                        Status = lr.Status,
                        RequestedDate = lr.RequestedDate,
                        ProcessedDate = lr.ProcessedDate,
                        AdminComments = lr.AdminComments,
                        TotalDays = lr.TotalDays
                    })
                    .ToList()
            };

            return View(viewModel);
        }

        // GET: Employee/ApplyLeave
        [HttpGet]
        public IActionResult ApplyLeave()
        {
            var model = new LeaveRequestViewModel
            {
                FromDate = DateTime.Today,
                ToDate = DateTime.Today
            };
            return View(model);
        }

        // POST: Employee/ApplyLeave
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyLeave(LeaveRequestViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var userId = CurrentUserId.Value;

                // Validate date range
                if (model.FromDate > model.ToDate)
                {
                    ModelState.AddModelError("ToDate", "To Date must be greater than or equal to From Date.");
                    return View(model);
                }

                // Validate past dates
                if (model.FromDate < DateTime.Today)
                {
                    ModelState.AddModelError("FromDate", "Cannot apply leave for past dates.");
                    return View(model);
                }

                // Check for overlapping leave requests
                var hasOverlap = await _context.LeaveRequests
                    .Where(lr => lr.UserId == userId && lr.Status != "Rejected" && lr.Status != "Cancelled")
                    .AnyAsync(lr =>
                        (model.FromDate >= lr.FromDate && model.FromDate <= lr.ToDate) ||
                        (model.ToDate >= lr.FromDate && model.ToDate <= lr.ToDate) ||
                        (model.FromDate <= lr.FromDate && model.ToDate >= lr.ToDate)
                    );

                if (hasOverlap)
                {
                    ModelState.AddModelError("", "You already have a leave request for the selected dates.");
                    return View(model);
                }

                // Create leave request
                var leaveRequest = new LeaveRequest
                {
                    UserId = userId,
                    FromDate = model.FromDate,
                    ToDate = model.ToDate,
                    Reason = model.Reason,
                    Status = "Pending",
                    RequestedDate = DateTime.Now
                };

                _context.LeaveRequests.Add(leaveRequest);
                await _context.SaveChangesAsync();

                // Add audit log
                var auditLog = new AuditLog
                {
                    LeaveRequestId = leaveRequest.LeaveRequestId,
                    ActionBy = userId,
                    Action = "Created",
                    Comments = "Leave request submitted",
                    ActionDate = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request submitted successfully!";
                return RedirectToAction(nameof(MyLeaves));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", "An error occurred while submitting leave request.");
                return View(model);
            }
        }

        // GET: Employee/MyLeaves
        public async Task<IActionResult> MyLeaves(string status)
        {
            var userId = CurrentUserId.Value;

            // Build query
            var leaveRequestsQuery = _context.LeaveRequests
                .Include(lr => lr.ProcessedByAdmin)
                .Where(lr => lr.UserId == userId);

            // Apply status filter
            if (!string.IsNullOrWhiteSpace(status))
            {
                leaveRequestsQuery = leaveRequestsQuery.Where(lr => lr.Status == status);
                ViewBag.Status = status;
            }

            var leaveRequests = await leaveRequestsQuery
                .OrderByDescending(lr => lr.RequestedDate)
                .Select(lr => new LeaveRequestViewModel
                {
                    LeaveRequestId = lr.LeaveRequestId,
                    FromDate = lr.FromDate,
                    ToDate = lr.ToDate,
                    Reason = lr.Reason,
                    Status = lr.Status,
                    RequestedDate = lr.RequestedDate,
                    ProcessedDate = lr.ProcessedDate,
                    ProcessedByName = lr.ProcessedByAdmin != null ? lr.ProcessedByAdmin.FullName : null,
                    AdminComments = lr.AdminComments,
                    TotalDays = (lr.ToDate - lr.FromDate).Days + 1
                })
                .ToListAsync();

            return View(leaveRequests);
        }

        // GET: Employee/LeaveDetails/5
        public async Task<IActionResult> LeaveDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = CurrentUserId.Value;
            var leaveRequest = await _context.LeaveRequests
                .Include(lr => lr.ProcessedByAdmin)
                .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id && lr.UserId == userId);

            if (leaveRequest == null)
            {
                return NotFound();
            }

            var viewModel = new LeaveRequestViewModel
            {
                LeaveRequestId = leaveRequest.LeaveRequestId,
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

        // POST: Employee/CancelLeave/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelLeave(int id)
        {
            try
            {
                var userId = CurrentUserId.Value;
                var leaveRequest = await _context.LeaveRequests
                    .FirstOrDefaultAsync(lr => lr.LeaveRequestId == id && lr.UserId == userId);

                if (leaveRequest == null)
                {
                    return NotFound();
                }

                // Only pending requests can be cancelled
                if (leaveRequest.Status != "Pending")
                {
                    TempData["ErrorMessage"] = "Only pending leave requests can be cancelled.";
                    return RedirectToAction(nameof(MyLeaves));
                }

                leaveRequest.Status = "Cancelled";

                // Add audit log
                var auditLog = new AuditLog
                {
                    LeaveRequestId = id,
                    ActionBy = userId,
                    Action = "Cancelled",
                    Comments = "Leave request cancelled by employee",
                    ActionDate = DateTime.Now
                };
                _context.AuditLogs.Add(auditLog);

                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Leave request cancelled successfully!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while cancelling leave request.";
            }

            return RedirectToAction(nameof(MyLeaves));
        }
    }
}