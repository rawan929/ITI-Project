using System;

namespace ITI.BLL.ViewModel
{
    public class DonorResponseHistoryVM
    {
        public Guid RequestId { get; set; }
        public string HospitalName { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string BloodType { get; set; } = string.Empty;
        public int UnitsRequired { get; set; }
        public string Urgency { get; set; } = string.Empty;
        public string RequestStatus { get; set; } = string.Empty;
        public string ResponseStatus { get; set; } = string.Empty;
    }
}
