using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class Appointment
{
    public int Id { get; set; }
    public int PetId { get; set; }
    public Pet Pet { get; set; } = null!;

    public int VeterinarianId { get; set; }
    public int ClinicServiceId { get; set; }
    public VeterinarianService VeterinarianService { get; set; } = null!;

    public DateTime StartsAtUtc { get; set; }

    // Hizmet sonradan değişse bile randevunun süresi ve ücreti aynı kalır.
    [Range(1, 1440)]
    public int DurationMinutes { get; set; }

    [Range(typeof(decimal), "0", "99999999.99")]
    public decimal Fee { get; set; }

    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public ExaminationNote? ExaminationNote { get; set; }
}
