using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Models;

namespace VeterinerKlinigi.Data;

public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : IdentityDbContext(options)
{
    public DbSet<Clinic> Clinics => Set<Clinic>();
    public DbSet<ClinicWorkingHour> ClinicWorkingHours => Set<ClinicWorkingHour>();
    public DbSet<ClinicService> ClinicServices => Set<ClinicService>();
    public DbSet<Veterinarian> Veterinarians => Set<Veterinarian>();
    public DbSet<Specialty> Specialties => Set<Specialty>();
    public DbSet<VeterinarianService> VeterinarianServices => Set<VeterinarianService>();
    public DbSet<VeterinarianAvailability> VeterinarianAvailabilities => Set<VeterinarianAvailability>();
    public DbSet<Pet> Pets => Set<Pet>();
    public DbSet<VaccinationRecord> VaccinationRecords => Set<VaccinationRecord>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<ExaminationNote> ExaminationNotes => Set<ExaminationNote>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<IdentityRole>().HasData(
            CreateRole("clinic-admin", RoleNames.Admin),
            CreateRole("clinic-veterinarian", RoleNames.Veterinarian),
            CreateRole("clinic-member", RoleNames.Member));

        modelBuilder.Entity<ClinicWorkingHour>(entity =>
        {
            entity.HasIndex(hour => new { hour.ClinicId, hour.DayOfWeek, hour.OpensAt }).IsUnique();
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ClinicWorkingHours_Day", """ "DayOfWeek" BETWEEN 0 AND 6 """);
                table.HasCheckConstraint("CK_ClinicWorkingHours_Interval", """ "OpensAt" < "ClosesAt" """);
            });
        });

        modelBuilder.Entity<ClinicService>(entity =>
        {
            entity.HasOne(service => service.Clinic).WithMany(clinic => clinic.Services)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasAlternateKey(service => new { service.Id, service.ClinicId });
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_ClinicServices_Duration", """ "DurationMinutes" BETWEEN 1 AND 1440 """);
                table.HasCheckConstraint("CK_ClinicServices_Fee", """ CAST("Fee" AS NUMERIC) >= 0 """);
            });
        });

        modelBuilder.Entity<Veterinarian>(entity =>
        {
            entity.HasOne(vet => vet.Clinic).WithMany(clinic => clinic.Veterinarians)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(vet => vet.User).WithOne()
                .HasForeignKey<Veterinarian>(vet => vet.UserId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasAlternateKey(vet => new { vet.Id, vet.ClinicId });
            entity.HasMany(vet => vet.Specialties).WithMany(specialty => specialty.Veterinarians)
                .UsingEntity("VeterinarianSpecialties");
        });

        modelBuilder.Entity<Specialty>().HasIndex(specialty => specialty.Name).IsUnique();

        modelBuilder.Entity<VeterinarianService>(entity =>
        {
            entity.HasKey(link => new { link.VeterinarianId, link.ClinicServiceId });

            // Hekim ve hizmet aynı kliniğe ait olmalı.
            entity.HasOne(link => link.Veterinarian).WithMany(vet => vet.VeterinarianServices)
                .HasForeignKey(link => new { link.VeterinarianId, link.ClinicId })
                .HasPrincipalKey(vet => new { vet.Id, vet.ClinicId });
            entity.HasOne(link => link.ClinicService).WithMany(service => service.VeterinarianServices)
                .HasForeignKey(link => new { link.ClinicServiceId, link.ClinicId })
                .HasPrincipalKey(service => new { service.Id, service.ClinicId });
        });

        modelBuilder.Entity<VeterinarianAvailability>(entity =>
        {
            entity.HasIndex(slot => new { slot.VeterinarianId, slot.Date, slot.StartsAt }).IsUnique();
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_VeterinarianAvailabilities_Interval", """ "StartsAt" < "EndsAt" """));
        });

        modelBuilder.Entity<Pet>(entity =>
        {
            entity.HasOne(pet => pet.Owner).WithMany().OnDelete(DeleteBehavior.Restrict);
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_Pets_Weight", """ CAST("WeightKg" AS NUMERIC) > 0 """));
        });

        modelBuilder.Entity<VaccinationRecord>(entity =>
        {
            entity.HasOne(record => record.Pet).WithMany(pet => pet.VaccinationRecords)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(record => new { record.PetId, record.NextDoseOn });
            entity.ToTable(table =>
                table.HasCheckConstraint("CK_VaccinationRecords_NextDose", """ "NextDoseOn" IS NULL OR "NextDoseOn" >= "AdministeredOn" """));
        });

        modelBuilder.Entity<Appointment>(entity =>
        {
            entity.HasOne(appointment => appointment.Pet).WithMany(pet => pet.Appointments)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne(appointment => appointment.VeterinarianService).WithMany()
                .HasForeignKey(appointment => new { appointment.VeterinarianId, appointment.ClinicServiceId })
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(appointment => new { appointment.VeterinarianId, appointment.StartsAtUtc });
            entity.Property(appointment => appointment.StartsAtUtc)
                .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
            entity.Property(appointment => appointment.Status).HasDefaultValue(AppointmentStatus.Pending);
            entity.ToTable(table =>
            {
                table.HasCheckConstraint("CK_Appointments_Duration", """ "DurationMinutes" BETWEEN 1 AND 1440 """);
                table.HasCheckConstraint("CK_Appointments_Fee", """ CAST("Fee" AS NUMERIC) >= 0 """);
                table.HasCheckConstraint("CK_Appointments_Status", """ "Status" BETWEEN 0 AND 3 """);
            });
        });

        modelBuilder.Entity<ExaminationNote>(entity =>
        {
            entity.HasKey(note => note.AppointmentId);
            entity.HasOne(note => note.Appointment).WithOne(appointment => appointment.ExaminationNote)
                .HasForeignKey<ExaminationNote>(note => note.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.Property(note => note.RecordedAtUtc)
                .HasConversion(value => value, value => DateTime.SpecifyKind(value, DateTimeKind.Utc));
        });
    }

    private static IdentityRole CreateRole(string id, string name)
    {
        return new IdentityRole
        {
            Id = id,
            Name = name,
            NormalizedName = name.ToUpperInvariant(),
            ConcurrencyStamp = id
        };
    }
}
