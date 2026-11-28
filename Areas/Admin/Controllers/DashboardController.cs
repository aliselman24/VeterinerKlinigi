using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeterinerKlinigi.Areas.Admin.ViewModels;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class DashboardController(
    IAdminDashboardService dashboardService,
    ILogger<DashboardController> logger) : Controller
{
    private readonly IAdminDashboardService _dashboardService = dashboardService
        ?? throw new ArgumentNullException(nameof(dashboardService));
    private readonly ILogger<DashboardController> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        try
        {
            var summary = await _dashboardService.GetSummaryAsync(cancellationToken);
            return View(new AdminDashboardViewModel { Summary = summary });
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            _logger.LogError(exception, "Admin paneli özet verileri yüklenirken hata oluştu.");
            return View(new AdminDashboardViewModel
            {
                Summary = AdminDashboardSummaryDto.Empty,
                ErrorMessage = "Panel istatistikleri şu anda yüklenemedi. Lütfen veritabanı bağlantısını kontrol edip tekrar deneyin."
            });
        }
    }
}