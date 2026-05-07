using ACT.Application.Dtos;
using ACT.Domain.Entities;

namespace ACT.Application.Services.Interfaces;

public interface ITreatmentTypeService
{
    Task<IEnumerable<TreatmentType>> GetAllActiveAsync(int companyId);
    Task<TreatmentTypeDto?> GetByIdAsync(int id);
    Task<PagedResult<TreatmentTypeDto>> GetPagedAsync(int companyId, int page, int pageSize);
    Task<TreatmentTypeDto> CreateAsync(int companyId, CreateTreatmentTypeRequest request);
    Task<TreatmentTypeDto?> UpdateAsync(int id, UpdateTreatmentTypeRequest request);
}
