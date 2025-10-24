using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ApprovalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ApprovalController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Show pending claims for approval
        public async Task<IActionResult> Pending()
        {
            var pendingClaims = await _context.Claims
                .Where(c => c.Status == "Pending")
                .OrderBy(c => c.SubmittedDate)
                .ToListAsync();

            return View(pendingClaims);
        }

        // POST: Approve a claim
        [HttpPost]
        public async Task<IActionResult> Approve(int id)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            claim.Status = "Approved";
            claim.ApprovedDate = DateTime.Now;
            claim.ApprovedBy = "Coordinator";

            // NEW: Add status tracking
            var statusUpdate = new StatusUpdate
            {
                ClaimId = claim.Id,
                Status = "Approved",
                UpdatedBy = "Coordinator",
                Notes = "Claim approved by coordinator"
            };
            _context.StatusUpdates.Add(statusUpdate);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim approved successfully!";
            return RedirectToAction(nameof(Pending));
        }

        // POST: Reject a claim
        [HttpPost]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            var claim = await _context.Claims.FindAsync(id);
            if (claim == null)
            {
                return NotFound();
            }

            if (string.IsNullOrWhiteSpace(rejectionReason))
            {
                TempData["ErrorMessage"] = "Rejection reason is required.";
                return RedirectToAction(nameof(Pending));
            }

            claim.Status = "Rejected";
            claim.RejectionReason = rejectionReason;
            claim.ApprovedBy = "Coordinator";

            // NEW: Add status tracking
            var statusUpdate = new StatusUpdate
            {
                ClaimId = claim.Id,
                Status = "Rejected",
                UpdatedBy = "Coordinator",
                Notes = $"Rejected: {rejectionReason}"
            };
            _context.StatusUpdates.Add(statusUpdate);

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Claim rejected.";
            return RedirectToAction(nameof(Pending));
        }

        // NEW: All claims view for coordinators
        public async Task<IActionResult> AllClaims()
        {
            var allClaims = await _context.Claims
                .OrderByDescending(c => c.SubmittedDate)
                .ToListAsync();

            return View(allClaims);
        }
    }
}