using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;
using System;
using System.Collections.Generic;
using System.Text;
using ITI.DAL.Repo;

namespace ITI.BLL.Services.Implementation
{
    public class DonationService : IDonationService
    {
        private readonly IDonationRepo _donationRepo;
        private readonly IBloodInventoryRepo _inventoryRepo;

        public DonationService(IDonationRepo donationRepo, IBloodInventoryRepo inventoryRepo)
        {
            _donationRepo = donationRepo;
            _inventoryRepo = inventoryRepo;
        }

        public async Task<bool> RecordDonationAsync(CreateDonationVM model)
        {
            Guid.TryParse(model.DonorId, out Guid DonorGuid);
            Guid.TryParse(model.BloodBankId, out Guid BankGuid);
            var donation = new Donation
            {
                Id = Guid.NewGuid(),
                DonorId = DonorGuid != Guid.Empty ? DonorGuid : Guid.NewGuid(),
                BloodBankId = BankGuid != Guid.Empty ? BankGuid: Guid.NewGuid(),
                Units = model.Units,
                DonationDate = DateTime.UtcNow,
                Status = "Completed"
            };

            await _donationRepo.AddAsync(donation);
            return await _donationRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<DonationVM>> GetDonationsByBankAsync(Guid bloodBankId)
        {
            var donations = await _donationRepo.GetByBankIdAsync(bloodBankId);
            return donations.Select(d => new DonationVM
            {
                Id = d.Id,
                DonorId = d.DonorId,
                DonorName = d.Donor?.User?.UserName ?? string.Empty,
                BloodBankId = d.BloodBankId,
                Units = d.Units,
                DonationDate = d.DonationDate,
                Status = d.Status
            });
        }

        public async Task<IEnumerable<DonationVM>> GetDonationsByDonorAsync(Guid donorId)
        {
            var donations = await _donationRepo.GetByDonorIdAsync(donorId);
            return donations.Select(d => new DonationVM
            {
                Id = d.Id,
                DonorId = d.DonorId,
                BloodBankId = d.BloodBankId,
                Units = d.Units,
                DonationDate = d.DonationDate,
                Status = d.Status
            });
        }
    }
}
