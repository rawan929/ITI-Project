using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class DonorResponseViewModel
    {
        public Guid DonorId { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string BloodType { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public int TotalDonations { get; set; }

        public bool IsActive { get; set; }

        /// <summary>Invited / Accepted / Declined.</summary>
        public string ResponseStatus { get; set; } = string.Empty;

        public string Initials
        {
            get
            {
                if (string.IsNullOrWhiteSpace(FullName))
                    return "??";

                var parts = FullName
                    .Trim()
                    .Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (parts.Length == 1)
                    return parts[0]
                        .Substring(0, Math.Min(2, parts[0].Length))
                        .ToUpper();

                return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
            }
        }
    }
}

