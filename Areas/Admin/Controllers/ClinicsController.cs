using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeterinerKlinigi.Areas.Admin.ViewModels;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class ClinicsController(IClinicManagementService clinicService) : Controller
{
    private readonly IClinicManagementService _clinicService = clinicService
        ?? throw new ArgumentNullException(nameof(clinicService));

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var clinics = await _clinicService.GetClinicsAsync(cancellationToken);
        return View(clinics);
    }

    [HttpGet]
    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var detail = await _clinicService.GetClinicDetailAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return View(detail);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ClinicFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ClinicFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.CreateClinicAsync(
            new ClinicUpsertRequest(model.Name, model.Address),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Klinik oluşturulamadı.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Klinik başarıyla oluşturuldu.";
        return RedirectToAction(nameof(Details), new { id = result.Value });
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var detail = await _clinicService.GetClinicDetailAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return View(new ClinicFormViewModel
        {
            Id = detail.Id,
            Name = detail.Name,
            Address = detail.Address
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ClinicFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.UpdateClinicAsync(
            id,
            new ClinicUpsertRequest(model.Name, model.Address),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Klinik güncellenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Klinik bilgileri güncellendi.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var detail = await _clinicService.GetClinicDetailAsync(id, cancellationToken);
        if (detail is null)
        {
            return NotFound();
        }

        return View(detail);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await _clinicService.DeleteClinicAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Klinik silinemedi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        TempData["SuccessMessage"] = "Klinik silindi.";
        return RedirectToAction(nameof(Index));
    }
}