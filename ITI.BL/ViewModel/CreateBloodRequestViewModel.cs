using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class CreateBloodRequestViewModel
    {

        [Required]
        public int BloodTypeId { get; set; }

        [Required]
        [Range(1, 100)]
        public int UnitsRequired { get; set; }

        [Required]
        public string Urgency { get; set; } = string.Empty;

    }
}
