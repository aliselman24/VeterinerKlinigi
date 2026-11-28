namespace VeterinerKlinigi.Services.Dtos;

public sealed record ClinicListItemDto(
    int Id,
    string Name,
    string Address,
    int WorkingHourCount,
    int ServiceCount,
    int VeterinarianCount);

public sealed record ClinicWorkingHourDto(
    int Id,
    int ClinicId,
    string ClinicName,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt);

public sealed record ClinicServiceItemDto(
    int Id,
    int ClinicId,
    string ClinicName,
    string Name,
    int DurationMinutes,
    decimal Fee,
    int AssignedVeterinarianCount);

public sealed record ClinicDetailDto(
    int Id,
    string Name,
    string Address,
    int VeterinarianCount,
    IReadOnlyList<ClinicWorkingHourDto> WorkingHours,
    IReadOnlyList<ClinicServiceItemDto> Services);

public sealed record SpecialtyItemDto(
    int Id,
    string Name,
    int VeterinarianCount);

public sealed record ClinicUpsertRequest(
    string Name,
    string Address);

public sealed record ClinicWorkingHourUpsertRequest(
    int ClinicId,
    DayOfWeek DayOfWeek,
    TimeOnly OpensAt,
    TimeOnly ClosesAt);

public sealed record ClinicServiceUpsertRequest(
    int ClinicId,
    string Name,
    int DurationMinutes,
    decimal Fee);

public sealed record SpecialtyUpsertRequest(
    string Name);