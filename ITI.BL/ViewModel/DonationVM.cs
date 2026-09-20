using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class DonationVM
    {
        public Guid Id { get; set; }
        public Guid DonorId { get; set; }
        public string? DonorName { get; set; }
        public Guid BloodBankId { get; set; }
        public int Units { get; set; }
        public string? Status { get; set; }
        public DateTime DonationDate { get; set; }
        public string? BloodType { get; set; }
        public string Code => $"DON-{DonationDate:yyyy}-{Id.ToString("N")[..6].ToUpper()}";
    }

    public class CreateDonationVM
    {
        public string? DonorId { get; set; }
        public string? BloodBankId { get; set; }
        public int Units { get; set; } = 1;
    }
}
