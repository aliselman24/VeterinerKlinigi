namespace VeterinerKlinigi.Services.Dtos;

public sealed record AdminDashboardSummaryDto(
    int ClinicCount,
    int ServiceCount,
    int SpecialtyCount,
    int VeterinarianCount,
    int PetCount,
    int VaccinationRecordCount,
    int TotalUserCount,
    int AdminUserCount,
    int VeterinarianUserCount,
    int MemberUserCount,
    int PendingAppointmentCount,
    int ApprovedAppointmentCount,
    int RejectedAppointmentCount,
    int CompletedAppointmentCount,
    int UpcomingAppointmentCount,
    DateTime GeneratedAtUtc)
{
    public static AdminDashboardSummaryDto Empty => new(
        ClinicCount: 0,
        ServiceCount: 0,
        SpecialtyCount: 0,
        VeterinarianCount: 0,
        PetCount: 0,
        VaccinationRecordCount: 0,
        TotalUserCount: 0,
        AdminUserCount: 0,
        VeterinarianUserCount: 0,
        MemberUserCount: 0,
        PendingAppointmentCount: 0,
        ApprovedAppointmentCount: 0,
        RejectedAppointmentCount: 0,
        CompletedAppointmentCount: 0,
        UpcomingAppointmentCount: 0,
        GeneratedAtUtc: DateTime.UtcNow);
}