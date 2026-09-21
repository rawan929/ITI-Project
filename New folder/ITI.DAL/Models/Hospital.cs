using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class Hospital
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;

        // Navigation Properties
        public ApplicationUser User { get; set; } = null!;
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
    }
}
