using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Services;

public sealed class AdminDashboardService(ApplicationDbContext dbContext) : IAdminDashboardService
{
    private readonly ApplicationDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));

    public async Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var nowUtc = DateTime.UtcNow;

        // EF Core DbContext eşzamanlı sorgu desteklemediği için sorgular sırayla çalıştırılır.
        var clinicCount = await _dbContext.Clinics.AsNoTracking().CountAsync(cancellationToken);
        var serviceCount = await _dbContext.ClinicServices.AsNoTracking().CountAsync(cancellationToken);
        var specialtyCount = await _dbContext.Specialties.AsNoTracking().CountAsync(cancellationToken);
        var veterinarianCount = await _dbContext.Veterinarians.AsNoTracking().CountAsync(cancellationToken);
        var petCount = await _dbContext.Pets.AsNoTracking().CountAsync(cancellationToken);
        var vaccinationRecordCount = await _dbContext.VaccinationRecords.AsNoTracking().CountAsync(cancellationToken);
        var totalUserCount = await _dbContext.Users.AsNoTracking().CountAsync(cancellationToken);

        var roleGroups = await (
            from userRole in _dbContext.UserRoles.AsNoTracking()
            join role in _dbContext.Roles.AsNoTracking() on userRole.RoleId equals role.Id
            where role.Name != null
            group userRole by role.Name into grouped
            select new { RoleName = grouped.Key!, Count = grouped.Count() }
        ).ToListAsync(cancellationToken);

        var roleCounts = roleGroups.ToDictionary(
            item => item.RoleName,
            item => item.Count,
            StringComparer.OrdinalIgnoreCase);

        var appointmentGroups = await _dbContext.Appointments
            .AsNoTracking()
            .GroupBy(appointment => appointment.Status)
            .Select(grouped => new { Status = grouped.Key, Count = grouped.Count() })
            .ToListAsync(cancellationToken);

        var appointmentCounts = appointmentGroups.ToDictionary(item => item.Status, item => item.Count);

        var upcomingAppointmentCount = await _dbContext.Appointments
            .AsNoTracking()
            .CountAsync(
                appointment => appointment.StartsAtUtc >= nowUtc &&
                    (appointment.Status == AppointmentStatus.Pending ||
                     appointment.Status == AppointmentStatus.Approved),
                cancellationToken);

        return new AdminDashboardSummaryDto(
            ClinicCount: clinicCount,
            ServiceCount: serviceCount,
            SpecialtyCount: specialtyCount,
            VeterinarianCount: veterinarianCount,
            PetCount: petCount,
            VaccinationRecordCount: vaccinationRecordCount,
            TotalUserCount: totalUserCount,
            AdminUserCount: roleCounts.GetValueOrDefault(RoleNames.Admin),
            VeterinarianUserCount: roleCounts.GetValueOrDefault(RoleNames.Veterinarian),
            MemberUserCount: roleCounts.GetValueOrDefault(RoleNames.Member),
            PendingAppointmentCount: appointmentCounts.GetValueOrDefault(AppointmentStatus.Pending),
            ApprovedAppointmentCount: appointmentCounts.GetValueOrDefault(AppointmentStatus.Approved),
            RejectedAppointmentCount: appointmentCounts.GetValueOrDefault(AppointmentStatus.Rejected),
            CompletedAppointmentCount: appointmentCounts.GetValueOrDefault(AppointmentStatus.Completed),
            UpcomingAppointmentCount: upcomingAppointmentCount,
            GeneratedAtUtc: nowUtc);
    }
}