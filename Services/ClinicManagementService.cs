using Microsoft.EntityFrameworkCore;
using VeterinerKlinigi.Data;
using VeterinerKlinigi.Infrastructure;
using VeterinerKlinigi.Models;
using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Services;

public sealed class ClinicManagementService(
    ApplicationDbContext dbContext,
    ILogger<ClinicManagementService> logger) : IClinicManagementService
{
    private readonly ApplicationDbContext _dbContext = dbContext
        ?? throw new ArgumentNullException(nameof(dbContext));
    private readonly ILogger<ClinicManagementService> _logger = logger
        ?? throw new ArgumentNullException(nameof(logger));

    public async Task<IReadOnlyList<ClinicListItemDto>> GetClinicsAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Clinics
            .AsNoTracking()
            .OrderBy(clinic => clinic.Name)
            .Select(clinic => new ClinicListItemDto(
                clinic.Id,
                clinic.Name,
                clinic.Address,
                clinic.WorkingHours.Count,
                clinic.Services.Count,
                clinic.Veterinarians.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicDetailDto?> GetClinicDetailAsync(int id, CancellationToken cancellationToken = default)
    {
        var clinic = await _dbContext.Clinics
            .AsNoTracking()
            .Where(item => item.Id == id)
            .Select(item => new
            {
                item.Id,
                item.Name,
                item.Address,
                VeterinarianCount = item.Veterinarians.Count,
                WorkingHours = item.WorkingHours
                    .Select(hour => new
                    {
                        hour.Id,
                        hour.ClinicId,
                        hour.DayOfWeek,
                        hour.OpensAt,
                        hour.ClosesAt
                    })
                    .ToList(),
                Services = item.Services
                    .Select(service => new
                    {
                        service.Id,
                        service.ClinicId,
                        service.Name,
                        service.DurationMinutes,
                        service.Fee,
                        AssignedVeterinarianCount = service.VeterinarianServices.Count
                    })
                    .ToList()
            })
            .FirstOrDefaultAsync(cancellationToken);

        if (clinic is null)
        {
            return null;
        }

        var orderedHours = clinic.WorkingHours
            .OrderBy(hour => DayOfWeekHelper.GetSortOrder(hour.DayOfWeek))
            .ThenBy(hour => hour.OpensAt)
            .Select(hour => new ClinicWorkingHourDto(
                hour.Id,
                hour.ClinicId,
                clinic.Name,
                hour.DayOfWeek,
                hour.OpensAt,
                hour.ClosesAt))
            .ToList();

        var orderedServices = clinic.Services
            .OrderBy(service => service.Name, StringComparer.CurrentCultureIgnoreCase)
            .Select(service => new ClinicServiceItemDto(
                service.Id,
                service.ClinicId,
                clinic.Name,
                service.Name,
                service.DurationMinutes,
                service.Fee,
                service.AssignedVeterinarianCount))
            .ToList();

        return new ClinicDetailDto(
            clinic.Id,
            clinic.Name,
            clinic.Address,
            clinic.VeterinarianCount,
            orderedHours,
            orderedServices);
    }

    public async Task<ServiceResult<int>> CreateClinicAsync(
        ClinicUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateClinic(request);
        if (validationError is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.Validation, validationError);
        }

        var entity = new Clinic
        {
            Name = request.Name.Trim(),
            Address = request.Address.Trim()
        };

        try
        {
            _dbContext.Clinics.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult<int>.Ok(entity.Id);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Klinik kaydedilirken veritabanı hatası oluştu.");
            return ServiceResult<int>.Fail(ServiceErrorType.Unexpected, "Klinik kaydedilemedi.");
        }
    }

    public async Task<ServiceResult> UpdateClinicAsync(
        int id,
        ClinicUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateClinic(request);
        if (validationError is not null)
        {
            return ServiceResult.Fail(ServiceErrorType.Validation, validationError);
        }

        var entity = await _dbContext.Clinics.FirstOrDefaultAsync(clinic => clinic.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Klinik kaydı bulunamadı.");
        }

        entity.Name = request.Name.Trim();
        entity.Address = request.Address.Trim();

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Klinik güncellenirken hata oluştu. ClinicId={ClinicId}", id);
            return ServiceResult.Fail(ServiceErrorType.Unexpected, "Klinik güncellenemedi.");
        }
    }

    public async Task<ServiceResult> DeleteClinicAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Clinics.FirstOrDefaultAsync(clinic => clinic.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Klinik kaydı bulunamadı.");
        }

        var hasServices = await _dbContext.ClinicServices.AsNoTracking()
            .AnyAsync(service => service.ClinicId == id, cancellationToken);
        if (hasServices)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Bu kliniğe bağlı hizmetler bulunduğu için silinemez. Önce bağlı hizmetleri kaldırın.");
        }

        var hasVeterinarians = await _dbContext.Veterinarians.AsNoTracking()
            .AnyAsync(vet => vet.ClinicId == id, cancellationToken);
        if (hasVeterinarians)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Bu klinikte görevli veteriner hekimler bulunduğu için silinemez.");
        }

        try
        {
            _dbContext.Clinics.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Klinik silinirken hata oluştu. ClinicId={ClinicId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Bağlı kayıtlar nedeniyle klinik silinemedi.");
        }
    }

    public async Task<ClinicWorkingHourDto?> GetWorkingHourAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClinicWorkingHours
            .AsNoTracking()
            .Where(hour => hour.Id == id)
            .Select(hour => new ClinicWorkingHourDto(
                hour.Id,
                hour.ClinicId,
                hour.Clinic.Name,
                hour.DayOfWeek,
                hour.OpensAt,
                hour.ClosesAt))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateWorkingHourAsync(
        ClinicWorkingHourUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateWorkingHour(request);
        if (validationError is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.Validation, validationError);
        }

        var clinicExists = await _dbContext.Clinics.AsNoTracking()
            .AnyAsync(clinic => clinic.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.NotFound, "Seçilen klinik bulunamadı.");
        }

        if (await HasOverlappingWorkingHourAsync(request.ClinicId, request.DayOfWeek, request.OpensAt, request.ClosesAt, null, cancellationToken))
        {
            return ServiceResult<int>.Fail(
                ServiceErrorType.Conflict,
                $"{DayOfWeekHelper.GetTurkishName(request.DayOfWeek)} günü için girilen saat aralığı mevcut bir çalışma saatiyle çakışıyor.");
        }

        var entity = new ClinicWorkingHour
        {
            ClinicId = request.ClinicId,
            DayOfWeek = request.DayOfWeek,
            OpensAt = request.OpensAt,
            ClosesAt = request.ClosesAt
        };

        try
        {
            _dbContext.ClinicWorkingHours.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult<int>.Ok(entity.Id);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Çalışma saati eklenirken hata oluştu. ClinicId={ClinicId}", request.ClinicId);
            return ServiceResult<int>.Fail(ServiceErrorType.Conflict, "Çalışma saati kaydedilemedi.");
        }
    }

    public async Task<ServiceResult> UpdateWorkingHourAsync(
        int id,
        ClinicWorkingHourUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateWorkingHour(request);
        if (validationError is not null)
        {
            return ServiceResult.Fail(ServiceErrorType.Validation, validationError);
        }

        var entity = await _dbContext.ClinicWorkingHours.FirstOrDefaultAsync(hour => hour.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Çalışma saati kaydı bulunamadı.");
        }

        if (await HasOverlappingWorkingHourAsync(entity.ClinicId, request.DayOfWeek, request.OpensAt, request.ClosesAt, id, cancellationToken))
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                $"{DayOfWeekHelper.GetTurkishName(request.DayOfWeek)} günü için girilen saat aralığı mevcut bir çalışma saatiyle çakışıyor.");
        }

        entity.DayOfWeek = request.DayOfWeek;
        entity.OpensAt = request.OpensAt;
        entity.ClosesAt = request.ClosesAt;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Çalışma saati güncellenirken hata oluştu. WorkingHourId={WorkingHourId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Çalışma saati güncellenemedi.");
        }
    }

    public async Task<ServiceResult<int>> DeleteWorkingHourAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClinicWorkingHours.FirstOrDefaultAsync(hour => hour.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.NotFound, "Çalışma saati kaydı bulunamadı.");
        }

        var clinicId = entity.ClinicId;
        try
        {
            _dbContext.ClinicWorkingHours.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult<int>.Ok(clinicId);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Çalışma saati silinirken hata oluştu. WorkingHourId={WorkingHourId}", id);
            return ServiceResult<int>.Fail(ServiceErrorType.Unexpected, "Çalışma saati silinemedi.");
        }
    }

    public async Task<IReadOnlyList<ClinicServiceItemDto>> GetClinicServicesAsync(
        int? clinicId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _dbContext.ClinicServices.AsNoTracking();
        if (clinicId.HasValue)
        {
            query = query.Where(service => service.ClinicId == clinicId.Value);
        }

        return await query
            .OrderBy(service => service.Clinic.Name)
            .ThenBy(service => service.Name)
            .Select(service => new ClinicServiceItemDto(
                service.Id,
                service.ClinicId,
                service.Clinic.Name,
                service.Name,
                service.DurationMinutes,
                service.Fee,
                service.VeterinarianServices.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<ClinicServiceItemDto?> GetClinicServiceAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.ClinicServices
            .AsNoTracking()
            .Where(service => service.Id == id)
            .Select(service => new ClinicServiceItemDto(
                service.Id,
                service.ClinicId,
                service.Clinic.Name,
                service.Name,
                service.DurationMinutes,
                service.Fee,
                service.VeterinarianServices.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateClinicServiceAsync(
        ClinicServiceUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateClinicService(request);
        if (validationError is not null)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.Validation, validationError);
        }

        var clinicExists = await _dbContext.Clinics.AsNoTracking()
            .AnyAsync(clinic => clinic.Id == request.ClinicId, cancellationToken);
        if (!clinicExists)
        {
            return ServiceResult<int>.Fail(ServiceErrorType.NotFound, "Seçilen klinik bulunamadı.");
        }

        var trimmedName = request.Name.Trim();
        var existingNames = await _dbContext.ClinicServices
            .AsNoTracking()
            .Where(service => service.ClinicId == request.ClinicId)
            .Select(service => service.Name)
            .ToListAsync(cancellationToken);

        if (existingNames.Any(name => string.Equals(name, trimmedName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult<int>.Fail(
                ServiceErrorType.Conflict,
                "Bu klinikte aynı ada sahip bir hizmet zaten tanımlı.");
        }

        var entity = new ClinicService
        {
            ClinicId = request.ClinicId,
            Name = trimmedName,
            DurationMinutes = request.DurationMinutes,
            Fee = decimal.Round(request.Fee, 2, MidpointRounding.AwayFromZero)
        };

        try
        {
            _dbContext.ClinicServices.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult<int>.Ok(entity.Id);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Hizmet eklenirken hata oluştu. ClinicId={ClinicId}", request.ClinicId);
            return ServiceResult<int>.Fail(ServiceErrorType.Unexpected, "Hizmet kaydedilemedi.");
        }
    }

    public async Task<ServiceResult> UpdateClinicServiceAsync(
        int id,
        ClinicServiceUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var validationError = ValidateClinicService(request);
        if (validationError is not null)
        {
            return ServiceResult.Fail(ServiceErrorType.Validation, validationError);
        }

        var entity = await _dbContext.ClinicServices.FirstOrDefaultAsync(service => service.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Hizmet kaydı bulunamadı.");
        }

        if (entity.ClinicId != request.ClinicId)
        {
            var clinicExists = await _dbContext.Clinics.AsNoTracking()
                .AnyAsync(clinic => clinic.Id == request.ClinicId, cancellationToken);
            if (!clinicExists)
            {
                return ServiceResult.Fail(ServiceErrorType.NotFound, "Seçilen klinik bulunamadı.");
            }

            var hasLinks = await _dbContext.VeterinarianServices.AsNoTracking()
                .AnyAsync(link => link.ClinicServiceId == id, cancellationToken);
            var hasAppointments = await _dbContext.Appointments.AsNoTracking()
                .AnyAsync(appointment => appointment.ClinicServiceId == id, cancellationToken);

            if (hasLinks || hasAppointments)
            {
                return ServiceResult.Fail(
                    ServiceErrorType.Conflict,
                    "Bu hizmet veteriner hekimlere veya randevulara bağlı olduğu için kliniği değiştirilemez.");
            }
        }

        var trimmedName = request.Name.Trim();
        var existingNames = await _dbContext.ClinicServices
            .AsNoTracking()
            .Where(service => service.ClinicId == request.ClinicId && service.Id != id)
            .Select(service => service.Name)
            .ToListAsync(cancellationToken);

        if (existingNames.Any(name => string.Equals(name, trimmedName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Seçilen klinikte aynı ada sahip başka bir hizmet zaten tanımlı.");
        }

        entity.ClinicId = request.ClinicId;
        entity.Name = trimmedName;
        entity.DurationMinutes = request.DurationMinutes;
        entity.Fee = decimal.Round(request.Fee, 2, MidpointRounding.AwayFromZero);

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Hizmet güncellenirken hata oluştu. ServiceId={ServiceId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Hizmet güncellenemedi.");
        }
    }

    public async Task<ServiceResult> DeleteClinicServiceAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.ClinicServices.FirstOrDefaultAsync(service => service.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Hizmet kaydı bulunamadı.");
        }

        var hasAppointments = await _dbContext.Appointments.AsNoTracking()
            .AnyAsync(appointment => appointment.ClinicServiceId == id, cancellationToken);
        if (hasAppointments)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Bu hizmete ait randevu kayıtları bulunduğu için silinemez.");
        }

        var hasVeterinarianLinks = await _dbContext.VeterinarianServices.AsNoTracking()
            .AnyAsync(link => link.ClinicServiceId == id, cancellationToken);
        if (hasVeterinarianLinks)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Bu hizmet veteriner hekimlere atanmış olduğu için önce hekim atamalarını kaldırın.");
        }

        try
        {
            _dbContext.ClinicServices.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Hizmet silinirken hata oluştu. ServiceId={ServiceId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Bağlı kayıtlar nedeniyle hizmet silinemedi.");
        }
    }

    public async Task<IReadOnlyList<SpecialtyItemDto>> GetSpecialtiesAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.Specialties
            .AsNoTracking()
            .OrderBy(specialty => specialty.Name)
            .Select(specialty => new SpecialtyItemDto(
                specialty.Id,
                specialty.Name,
                specialty.Veterinarians.Count))
            .ToListAsync(cancellationToken);
    }

    public async Task<SpecialtyItemDto?> GetSpecialtyAsync(int id, CancellationToken cancellationToken = default)
    {
        return await _dbContext.Specialties
            .AsNoTracking()
            .Where(specialty => specialty.Id == id)
            .Select(specialty => new SpecialtyItemDto(
                specialty.Id,
                specialty.Name,
                specialty.Veterinarians.Count))
            .FirstOrDefaultAsync(cancellationToken);
    }

    public async Task<ServiceResult<int>> CreateSpecialtyAsync(
        SpecialtyUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.Length > 100)
        {
            return ServiceResult<int>.Fail(
                ServiceErrorType.Validation,
                "Uzmanlık adı zorunludur ve en fazla 100 karakter olabilir.");
        }

        var existingNames = await _dbContext.Specialties
            .AsNoTracking()
            .Select(specialty => specialty.Name)
            .ToListAsync(cancellationToken);

        if (existingNames.Any(name => string.Equals(name, trimmedName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult<int>.Fail(ServiceErrorType.Conflict, "Bu uzmanlık alanı zaten tanımlı.");
        }

        var entity = new Specialty { Name = trimmedName };

        try
        {
            _dbContext.Specialties.Add(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult<int>.Ok(entity.Id);
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Uzmanlık alanı eklenirken hata oluştu.");
            return ServiceResult<int>.Fail(ServiceErrorType.Conflict, "Uzmanlık alanı kaydedilemedi.");
        }
    }

    public async Task<ServiceResult> UpdateSpecialtyAsync(
        int id,
        SpecialtyUpsertRequest request,
        CancellationToken cancellationToken = default)
    {
        var trimmedName = request.Name?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(trimmedName) || trimmedName.Length > 100)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Validation,
                "Uzmanlık adı zorunludur ve en fazla 100 karakter olabilir.");
        }

        var entity = await _dbContext.Specialties.FirstOrDefaultAsync(specialty => specialty.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Uzmanlık alanı bulunamadı.");
        }

        var existingNames = await _dbContext.Specialties
            .AsNoTracking()
            .Where(specialty => specialty.Id != id)
            .Select(specialty => specialty.Name)
            .ToListAsync(cancellationToken);

        if (existingNames.Any(name => string.Equals(name, trimmedName, StringComparison.CurrentCultureIgnoreCase)))
        {
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Aynı ada sahip başka bir uzmanlık alanı mevcut.");
        }

        entity.Name = trimmedName;

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Uzmanlık alanı güncellenirken hata oluştu. SpecialtyId={SpecialtyId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Uzmanlık alanı güncellenemedi.");
        }
    }

    public async Task<ServiceResult> DeleteSpecialtyAsync(int id, CancellationToken cancellationToken = default)
    {
        var entity = await _dbContext.Specialties.FirstOrDefaultAsync(specialty => specialty.Id == id, cancellationToken);
        if (entity is null)
        {
            return ServiceResult.Fail(ServiceErrorType.NotFound, "Uzmanlık alanı bulunamadı.");
        }

        var inUse = await _dbContext.Veterinarians.AsNoTracking()
            .AnyAsync(vet => vet.Specialties.Any(specialty => specialty.Id == id), cancellationToken);
        if (inUse)
        {
            return ServiceResult.Fail(
                ServiceErrorType.Conflict,
                "Bu uzmanlık alanı veteriner hekimlere atanmış olduğu için silinemez.");
        }

        try
        {
            _dbContext.Specialties.Remove(entity);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return ServiceResult.Ok();
        }
        catch (DbUpdateException exception)
        {
            _logger.LogError(exception, "Uzmanlık alanı silinirken hata oluştu. SpecialtyId={SpecialtyId}", id);
            return ServiceResult.Fail(ServiceErrorType.Conflict, "Bağlı kayıtlar nedeniyle uzmanlık alanı silinemedi.");
        }
    }

    private async Task<bool> HasOverlappingWorkingHourAsync(
        int clinicId,
        DayOfWeek dayOfWeek,
        TimeOnly opensAt,
        TimeOnly closesAt,
        int? excludeId,
        CancellationToken cancellationToken)
    {
        var existingHours = await _dbContext.ClinicWorkingHours
            .AsNoTracking()
            .Where(hour => hour.ClinicId == clinicId && hour.DayOfWeek == dayOfWeek)
            .Select(hour => new { hour.Id, hour.OpensAt, hour.ClosesAt })
            .ToListAsync(cancellationToken);

        return existingHours.Any(hour =>
            (!excludeId.HasValue || hour.Id != excludeId.Value) &&
            hour.OpensAt < closesAt &&
            opensAt < hour.ClosesAt);
    }

    private static string? ValidateClinic(ClinicUpsertRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        var address = request.Address?.Trim() ?? string.Empty;

        if (string.IsNullOrWhiteSpace(name) || name.Length > 120)
        {
            return "Klinik adı zorunludur ve en fazla 120 karakter olabilir.";
        }

        if (string.IsNullOrWhiteSpace(address) || address.Length > 300)
        {
            return "Klinik adresi zorunludur ve en fazla 300 karakter olabilir.";
        }

        return null;
    }

    private static string? ValidateWorkingHour(ClinicWorkingHourUpsertRequest request)
    {
        if (!Enum.IsDefined(request.DayOfWeek))
        {
            return "Geçerli bir gün seçiniz.";
        }

        if (request.OpensAt >= request.ClosesAt)
        {
            return "Açılış saati kapanış saatinden önce olmalıdır.";
        }

        return null;
    }

    private static string? ValidateClinicService(ClinicServiceUpsertRequest request)
    {
        var name = request.Name?.Trim() ?? string.Empty;
        if (request.ClinicId <= 0)
        {
            return "Geçerli bir klinik seçiniz.";
        }

        if (string.IsNullOrWhiteSpace(name) || name.Length > 120)
        {
            return "Hizmet adı zorunludur ve en fazla 120 karakter olabilir.";
        }

        if (request.DurationMinutes is < 1 or > 1440)
        {
            return "Hizmet süresi 1 ile 1440 dakika arasında olmalıdır.";
        }

        if (request.Fee is < 0 or > 99999999.99m)
        {
            return "Hizmet ücreti 0 ile 99.999.999,99 TL arasında olmalıdır.";
        }

        return null;
    }
}