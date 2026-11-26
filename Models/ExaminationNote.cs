using System.ComponentModel.DataAnnotations;

namespace VeterinerKlinigi.Models;

public class ExaminationNote
{
    public int AppointmentId { get; set; }
    public Appointment Appointment { get; set; } = null!;

    [Required, StringLength(2000)]
    public string Notes { get; set; } = string.Empty;

    public DateTime RecordedAtUtc { get; set; }
}
