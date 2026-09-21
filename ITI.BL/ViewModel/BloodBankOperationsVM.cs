using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ITI.BLL.ViewModel
{
    // ---------------------------------------------------------------
    // Inventory
    // ---------------------------------------------------------------

    public class BloodBankInventoryPageVM
    {
        public string BloodBankName { get; set; } = string.Empty;
        public List<BloodBankInventoryItemVM> Items { get; set; } = new();
        public BloodBankAdjustInventoryVM Adjust { get; set; } = new();
    }

    public class BloodBankAdjustInventoryVM
    {
        [Required(ErrorMessage = "Please choose a blood type")]
        [Display(Name = "Blood Type")]
        public int BloodTypeId { get; set; }

        [Required(ErrorMessage = "Please enter the number of units")]
        [Range(1, 500, ErrorMessage = "Units must be between 1 and 500")]
        public int Units { get; set; }

        /// <summary>"Add" to receive stock, "Remove" to discard or issue stock manually.</summary>
        [Required]
        public string Operation { get; set; } = "Add";

        public List<BloodTypeOptionVM> AvailableBloodTypes { get; set; } = new();
    }

    public class BloodTypeOptionVM
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    public enum AdjustInventoryResult
    {
        Success,
        BloodBankNotFound,
        BloodBankNotApproved,
        InvalidBloodType,
        InsufficientStock
    }

    // ---------------------------------------------------------------
    // Recording a donation
    // ---------------------------------------------------------------

    public class BloodBankRecordDonationVM
    {
        [Required(ErrorMessage = "Please choose a donor")]
        [Display(Name = "Donor")]
        public Guid DonorId { get; set; }

        [Required(ErrorMessage = "Please enter the number of units")]
        [Range(1, 5, ErrorMessage = "A single donation is normally 1 unit")]
        public int Units { get; set; } = 1;

        [Display(Name = "Donation Date")]
        [DataType(DataType.Date)]
        public DateTime DonationDate { get; set; } = DateTime.UtcNow.Date;

        /// <summary>
        /// Lets staff record a donation for a donor the 90-day rule would normally block,
        /// e.g. when the donor's recorded history is wrong. Always an explicit choice.
        /// </summary>
        [Display(Name = "Override the 90-day eligibility check")]
        public bool OverrideEligibility { get; set; }

        public List<BloodBankDonorOptionVM> AvailableDonors { get; set; } = new();
    }

    public class BloodBankDonorOptionVM
    {
        public Guid DonorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public DateTime? LastDonationDate { get; set; }
        public DateTime? NextEligibleDate { get; set; }
        public bool IsEligible { get; set; }
    }

    public enum RecordDonationResult
    {
        Success,
        BloodBankNotFound,
        BloodBankNotApproved,
        DonorNotFound,
        DonorBloodTypeMissing,
        DonorNotEligible,
        InvalidDate
    }

    // ---------------------------------------------------------------
    // Appointments
    // ---------------------------------------------------------------

    public class BloodBankAppointmentVM
    {
        public Guid AppointmentId { get; set; }
        public Guid DonorId { get; set; }
        public string DonorName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public DateTime AppointmentDate { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsUpcoming { get; set; }
    }

    // ---------------------------------------------------------------
    // Incoming hospital requests
    // ---------------------------------------------------------------

    public class BloodBankIncomingRequestVM
    {
        public Guid RequestId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalCity { get; set; } = string.Empty;
        public string HospitalPhone { get; set; } = string.Empty;
        public int BloodTypeId { get; set; }
        public string BloodTypeName { get; set; } = string.Empty;
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;

        /// <summary>How many units of that exact type this bank currently holds.</summary>
        public int UnitsInStock { get; set; }
        public bool CanFulfill => UnitsInStock >= UnitsRequired;
    }

    public enum FulfillRequestResult
    {
        Success,
        BloodBankNotFound,
        BloodBankNotApproved,
        RequestNotFound,
        RequestAlreadyClosed,
        InsufficientStock
    }

    // ---------------------------------------------------------------
    // Profile
    // ---------------------------------------------------------------

    public class BloodBankProfileVM
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Blood Bank Name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone is required")]
        [Phone]
        public string Phone { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;
        public bool IsApproved { get; set; }
    }
}
