using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using VeterinerKlinigi.Areas.Admin.ViewModels;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ServicesController(IClinicManagementService clinicService) : Controller
{
    private readonly IClinicManagementService _clinicService = clinicService
        ?? throw new ArgumentNullException(nameof(clinicService));

    [HttpGet]
    public async Task<IActionResult> Index(int? clinicId, CancellationToken cancellationToken)
    {
        var clinics = await _clinicService.GetClinicsAsync(cancellationToken);
        var services = await _clinicService.GetClinicServicesAsync(clinicId, cancellationToken);

        var options = clinics
            .Select(clinic => new SelectListItem
            {
                Value = clinic.Id.ToString(),
                Text = clinic.Name,
                Selected = clinicId.HasValue && clinic.Id == clinicId.Value
            })
            .ToList();

        return View(new ClinicServiceIndexViewModel
        {
            SelectedClinicId = clinicId,
            ClinicOptions = options,
            Services = services
        });
    }

    [HttpGet]
    public async Task<IActionResult> Create(int? clinicId, CancellationToken cancellationToken)
    {
        var options = await GetClinicSelectListAsync(clinicId, cancellationToken);
        if (options.Count == 0)
        {
            TempData["ErrorMessage"] = "Hizmet eklemeden önce en az bir klinik tanımlamanız gerekir.";
            return RedirectToAction("Create", "Clinics", new { area = "Admin" });
        }

        var selectedClinicId = clinicId.HasValue && options.Any(o => o.Value == clinicId.Value.ToString())
            ? clinicId.Value
            : int.Parse(options[0].Value);

        return View(new ClinicServiceFormViewModel
        {
            ClinicId = selectedClinicId,
            ClinicOptions = options
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClinicServiceFormViewModel model, CancellationToken cancellationToken)
    {
        model.ClinicOptions = await GetClinicSelectListAsync(model.ClinicId, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.CreateClinicServiceAsync(
            new ClinicServiceUpsertRequest(model.ClinicId, model.Name, model.DurationMinutes, model.Fee),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Hizmet oluşturulamadı.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Hizmet başarıyla eklendi.";
        return RedirectToAction(nameof(Index), new { clinicId = model.ClinicId });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var service = await _clinicService.GetClinicServiceAsync(id, cancellationToken);
        if (service is null)
        {
            return NotFound();
        }

        return View(new ClinicServiceFormViewModel
        {
            Id = service.Id,
            ClinicId = service.ClinicId,
            Name = service.Name,
            DurationMinutes = service.DurationMinutes,
            Fee = service.Fee,
            ClinicOptions = await GetClinicSelectListAsync(service.ClinicId, cancellationToken)
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClinicServiceFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        model.ClinicOptions = await GetClinicSelectListAsync(model.ClinicId, cancellationToken);

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.UpdateClinicServiceAsync(
            id,
            new ClinicServiceUpsertRequest(model.ClinicId, model.Name, model.DurationMinutes, model.Fee),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Hizmet güncellenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Hizmet bilgileri güncellendi.";
        return RedirectToAction(nameof(Index), new { clinicId = model.ClinicId });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var service = await _clinicService.GetClinicServiceAsync(id, cancellationToken);
        if (service is null)
        {
            return NotFound();
        }

        return View(service);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var existing = await _clinicService.GetClinicServiceAsync(id, cancellationToken);
        var result = await _clinicService.DeleteClinicServiceAsync(id, cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Hizmet silinemedi.";
            return RedirectToAction(nameof(Index), new { clinicId = existing?.ClinicId });
        }

        TempData["SuccessMessage"] = "Hizmet silindi.";
        return RedirectToAction(nameof(Index), new { clinicId = existing?.ClinicId });
    }

    private async Task<IReadOnlyList<SelectListItem>> GetClinicSelectListAsync(
        int? selectedClinicId,
        CancellationToken cancellationToken)
    {
        var clinics = await _clinicService.GetClinicsAsync(cancellationToken);
        return clinics
            .Select(clinic => new SelectListItem
            {
                Value = clinic.Id.ToString(),
                Text = clinic.Name,
                Selected = selectedClinicId.HasValue && clinic.Id == selectedClinicId.Value
            })
            .ToList();
    }
}