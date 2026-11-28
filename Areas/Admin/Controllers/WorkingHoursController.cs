using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeterinerKlinigi.Areas.Admin.ViewModels;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class WorkingHoursController(IClinicManagementService clinicService) : Controller
{
    private readonly IClinicManagementService _clinicService = clinicService
        ?? throw new ArgumentNullException(nameof(clinicService));

    [HttpGet]
    public async Task<IActionResult> Create(int clinicId, CancellationToken cancellationToken)
    {
        var clinic = await _clinicService.GetClinicDetailAsync(clinicId, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        return View(new WorkingHourFormViewModel
        {
            ClinicId = clinic.Id,
            ClinicName = clinic.Name
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(WorkingHourFormViewModel model, CancellationToken cancellationToken)
    {
        var clinic = await _clinicService.GetClinicDetailAsync(model.ClinicId, cancellationToken);
        if (clinic is null)
        {
            return NotFound();
        }

        model.ClinicName = clinic.Name;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.CreateWorkingHourAsync(
            new ClinicWorkingHourUpsertRequest(model.ClinicId, model.DayOfWeek, model.OpensAt, model.ClosesAt),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Çalışma saati eklenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Çalışma saati eklendi.";
        return RedirectToAction("Details", "Clinics", new { area = "Admin", id = model.ClinicId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var workingHour = await _clinicService.GetWorkingHourAsync(id, cancellationToken);
        if (workingHour is null)
        {
            return NotFound();
        }

        return View(new WorkingHourFormViewModel
        {
            Id = workingHour.Id,
            ClinicId = workingHour.ClinicId,
            ClinicName = workingHour.ClinicName,
            DayOfWeek = workingHour.DayOfWeek,
            OpensAt = workingHour.OpensAt,
            ClosesAt = workingHour.ClosesAt
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, WorkingHourFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        var existing = await _clinicService.GetWorkingHourAsync(id, cancellationToken);
        if (existing is null)
        {
            return NotFound();
        }

        model.ClinicId = existing.ClinicId;
        model.ClinicName = existing.ClinicName;

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.UpdateWorkingHourAsync(
            id,
            new ClinicWorkingHourUpsertRequest(existing.ClinicId, model.DayOfWeek, model.OpensAt, model.ClosesAt),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Çalışma saati güncellenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Çalışma saati güncellendi.";
        return RedirectToAction("Details", "Clinics", new { area = "Admin", id = existing.ClinicId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var workingHour = await _clinicService.GetWorkingHourAsync(id, cancellationToken);
        if (workingHour is null)
        {
            return NotFound();
        }

        return View(workingHour);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await _clinicService.DeleteWorkingHourAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Çalışma saati silinemedi.";
            return RedirectToAction("Index", "Clinics", new { area = "Admin" });
        }

        TempData["SuccessMessage"] = "Çalışma saati silindi.";
        return RedirectToAction("Details", "Clinics", new { area = "Admin", id = result.Value });
    }
}