using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface ITreatmentService
{
    Task<IEnumerable<TreatmentDto>> GetDueAsync(int? companyId);
    Task<IEnumerable<TreatmentDto>> GetTodayAsync(int? companyId);
    Task<IEnumerable<TreatmentDto>> GetByClientAsync(int clientId, int? companyId);
    Task<TreatmentDto?> GetByIdAsync(int id);
    Task<TreatmentDto> CreateAsync(int companyId, CreateTreatmentRequest request, int? userId, string userEmail);
    Task<PagedResult<TreatmentDto>> GetPagedAsync(int? companyId, int page, int pageSize);
    Task<TreatmentDto?> UpdateAsync(int id, UpdateTreatmentRequest request, int? userId, string userEmail);
    Task<TreatmentDto> CompleteFollowUpAsync(int id, CompleteFollowUpRequest request, int? userId, string userEmail);
}
