using System;

namespace SureAdmitCore.Areas.Admin.Models
{
    public class AppliedCourse
    {
        public int ApplicationId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string PinCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public bool IsAgreementAccepted { get; set; }
        public string Country { get; set; } = string.Empty;
        public string CountryName { get; set; } = string.Empty;
        public string BachelorCGPA { get; set; } = string.Empty;
        public string MasterCGPA { get; set; } = string.Empty;
        public string GREVerbal { get; set; } = string.Empty;
        public string GREQuant { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public string CourseNames { get; set; } = string.Empty;
        public decimal BaseAmount { get; set; }
        public decimal GSTAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public string PaymentStatus { get; set; } = "Pending";
        public string PaymentRefNo { get; set; } = string.Empty;
        public string BookingId { get; set; } = string.Empty;
        public string PaymentGateway { get; set; } = string.Empty;
        public decimal PaidAmount { get; set; }
        public string Currency { get; set; } = "INR";
        public string PaymentLogStatus { get; set; } = string.Empty;
        public DateTime? PaymentDate { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedOn { get; set; }
    }
}