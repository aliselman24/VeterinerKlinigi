using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class Clinic
{
    public int Id { get; set; }

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Required, StringLength(300)]
    public string Address { get; set; } = string.Empty;

    public ICollection<ClinicWorkingHour> WorkingHours { get; } = new List<ClinicWorkingHour>();
    public ICollection<ClinicService> Services { get; } = new List<ClinicService>();
    public ICollection<Veterinarian> Veterinarians { get; } = new List<Veterinarian>();
}
