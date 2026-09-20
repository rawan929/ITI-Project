using ITI.BLL.Constants;
using ITI.BLL.Services.Interface;
using ITI.BLL.ViewModel;
using ITI.DAL.Models;
using ITI.DAL.Repo.Interface;

namespace ITI.BLL.Services.Implementation
{
    public class DonationService : IDonationService
    {
        private const int MaxUnitsPerDonation = 10;

        private readonly IDonationRepo _donationRepo;
        private readonly IBloodBankRepo _bloodBankRepo;

        public DonationService(IDonationRepo donationRepo, IBloodBankRepo bloodBankRepo)
        {
            _donationRepo = donationRepo;
            _bloodBankRepo = bloodBankRepo;
        }
        public async Task<bool> RecordDonationAsync(CreateDonationVM model)
        {
            if (!Guid.TryParse(model.DonorId, out var donorId) ||
                !Guid.TryParse(model.BloodBankId, out var bankId))
                return false;

            if (model.Units < 1 || model.Units > MaxUnitsPerDonation) return false;
            if (!await _donationRepo.DonorExistsAsync(donorId)) return false;

            var bank = await _bloodBankRepo.GetByIdAsync(bankId);
            if (bank == null || !bank.IsApproved) return false;

            await _donationRepo.AddAsync(new Donation
            {
                Id = Guid.NewGuid(),
                DonorId = donorId,
                BloodBankId = bankId,
                Units = model.Units,
                DonationDate = DateTime.UtcNow,
                Status = DonationStatuses.InTesting
            });
            return await _donationRepo.SaveChangesAsync();
        }

        public async Task<IEnumerable<DonationVM>> GetDonationsByBankAsync(Guid bloodBankId)
        {
            var donations = await _donationRepo.GetByBankIdAsync(bloodBankId);
            return donations.Select(d => new DonationVM
            {
                Id = d.Id,
                DonorId = d.DonorId,
                DonorName = d.Donor?.User?.FullName ?? string.Empty,
                BloodType = d.Donor?.BloodType?.Name,
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
        public async Task<List<DonationHistoryItem>> GetDonorDonationHistory(Guid donorId)
        {
            var donations = await _donationRepo.GetByDonorIdAsync(donorId);
            return donations
                .OrderByDescending(d => d.DonationDate)
                .Select(d => new DonationHistoryItem
                {
                    DonationDate = d.DonationDate,
                    BloodBankName = d.BloodBank?.Name ?? string.Empty,
                    Units = d.Units,
                    Status = d.Status
                })
                .ToList();
        }

        public async Task<(bool Success, string? Error)> MarkProcessedAsync(Guid donationId, Guid bloodBankId)
        {
            var ok = await _donationRepo.ProcessAsync(
                donationId, bloodBankId, DonationStatuses.InTesting, DonationStatuses.Completed);

            return ok
                ? (true, null)
                : (false, "This donation was not found or has already been processed.");
        }
    }
}