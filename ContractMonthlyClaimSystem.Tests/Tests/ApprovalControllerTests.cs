using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ContractMonthlyClaimSystem.Controllers;
using ContractMonthlyClaimSystem.Data;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ApprovalControllerTests : IDisposable
    {
        private readonly ApplicationDbContext _context;

        public ApprovalControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            _context = new ApplicationDbContext(options);
            _context.Database.EnsureCreated();
        }

        public void Dispose()
        {
            _context.Database.EnsureDeleted();
            _context.Dispose();
        }

        [Xunit.Fact]
        public async Task Pending_ReturnsOnlyPendingClaims()
        {
            // Arrange
            var pendingClaim = new Claim
            {
                LecturerName = "Test",
                Status = "Pending",
                SubmittedDate = DateTime.Now,
                HoursWorked = 10,
                HourlyRate = 50,
                LecturerEmail = "test@test.com"
            };
            var approvedClaim = new Claim
            {
                LecturerName = "Test2",
                Status = "Approved",
                SubmittedDate = DateTime.Now,
                HoursWorked = 20,
                HourlyRate = 60,
                LecturerEmail = "test2@test.com"
            };

            await _context.Claims.AddRangeAsync(pendingClaim, approvedClaim);
            await _context.SaveChangesAsync();

            var controller = new ApprovalController(_context);

            // Act
            var result = await controller.Pending();

            // Assert
            var viewResult = Xunit.Assert.IsType<ViewResult>(result);
            var model = Xunit.Assert.IsAssignableFrom<IEnumerable<Claim>>(viewResult.Model);
            Xunit.Assert.Single(model);
            Xunit.Assert.All(model, claim => Xunit.Assert.Equal("Pending", claim.Status));
        }

        [Xunit.Fact]
        public async Task Approve_ValidId_ApprovesClaim()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test",
                Status = "Pending",
                SubmittedDate = DateTime.Now,
                HoursWorked = 10,
                HourlyRate = 50,
                LecturerEmail = "test@test.com"
            };
            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();

            var controller = new ApprovalController(_context);

            // Act
            var result = await controller.Approve(claim.Id);

            // Assert
            var redirectResult = Xunit.Assert.IsType<RedirectToActionResult>(result);
            Xunit.Assert.Equal("Pending", redirectResult.ActionName);

            // Verify database update
            var updatedClaim = await _context.Claims.FindAsync(claim.Id);
            Xunit.Assert.NotNull(updatedClaim);
            Xunit.Assert.Equal("Approved", updatedClaim!.Status);
            Xunit.Assert.NotNull(updatedClaim.ApprovedDate);
        }

        [Xunit.Fact]
        public async Task Approve_InvalidId_ReturnsNotFound()
        {
            // Arrange
            var controller = new ApprovalController(_context);

            // Act
            var result = await controller.Approve(999); // Non-existent ID

            // Assert
            Xunit.Assert.IsType<NotFoundResult>(result);
        }

        [Xunit.Fact]
        public async Task AllClaims_ReturnsAllClaims()
        {
            // Arrange
            var claim1 = new Claim
            {
                LecturerName = "Test1",
                Status = "Pending",
                SubmittedDate = DateTime.Now,
                HoursWorked = 10,
                HourlyRate = 50,
                LecturerEmail = "test1@test.com"
            };
            var claim2 = new Claim
            {
                LecturerName = "Test2",
                Status = "Approved",
                SubmittedDate = DateTime.Now,
                HoursWorked = 20,
                HourlyRate = 60,
                LecturerEmail = "test2@test.com"
            };

            await _context.Claims.AddRangeAsync(claim1, claim2);
            await _context.SaveChangesAsync();

            var controller = new ApprovalController(_context);

            // Act
            var result = await controller.AllClaims();

            // Assert
            var viewResult = Xunit.Assert.IsType<ViewResult>(result);
            var model = Xunit.Assert.IsAssignableFrom<IEnumerable<Claim>>(viewResult.Model);
            Xunit.Assert.Equal(2, model.Count());
        }

        [Xunit.Fact]
        public async Task Reject_ValidId_RejectsClaim()
        {
            // Arrange
            var claim = new Claim
            {
                LecturerName = "Test",
                Status = "Pending",
                SubmittedDate = DateTime.Now,
                HoursWorked = 10,
                HourlyRate = 50,
                LecturerEmail = "test@test.com"
            };
            await _context.Claims.AddAsync(claim);
            await _context.SaveChangesAsync();

            var controller = new ApprovalController(_context);
            var rejectionReason = "Insufficient documentation";

            // Act
            var result = await controller.Reject(claim.Id, rejectionReason);

            // Assert
            var redirectResult = Xunit.Assert.IsType<RedirectToActionResult>(result);
            Xunit.Assert.Equal("Pending", redirectResult.ActionName);

            var updatedClaim = await _context.Claims.FindAsync(claim.Id);
            Xunit.Assert.NotNull(updatedClaim);
            Xunit.Assert.Equal("Rejected", updatedClaim!.Status);
            Xunit.Assert.Equal(rejectionReason, updatedClaim.RejectionReason);
        }
    }
}