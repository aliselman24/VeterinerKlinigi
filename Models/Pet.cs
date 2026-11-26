using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VeterinerKlinigi.Models;

public class Pet
{
    public int Id { get; set; }

    [Required]
    public string OwnerId { get; set; } = string.Empty;

    public IdentityUser Owner { get; set; } = null!;

    [Required, StringLength(80)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Species { get; set; } = string.Empty;

    [Required, StringLength(80)]
    public string Breed { get; set; } = string.Empty;

    public DateOnly BirthDate { get; set; }

    [Range(typeof(decimal), "0.001", "99999.999")]
    public decimal WeightKg { get; set; }

    public ICollection<VaccinationRecord> VaccinationRecords { get; } = new List<VaccinationRecord>();
    public ICollection<Appointment> Appointments { get; } = new List<Appointment>();
}
