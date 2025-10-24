using System.ComponentModel.DataAnnotations;

namespace ContractMonthlyClaimSystem.Models
{
    public class Claim
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Lecturer name is required")]
        public string LecturerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string LecturerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Hours worked is required")]
        [Range(1, 200, ErrorMessage = "Hours must be between 1 and 200")]
        public int HoursWorked { get; set; }

        [Required(ErrorMessage = "Hourly rate is required")]
        [Range(10, 500, ErrorMessage = "Hourly rate must be between 10 and 500")]
        public decimal HourlyRate { get; set; }

        public decimal TotalAmount => HoursWorked * HourlyRate;

        [StringLength(500)]
        public string? Notes { get; set; }

        public string? DocumentFileName { get; set; }

        public string Status { get; set; } = "Pending";

        public DateTime SubmittedDate { get; set; } = DateTime.Now;

        public DateTime? ApprovedDate { get; set; }

        public string? ApprovedBy { get; set; }

        public string? RejectionReason { get; set; }
    }

    public class StatusUpdate
    {
        public int Id { get; set; }
        public int ClaimId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string UpdatedBy { get; set; } = string.Empty;
        public DateTime UpdateDate { get; set; } = DateTime.Now;
        public string? Notes { get; set; }
    }
}