using ITI.DAL.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace ITI.DAL.Context
{
    public class AppDbcontext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
    {
        public AppDbcontext(DbContextOptions<AppDbcontext> options) : base(options)
        {
        }

      
     
        public DbSet<Donor> Donors { get; set; }
        public DbSet<Hospital> Hospitals { get; set; }
        public DbSet<BloodBank> BloodBanks { get; set; }
        public DbSet<BloodType> BloodTypes { get; set; }
        public DbSet<BloodRequest> BloodRequests { get; set; }
        public DbSet<DonationRequest> DonationRequests { get; set; }
        public DbSet<Donation> Donations { get; set; }
        public DbSet<BloodInventory> BloodInventories { get; set; }
        public DbSet<Appointment> Appointments { get; set; }


        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<ApplicationUser>().ToTable("ApplicationUsers");
            modelBuilder.Entity<IdentityRole<Guid>>().ToTable("AspNetRoles");

            // --- 1. One-to-One Relationships ---

            // ApplicationUser <-> Donor (1 to 0..1)
            modelBuilder.Entity<Donor>()
                .HasOne(d => d.User)
                .WithOne(u => u.Donor)
                .HasForeignKey<Donor>(d => d.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // ApplicationUser <-> Hospital (1 to 0..1)
            modelBuilder.Entity<Hospital>()
                .HasOne(h => h.User)
                .WithOne(u => u.Hospital)
                .HasForeignKey<Hospital>(h => h.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // --- 2. One-to-Many Relationships ---

            // Donor <-> BloodType
            modelBuilder.Entity<Donor>()
                .HasOne(d => d.BloodType)
                .WithMany(bt => bt.Donors)
                .HasForeignKey(d => d.BloodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Hospital <-> BloodRequest
            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.Hospital)
                .WithMany(h => h.BloodRequests)
                .HasForeignKey(br => br.HospitalId)
                .OnDelete(DeleteBehavior.Cascade);

            // BloodType <-> BloodRequest
            modelBuilder.Entity<BloodRequest>()
                .HasOne(br => br.BloodType)
                .WithMany(bt => bt.BloodRequests)
                .HasForeignKey(br => br.BloodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // BloodRequest <-> DonationRequest

            modelBuilder.Entity<DonationRequest>()
                 .HasOne(dr => dr.BloodRequest)
                 .WithMany(br => br.DonationRequests)
                .HasForeignKey(dr => dr.BloodRequestId)
                .OnDelete(DeleteBehavior.Cascade); 

            // Donor <-> DonationRequest 
            modelBuilder.Entity<DonationRequest>()
                .HasOne(dr => dr.Donor)
                .WithMany(d => d.DonationRequests)
                .HasForeignKey(dr => dr.DonorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Donor <-> Donation
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.Donor)
                .WithMany(dn => dn.Donations)
                .HasForeignKey(d => d.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            // BloodBank <-> Donation
            modelBuilder.Entity<Donation>()
                .HasOne(d => d.BloodBank)
                .WithMany(bb => bb.Donations)
                .HasForeignKey(d => d.BloodBankId)
                .OnDelete(DeleteBehavior.Restrict);

            // BloodBank <-> BloodInventory
            modelBuilder.Entity<BloodInventory>()
                .HasOne(bi => bi.BloodBank)
                .WithMany(bb => bb.Inventories)
                .HasForeignKey(bi => bi.BloodBankId)
                .OnDelete(DeleteBehavior.Cascade);

            // BloodType <-> BloodInventory
            modelBuilder.Entity<BloodInventory>()
                .HasOne(bi => bi.BloodType)
                .WithMany(bt => bt.Inventories)
                .HasForeignKey(bi => bi.BloodTypeId)
                .OnDelete(DeleteBehavior.Restrict);

            // Donor <-> Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Donor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DonorId)
                .OnDelete(DeleteBehavior.Cascade);

            // BloodBank <-> Appointment
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.BloodBank)
                .WithMany(bb => bb.Appointments)
                .HasForeignKey(a => a.BloodBankId)
                .OnDelete(DeleteBehavior.Restrict);

            // Seed initial Data for BloodType
            modelBuilder.Entity<BloodType>().HasData(
                new BloodType { Id = 1, Name = "A+" },
                new BloodType { Id = 2, Name = "A-" },
                new BloodType { Id = 3, Name = "B+" },
                new BloodType { Id = 4, Name = "B-" },
                new BloodType { Id = 5, Name = "AB+" },
                new BloodType { Id = 6, Name = "AB-" },
                new BloodType { Id = 7, Name = "O+" },
                new BloodType { Id = 8, Name = "O-" }
            );
        }


    }
}
