using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request);
    Task<CompanyDto?> UpdateAsync(int id, UpdateCompanyRequest request);
    Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize);
}

