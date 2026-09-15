using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Models
{
    public class ApplicationUser : IdentityUser<Guid>
    {
        
        public string FullName { get; set; } = string.Empty;
        
        
        
        public string City { get; set; } = string.Empty;
        public string UserType { get; set; } = string.Empty; 
        public bool IsActive { get; set; } = true;

        // Navigation Properties
        public Donor? Donor { get; set; }
        public Hospital? Hospital { get; set; }
    }
}
