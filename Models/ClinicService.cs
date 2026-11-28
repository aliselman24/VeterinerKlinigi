using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class ClinicService
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    [Required, StringLength(120)]
    public string Name { get; set; } = string.Empty;

    [Range(1, 1440)]
    public int DurationMinutes { get; set; }

    [Range(typeof(decimal), "0", "99999999.99", ParseLimitsInInvariantCulture = true)]
    public decimal Fee { get; set; }

    public ICollection<VeterinarianService> VeterinarianServices { get; } = new List<VeterinarianService>();
}