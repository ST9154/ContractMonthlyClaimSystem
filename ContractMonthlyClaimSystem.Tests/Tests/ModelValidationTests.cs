using System.ComponentModel.DataAnnotations;
using ContractMonthlyClaimSystem.Models;

namespace ContractMonthlyClaimSystem.Tests
{
    public class ModelValidationTests
    {
        [Xunit.Fact]
        public void Claim_ValidData_PassesValidation()
        {
            var claim = new Claim
            {
                LecturerName = "John Doe",
                LecturerEmail = "john@university.edu",
                HoursWorked = 40,
                HourlyRate = 50
            };

            var context = new ValidationContext(claim);
            var results = new List<ValidationResult>();
            var isValid = Validator.TryValidateObject(claim, context, results, true);

            Xunit.Assert.True(isValid);
            Xunit.Assert.Empty(results);
        }

        [Xunit.Fact]
        public void Claim_TotalAmount_CalculatedCorrectly()
        {
            var claim = new Claim
            {
                HoursWorked = 40,
                HourlyRate = 50
            };

            Xunit.Assert.Equal(2000, claim.TotalAmount);
        }
    }
}