using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class BloodType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; 

        
        public ICollection<Donor> Donors { get; set; } = new List<Donor>();
        public ICollection<BloodRequest> BloodRequests { get; set; } = new List<BloodRequest>();
        public ICollection<BloodInventory> Inventories { get; set; } = new List<BloodInventory>();
    }
}
