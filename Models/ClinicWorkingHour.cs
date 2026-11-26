namespace VeterinerKlinigi.Models;

public class ClinicWorkingHour
{
    public int Id { get; set; }
    public int ClinicId { get; set; }
    public Clinic Clinic { get; set; } = null!;

    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly OpensAt { get; set; }
    public TimeOnly ClosesAt { get; set; }
}
