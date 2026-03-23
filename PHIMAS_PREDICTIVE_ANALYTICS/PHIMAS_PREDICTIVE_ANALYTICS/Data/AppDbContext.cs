using Microsoft.EntityFrameworkCore;
using PHIMAS_PREDICTIVE_ANALYTICS.Models;

namespace PHIMAS_PREDICTIVE_ANALYTICS.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users => Set<User>();
    public DbSet<Household> Households => Set<Household>();
    public DbSet<HouseholdMember> HouseholdMembers => Set<HouseholdMember>();
    public DbSet<HealthRecord> HealthRecords => Set<HealthRecord>();
    public DbSet<Inventory> Inventory => Set<Inventory>();
    public DbSet<TaskAssignment> TaskAssignments => Set<TaskAssignment>();
    public DbSet<PredictiveAnalysis> PredictiveAnalysis => Set<PredictiveAnalysis>();
    public DbSet<Report> Reports => Set<Report>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>().ToTable("users").HasIndex(user => user.Email).IsUnique();
        modelBuilder.Entity<Household>().ToTable("households").HasIndex(household => household.Address);
        modelBuilder.Entity<HouseholdMember>().ToTable("household_members");
        modelBuilder.Entity<HealthRecord>().ToTable("health_records");
        modelBuilder.Entity<Inventory>().ToTable("inventory");
        modelBuilder.Entity<TaskAssignment>().ToTable("task_assignments");
        modelBuilder.Entity<PredictiveAnalysis>().ToTable("predictive_analysis");
        modelBuilder.Entity<Report>().ToTable("reports");

        modelBuilder.Entity<HouseholdMember>()
            .HasIndex(member => new { member.HouseholdID, member.FullName, member.ContactNumber })
            .IsUnique();

        modelBuilder.Entity<HouseholdMember>()
            .HasOne(member => member.Household)
            .WithMany(household => household.Members)
            .HasForeignKey(member => member.HouseholdID)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<HealthRecord>()
            .Property(record => record.BHWID)
            .IsRequired();

        modelBuilder.Entity<HealthRecord>()
            .Property(record => record.PatientID)
            .IsRequired();

        modelBuilder.Entity<HealthRecord>()
            .HasOne(record => record.BHW)
            .WithMany()
            .HasForeignKey(record => record.BHWID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<HealthRecord>()
            .HasOne(record => record.Patient)
            .WithMany()
            .HasForeignKey(record => record.PatientID)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(task => task.BHW)
            .WithMany()
            .HasForeignKey(task => task.BHWID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<TaskAssignment>()
            .HasOne(task => task.Household)
            .WithMany()
            .HasForeignKey(task => task.HouseholdID)
            .OnDelete(DeleteBehavior.SetNull);

        modelBuilder.Entity<Report>()
            .Property(report => report.GeneratedBy)
            .IsRequired();

        modelBuilder.Entity<Report>()
            .Property(report => report.PatientID)
            .IsRequired();

        modelBuilder.Entity<Report>()
            .HasOne(report => report.GeneratedByUser)
            .WithMany()
            .HasForeignKey(report => report.GeneratedBy)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<Report>()
            .HasOne(report => report.Patient)
            .WithMany()
            .HasForeignKey(report => report.PatientID)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
