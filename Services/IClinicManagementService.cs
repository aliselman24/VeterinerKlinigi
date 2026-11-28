using VeterinerKlinigi.Services.Dtos;

namespace VeterinerKlinigi.Services;

public interface IClinicManagementService
{
    Task<IReadOnlyList<ClinicListItemDto>> GetClinicsAsync(CancellationToken cancellationToken = default);
    Task<ClinicDetailDto?> GetClinicDetailAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateClinicAsync(ClinicUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateClinicAsync(int id, ClinicUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteClinicAsync(int id, CancellationToken cancellationToken = default);

    Task<ClinicWorkingHourDto?> GetWorkingHourAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateWorkingHourAsync(ClinicWorkingHourUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateWorkingHourAsync(int id, ClinicWorkingHourUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> DeleteWorkingHourAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<ClinicServiceItemDto>> GetClinicServicesAsync(int? clinicId = null, CancellationToken cancellationToken = default);
    Task<ClinicServiceItemDto?> GetClinicServiceAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateClinicServiceAsync(ClinicServiceUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateClinicServiceAsync(int id, ClinicServiceUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteClinicServiceAsync(int id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<SpecialtyItemDto>> GetSpecialtiesAsync(CancellationToken cancellationToken = default);
    Task<SpecialtyItemDto?> GetSpecialtyAsync(int id, CancellationToken cancellationToken = default);
    Task<ServiceResult<int>> CreateSpecialtyAsync(SpecialtyUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> UpdateSpecialtyAsync(int id, SpecialtyUpsertRequest request, CancellationToken cancellationToken = default);
    Task<ServiceResult> DeleteSpecialtyAsync(int id, CancellationToken cancellationToken = default);
}