using ACT.Application.Dtos;

namespace ACT.Application.Services.Interfaces;

public interface ICompanyService
{
    Task<IEnumerable<CompanyDto>> GetAllAsync();
    Task<CompanyDto?> GetByIdAsync(int id);
    Task<CompanyDto> CreateAsync(CreateCompanyRequest request, int? userId, string userEmail);
    Task<CompanyDto?> UpdateAsync(int id, UpdateCompanyRequest request, int? userId, string userEmail);
    Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize);
    Task<bool> DeleteAsync(int id, int? userId, string userEmail);
}

