using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface ITreatmentService
{
    Task<IEnumerable<TreatmentDto>> GetDueAsync(int? companyId);
    Task<IEnumerable<TreatmentDto>> GetTodayAsync(int? companyId);
    Task<IEnumerable<TreatmentDto>> GetByClientAsync(int clientId);
    Task<TreatmentDto?> GetByIdAsync(int id);
    Task<TreatmentDto> CreateAsync(int companyId, CreateTreatmentRequest request);
    Task<PagedResult<TreatmentDto>> GetPagedAsync(int? companyId, int page, int pageSize);
    Task<TreatmentDto?> UpdateAsync(int id, UpdateTreatmentRequest request);
    Task<TreatmentDto> CompleteFollowUpAsync(int id, CompleteFollowUpRequest request);
}
