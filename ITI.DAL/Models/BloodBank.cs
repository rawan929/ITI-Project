using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class BloodBank
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public bool IsApproved { get; set; } = false;

        
        public ICollection<Donation> Donations { get; set; } = new List<Donation>();
        public ICollection<BloodInventory> Inventories { get; set; } = new List<BloodInventory>();
        public ICollection<Appointment> Appointments { get; set; } = new List<Appointment>();
    }
}

