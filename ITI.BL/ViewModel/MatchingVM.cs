using System;
using System.Collections.Generic;

namespace ITI.BLL.ViewModel
{
    /// <summary>
    /// The status values a DonationRequest row can hold. Kept in one place so the
    /// matching, notification and response code can't drift apart on spelling.
    /// </summary>
    public static class DonationRequestStatus
    {
        /// <summary>The system matched this donor and invited them. Waiting on the donor.</summary>
        public const string Invited = "Invited";

        /// <summary>The donor agreed to donate for this request.</summary>
        public const string Accepted = "Accepted";

        /// <summary>The donor turned this request down.</summary>
        public const string Declined = "Declined";
    }

    public class MatchedDonorVM
    {
        public Guid DonorId { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public int TotalDonations { get; set; }
        public DateTime? LastDonationDate { get; set; }

        /// <summary>True when the donor is in the same city as the requesting hospital.</summary>
        public bool IsSameCity { get; set; }

        /// <summary>Already invited or already responded to this request.</summary>
        public bool AlreadyContacted { get; set; }
    }

    public class MatchingResultVM
    {
        public Guid BloodRequestId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalCity { get; set; } = string.Empty;
        public string RequestedBloodType { get; set; } = string.Empty;
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;

        /// <summary>Donor blood types that can supply the requested type.</summary>
        public List<string> CompatibleDonorTypes { get; set; } = new();

        public List<MatchedDonorVM> Donors { get; set; } = new();
    }

    /// <summary>What happened when a request was created and matched.</summary>
    public class CreateRequestOutcome
    {
        public CreateRequestResult Result { get; set; }
        public Guid? BloodRequestId { get; set; }
        public int MatchedDonors { get; set; }
        public int NotifiedDonors { get; set; }
    }

    // -------------------------------------------------------------------
    // Donor inbox
    // -------------------------------------------------------------------

    public class DonorInboxItemVM
    {
        public Guid DonationRequestId { get; set; }
        public Guid BloodRequestId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string HospitalCity { get; set; } = string.Empty;
        public string HospitalPhone { get; set; } = string.Empty;
        public string BloodTypeName { get; set; } = string.Empty;
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string ResponseStatus { get; set; } = string.Empty;

        public bool IsAwaitingReply =>
            ResponseStatus == DonationRequestStatus.Invited && RequestStatus == "Pending";
    }

    public enum RespondToInvitationResult
    {
        Success,
        NotFound,
        NotYours,
        AlreadyAnswered,
        RequestClosed
    }
}
