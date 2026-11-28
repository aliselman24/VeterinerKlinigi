using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.ViewModels;

public sealed class AdminDashboardViewModel
{
    public required AdminDashboardSummaryDto Summary { get; init; }
    public string? ErrorMessage { get; init; }

    public int TotalAppointmentCount =>
        Summary.PendingAppointmentCount +
        Summary.ApprovedAppointmentCount +
        Summary.RejectedAppointmentCount +
        Summary.CompletedAppointmentCount;
}