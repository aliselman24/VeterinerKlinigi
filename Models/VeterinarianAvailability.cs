namespace VeterinerKlinigi.Models;

public class VeterinarianAvailability
{
    public int Id { get; set; }
    public int VeterinarianId { get; set; }
    public Veterinarian Veterinarian { get; set; } = null!;

    // Hekimin belirli bir günde randevu alabileceği saat aralığı.
    public DateOnly Date { get; set; }
    public TimeOnly StartsAt { get; set; }
    public TimeOnly EndsAt { get; set; }
}
