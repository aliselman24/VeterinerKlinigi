using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Areas.Admin.ViewModels;

public sealed class ClinicFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Klinik Adı")]
    [Required(ErrorMessage = "Klinik adı zorunludur.")]
    [StringLength(120, ErrorMessage = "Klinik adı en fazla 120 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Adres")]
    [Required(ErrorMessage = "Adres bilgisi zorunludur.")]
    [StringLength(300, ErrorMessage = "Adres en fazla 300 karakter olabilir.")]
    public string Address { get; set; } = string.Empty;
}

public sealed class WorkingHourFormViewModel : IValidatableObject
{
    public int Id { get; set; }

    [Required]
    public int ClinicId { get; set; }

    public string ClinicName { get; set; } = string.Empty;

    [Display(Name = "Gün")]
    [Required(ErrorMessage = "Gün seçimi zorunludur.")]
    public DayOfWeek DayOfWeek { get; set; } = DayOfWeek.Monday;

    [Display(Name = "Açılış Saati")]
    [Required(ErrorMessage = "Açılış saati zorunludur.")]
    public TimeOnly OpensAt { get; set; } = new(9, 0);

    [Display(Name = "Kapanış Saati")]
    [Required(ErrorMessage = "Kapanış saati zorunludur.")]
    public TimeOnly ClosesAt { get; set; } = new(18, 0);

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (OpensAt >= ClosesAt)
        {
            yield return new ValidationResult(
                "Açılış saati kapanış saatinden önce olmalıdır.",
                [nameof(ClosesAt)]);
        }
    }
}

public sealed class ClinicServiceIndexViewModel
{
    public int? SelectedClinicId { get; init; }
    public required IReadOnlyList<SelectListItem> ClinicOptions { get; init; }
    public required IReadOnlyList<ClinicServiceItemDto> Services { get; init; }
}

public sealed class ClinicServiceFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Klinik")]
    [Range(1, int.MaxValue, ErrorMessage = "Lütfen bir klinik seçiniz.")]
    public int ClinicId { get; set; }

    [Display(Name = "Hizmet Adı")]
    [Required(ErrorMessage = "Hizmet adı zorunludur.")]
    [StringLength(120, ErrorMessage = "Hizmet adı en fazla 120 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;

    [Display(Name = "Süre (Dakika)")]
    [Range(1, 1440, ErrorMessage = "Süre 1 ile 1440 dakika arasında olmalıdır.")]
    public int DurationMinutes { get; set; } = 30;

    [Display(Name = "Ücret (TL)")]
    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true, ErrorMessage = "Ücret 0 veya daha büyük bir tutar olmalıdır.")]
    public decimal Fee { get; set; }

    public IReadOnlyList<SelectListItem> ClinicOptions { get; set; } = [];
}

public sealed class SpecialtyFormViewModel
{
    public int Id { get; set; }

    [Display(Name = "Uzmanlık Alanı Adı")]
    [Required(ErrorMessage = "Uzmanlık adı zorunludur.")]
    [StringLength(100, ErrorMessage = "Uzmanlık adı en fazla 100 karakter olabilir.")]
    public string Name { get; set; } = string.Empty;
}