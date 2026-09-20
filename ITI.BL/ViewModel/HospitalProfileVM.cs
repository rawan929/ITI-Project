using System;
using System.ComponentModel.DataAnnotations;

namespace ITI.BLL.ViewModel
{
    public class HospitalProfileVM
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "Hospital name is required")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Address is required")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        public string Phone { get; set; } = string.Empty;

        public bool IsApproved { get; set; }
    }
}