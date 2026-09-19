using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class BookAppointmentVM
    {
        [Required]
        public string? DonorId { get; set; }
        [Required (ErrorMessage = "Please choose a Blood Bank")]
        public string? BloodBankId { get; set; }
        [Required (ErrorMessage ="Please choose the Date and Time")]
        [DataType(DataType.DateTime)]
        public DateTime AppointmentDate { get; set; }
        public string? BloodBankName { get; set; }
    }
}
