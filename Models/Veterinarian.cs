using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace VeterinerKlinigi.Models;

public class Veterinarian
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    [Required]
    public string UserId { get; set; } = string.Empty;

    public IdentityUser User { get; set; } = null!;

    [Required, StringLength(120)]
    public string FullName { get; set; } = string.Empty;

    public ICollection<Specialty> Specialties { get; } = new List<Specialty>();
    public ICollection<VeterinarianAvailability> Availability { get; } = new List<VeterinarianAvailability>();
    public ICollection<VeterinarianService> VeterinarianServices { get; } = new List<VeterinarianService>();
}
