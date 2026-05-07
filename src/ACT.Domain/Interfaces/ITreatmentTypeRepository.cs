using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface ITreatmentTypeRepository
{
    Task<IEnumerable<TreatmentType>> GetAllActiveAsync(int companyId);
    Task<TreatmentType?> GetByIdAsync(int id);
    Task AddAsync(TreatmentType treatmentType);
    Task UpdateAsync(TreatmentType treatmentType);
    Task SaveChangesAsync();
    Task<(IEnumerable<TreatmentType> Items, int TotalCount)> GetPagedAsync(int companyId, int page, int pageSize);
}