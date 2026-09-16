using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace ITI.BLL.ViewModel
{
    public class RecordDonationVM
    {
        [Required(ErrorMessage ="Please choose the Donor")]
        public Guid DonorId { get; set; }
        [Required(ErrorMessage ="Please choose the Blood Bank")]
        public Guid BloodBankId { get; set; }
        [Required(ErrorMessage ="Please choose the Blood Type")]
        public int BloodTypeId { get; set; }
        [Required(ErrorMessage ="Please specify the Number of Units")]
        [Range(1,10,ErrorMessage ="the number should be between 1 and 10")]
        public int Units { get; set; }
    }
}
