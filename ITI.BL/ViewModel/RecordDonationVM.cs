using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class RecordDonationVM
    {
        [Required(ErrorMessage ="Please choose the Donor")]
        public string? DonorId { get; set; }
        [Required(ErrorMessage ="Please choose the Blood Bank")]
        public string? BloodBankId { get; set; }
        [Required(ErrorMessage ="Please choose the Blood Type")]
        public string BloodTypeId { get; set; }= string.Empty;
        [Required(ErrorMessage ="Please specify the Number of Units")]
        [Range(1,10,ErrorMessage ="the number should be between 1 and 10")]
        public int Units { get; set; }
    }
}
