using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;
using Microsoft.AspNetCore.Hosting;

namespace ContractMonthlyClaimSystem.Controllers
{
    public class ClaimController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _environment;

        public ClaimController(ApplicationDbContext context, IWebHostEnvironment environment)
        {
            _context = context;
            _environment = environment;
        }

        // GET: Show claim submission form
        public IActionResult Create()
        {
            return View();
        }

        // POST: Submit new claim - FIXED null reference issues
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Claim claim, IFormFile? supportingDocument)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // FIX: Initialize collections to avoid null references
                    claim.Status = "Pending";
                    claim.SubmittedDate = DateTime.Now;

                    // Handle file upload - FIXED null reference
                    if (supportingDocument != null && supportingDocument.Length > 0)
                    {
                        try
                        {
                            // Validate file type and size
                            var allowedExtensions = new[] { ".pdf", ".docx", ".xlsx", ".jpg", ".png" };
                            var fileExtension = Path.GetExtension(supportingDocument.FileName)?.ToLower();

                            if (string.IsNullOrEmpty(fileExtension) || !allowedExtensions.Contains(fileExtension))
                            {
                                ModelState.AddModelError("", "Only PDF, DOCX, XLSX, JPG, PNG files are allowed.");
                                return View(claim);
                            }

                            if (supportingDocument.Length > 10 * 1024 * 1024) // 10MB
                            {
                                ModelState.AddModelError("", "File size must be less than 10MB.");
                                return View(claim);
                            }

                            // Save file - FIXED null reference for WebRootPath
                            var uploadsFolder = Path.Combine(_environment.WebRootPath ?? "wwwroot", "uploads");
                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = Guid.NewGuid().ToString() + fileExtension;
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                            using (var fileStream = new FileStream(filePath, FileMode.Create))
                            {
                                await supportingDocument.CopyToAsync(fileStream);
                            }

                            claim.DocumentFileName = uniqueFileName;
                        }
                        catch (Exception fileEx)
                        {
                            ModelState.AddModelError("", $"File upload error: {fileEx.Message}");
                            return View(claim);
                        }
                    }

                    // Save claim to database - FIXED potential null reference
                    if (_context.Claims != null)
                    {
                        _context.Claims.Add(claim);
                        await _context.SaveChangesAsync();

                        // Add initial status tracking - FIXED null reference for StatusUpdates
                        if (_context.StatusUpdates != null)
                        {
                            var statusUpdate = new StatusUpdate
                            {
                                ClaimId = claim.Id,
                                Status = "Submitted",
                                UpdatedBy = claim.LecturerName ?? "Unknown",
                                Notes = "Claim submitted for approval"
                            };
                            _context.StatusUpdates.Add(statusUpdate);
                            await _context.SaveChangesAsync();
                        }

                        TempData["SuccessMessage"] = "Claim submitted successfully!";
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError("", "Database context is not properly initialized.");
                        return View(claim);
                    }
                }
                catch (DbUpdateException dbEx)
                {
                    ModelState.AddModelError("", $"Database error: {dbEx.Message}");
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Unexpected error: {ex.Message}");
                }
            }

            return View(claim);
        }

        // GET: Show all claims for lecturer
        public async Task<IActionResult> MyClaims()
        {
            try
            {
                var claims = await _context.Claims
                    .OrderByDescending(c => c.SubmittedDate)
                    .ToListAsync();
                return View(claims);
            }
            catch (Exception ex)
            {
                // Log error and return empty list
                Console.WriteLine($"Error retrieving claims: {ex.Message}");
                return View(new List<Claim>());
            }
        }

        // GET: Claim details with status history
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var claim = await _context.Claims
                    .FirstOrDefaultAsync(c => c.Id == id);

                if (claim == null)
                {
                    return NotFound();
                }

                var statusHistory = await _context.StatusUpdates
                    .Where(s => s.ClaimId == id)
                    .OrderBy(s => s.UpdateDate)
                    .ToListAsync();

                ViewBag.StatusHistory = statusHistory ?? new List<StatusUpdate>();
                return View(claim);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error retrieving claim details: {ex.Message}");
                return NotFound();
            }
        }
    }
}