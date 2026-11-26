using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class Specialty
{
    public int Id { get; set; }

    [Required, StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public ICollection<Veterinarian> Veterinarians { get; } = new List<Veterinarian>();
}
