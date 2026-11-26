using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class VaccinationRecord
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    [Required, StringLength(120)]
    public string VaccineName { get; set; } = string.Empty;

    public DateOnly AdministeredOn { get; set; }
    public DateOnly? NextDoseOn { get; set; }
}
