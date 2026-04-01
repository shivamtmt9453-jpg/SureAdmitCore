using Microsoft.AspNetCore.Mvc.Rendering;
using SureAdmitCore.Areas.Admin.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace SureAdmitCore.Models
{
    public class CourseCheckoutModel
    {
        // Personal Details
        [Required(ErrorMessage = "Name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        [RegularExpression(
@"^[a-zA-Z0-9._%+-]+@((gmail\.com)|(googlemail\.com)|(outlook\.com)|([a-zA-Z0-9.-]+\.[a-zA-Z]{2,}))$",
ErrorMessage = "Enter a valid email address. Popular domains like gmail.com, outlook.com must be spelled correctly.")]

        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Contact number is required.")]
        [RegularExpression(@"^\d{7,15}$", ErrorMessage = "Phone number must be between 7 and 15 digits.")]
        public string Phone { get; set; } = string.Empty;

        public string? Country { get; set; }

        // Education Details
        [Display(Name = "Bachelor's CGPA/%")]
        public string? BachelorCGPA { get; set; }

        [Display(Name = "Master's CGPA/%")]
        public string? MasterCGPA { get; set; }

        public string? GREVerbal { get; set; }

        public string? GREQuant { get; set; }

        // Additional Message
        public string? Message { get; set; }

        // Cart & Payment Details
        public List<int> CartCourseIds { get; set; } = new List<int>();

        [DataType(DataType.Currency)]
        public decimal BaseAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal GSTAmount { get; set; }

        [DataType(DataType.Currency)]
        public decimal TotalAmount { get; set; }

        // Payment & Booking (filled after payment)
        public string? PaymentStatus { get; set; }

        public string? PaymentRefNo { get; set; }

        public string? BookingId { get; set; }
        public decimal Subtotal { get; set; }
        public decimal GST { get; set; }
        public decimal Total { get; set; }
        // Cart Courses
        public List<Coursecheckout> CartCourses { get; set; } = new List<Coursecheckout>();

        // Dropdown for countries
        public IEnumerable<SelectListItem> CountryList { get; set; } = new List<SelectListItem>();

    }
    // Example course class
    public class Coursecheckout
    {
        public int CourseId { get; set; }
        public string? CourseName { get; set; }
        public string? CourseImgPath { get; set; }

        // Decimal value for calculations
        public decimal CoursePrice { get; set; }

        // Original string for display (with $ or ₹)
        public string CoursePriceDisplay { get; set; }
    }
}