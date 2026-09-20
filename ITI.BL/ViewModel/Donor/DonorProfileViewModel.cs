using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITI.BLL.ViewModel.Donor
{
    public class DonorProfileViewModel
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 3)]
        [Display(Name = "Full Name")]
        public string FullName { get; set; } = string.Empty;

        [Display(Name = "Email Address")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Please select your blood type")]
        [Display(Name = "Blood Type")]
        public int? BloodTypeId { get; set; }

        [Required(ErrorMessage = "Date of birth is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Date of Birth")]
        public DateTime? DateOfBirth { get; set; }

        [Required(ErrorMessage = "Please select your gender")]
        public string Gender { get; set; } = string.Empty;

        [Range(1, 400, ErrorMessage = "Please enter a valid weight")]
        [Display(Name = "Weight (kg)")]
        public double? Weight { get; set; }

        [Range(1, 300, ErrorMessage = "Please enter a valid height")]
        [Display(Name = "Height (cm)")]
        public double? Height { get; set; }

        [Display(Name = "Known Allergies")]
        public string? KnownAllergies { get; set; }

        [Display(Name = "Chronic Conditions")]
        public string? ChronicConditions { get; set; }

        public List<BloodTypeOption> AvailableBloodTypes { get; set; } = new();
    }

    public class BloodTypeOption
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}