using LoanManagementSystem.Api.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace LoanManagementSystem.Api.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        // DbSets
        public DbSet<User> Users { get; set; }
        public DbSet<LoanType> LoanTypes { get; set; }
        public DbSet<LoanApplication> LoanApplications { get; set; }
        public DbSet<EMI> EMIs { get; set; }
        public DbSet<LoanDocument> LoanDocuments { get; set; } 
        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var dateOnlyConverter = new ValueConverter<DateOnly, DateTime>(
                d => d.ToDateTime(TimeOnly.MinValue),
                d => DateOnly.FromDateTime(d)
            );

            var nullableDateOnlyConverter = new ValueConverter<DateOnly?, DateTime?>(
                d => d.HasValue ? d.Value.ToDateTime(TimeOnly.MinValue) : null,
                d => d.HasValue ? DateOnly.FromDateTime(d.Value) : null
            );


            // USER CONFIGURATION

            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            modelBuilder.Entity<User>()
                .HasMany(u => u.LoanApplications)
                .WithOne(l => l.Customer)
                .HasForeignKey(l => l.CustomerId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<User>()
                .HasMany(u => u.ApprovedLoans)
                .WithOne(l => l.ApprovedByUser)
                .HasForeignKey(l => l.ApprovedBy)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<LoanApplication>()
                .HasOne(l => l.VerifiedByUser)
                .WithMany()
                .HasForeignKey(l => l.VerifiedBy)
                .OnDelete(DeleteBehavior.Restrict);


            // LOAN TYPE CONFIGURATION
            
            modelBuilder.Entity<LoanType>()
                .HasMany(l => l.LoanApplications)
                .WithOne(a => a.LoanType)
                .HasForeignKey(a => a.LoanTypeId)
                .OnDelete(DeleteBehavior.Restrict);


            // LOAN APPLICATION CONFIGURATION
            
            modelBuilder.Entity<LoanApplication>()
                .Property(l => l.Status)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<LoanApplication>()
                .HasMany(l => l.EMIs)
                .WithOne(e => e.LoanApplication)
                .HasForeignKey(e => e.LoanId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<LoanApplication>()
                .Property(e => e.AppliedDate)
                .HasConversion(dateOnlyConverter);

            modelBuilder.Entity<LoanApplication>()
                .Property(e => e.ApprovedDate)
                .HasConversion(nullableDateOnlyConverter);

            modelBuilder.Entity<LoanApplication>()
                .Property(e => e.EmiStartDate)
                .HasConversion(dateOnlyConverter);


            // EMI CONFIGURATION

            modelBuilder.Entity<EMI>()
                .Property(e => e.EMIAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<EMI>()
                .HasOne(e => e.Payment)
                .WithOne(p => p.EMI)
                .HasForeignKey<Payment>(p => p.EMIId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<EMI>()
                .Property(e => e.DueDate)
                .HasConversion(dateOnlyConverter);

            modelBuilder.Entity<EMI>()
                .Property(e => e.PaidDate)
                .HasConversion(nullableDateOnlyConverter);


            // PAYMENT CONFIGURATION

            modelBuilder.Entity<Payment>()
                .Property(p => p.PaidAmount)
                .HasPrecision(18, 2);

            
            // LOAN TYPE AMOUNT PRECISION
           
            modelBuilder.Entity<LoanType>()
                .Property(l => l.MinAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<LoanType>()
                .Property(l => l.MaxAmount)
                .HasPrecision(18, 2);
        }
    }
}
