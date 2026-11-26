namespace VeterinerKlinigi.Models;

public class VeterinarianService
{
    public int VeterinarianId { get; set; }
    public Veterinarian Veterinarian { get; set; } = null!;

    public int ClinicServiceId { get; set; }
    public ClinicService ClinicService { get; set; } = null!;

    public int ClinicId { get; set; }
}
