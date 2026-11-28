using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Services;

public interface IAdminDashboardService
{
    Task<AdminDashboardSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default);
}