using ACT.Domain.Entities;

namespace ACT.Domain.Interfaces;

public interface ICompanyRepository
{
    Task<IEnumerable<Company>> GetAllAsync();
    Task<Company?> GetByIdAsync(int id);
    Task AddAsync(Company company);
    Task UpdateAsync(Company company);
    Task SaveChangesAsync();
    Task<(IEnumerable<Company> Items, int TotalCount)> GetPagedAsync(int page, int pageSize);
}

