using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;


public interface ITreatmentTypeRepository
{
    Task<IEnumerable<TreatmentType>> GetAllActiveAsync();
    Task<TreatmentType?> GetByIdAsync(Guid id);
    Task AddAsync(TreatmentType treatmentType);
    Task SaveChangesAsync();
}