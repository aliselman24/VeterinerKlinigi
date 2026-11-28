using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using VeterinerKlinigi.Areas.Admin.ViewModels;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.Controllers;

[Area("Admin")]
[Authorize(Roles = RoleNames.Admin)]
public sealed class SpecialtiesController(IClinicManagementService clinicService) : Controller
{
    private readonly IClinicManagementService _clinicService = clinicService
        ?? throw new ArgumentNullException(nameof(clinicService));

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var specialties = await _clinicService.GetSpecialtiesAsync(cancellationToken);
        return View(specialties);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new SpecialtyFormViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SpecialtyFormViewModel model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.CreateSpecialtyAsync(
            new SpecialtyUpsertRequest(model.Name),
            cancellationToken);

        if (!result.Succeeded)
        {
            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Uzmanlık alanı eklenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Uzmanlık alanı eklendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var specialty = await _clinicService.GetSpecialtyAsync(id, cancellationToken);
        if (specialty is null)
        {
            return NotFound();
        }

        return View(new SpecialtyFormViewModel
        {
            Id = specialty.Id,
            Name = specialty.Name
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, SpecialtyFormViewModel model, CancellationToken cancellationToken)
    {
        if (id != model.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var result = await _clinicService.UpdateSpecialtyAsync(
            id,
            new SpecialtyUpsertRequest(model.Name),
            cancellationToken);

        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Uzmanlık alanı güncellenemedi.");
            return View(model);
        }

        TempData["SuccessMessage"] = "Uzmanlık alanı güncellendi.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var specialty = await _clinicService.GetSpecialtyAsync(id, cancellationToken);
        if (specialty is null)
        {
            return NotFound();
        }

        return View(specialty);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id, CancellationToken cancellationToken)
    {
        var result = await _clinicService.DeleteSpecialtyAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            if (result.ErrorType == ServiceErrorType.NotFound)
            {
                return NotFound();
            }

            TempData["ErrorMessage"] = result.ErrorMessage ?? "Uzmanlık alanı silinemedi.";
            return RedirectToAction(nameof(Index));
        }

        TempData["SuccessMessage"] = "Uzmanlık alanı silindi.";
        return RedirectToAction(nameof(Index));
    }
}