using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;

    public CompanyService(ICompanyRepository companyRepository)
    {
        _companyRepository = companyRepository;
    }

    public async Task<IEnumerable<CompanyDto>> GetAllAsync()
    {
        var companies = await _companyRepository.GetAllAsync();
        return companies.Select(ToDto);
    }

    public async Task<CompanyDto?> GetByIdAsync(int id)
    {
        var company = await _companyRepository.GetByIdAsync(id);
        return company == null ? null : ToDto(company);
    }

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request)
    {
        var entity = new Company
        {
            Name = request.Name,
            ContactEmail = request.ContactEmail,
            Phone = request.Phone,
            Address = request.Address
        };

        await _companyRepository.AddAsync(entity);
        await _companyRepository.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<CompanyDto?> UpdateAsync(int id, UpdateCompanyRequest request)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return null;

        entity.Name = request.Name;
        entity.ContactEmail = request.ContactEmail;
        entity.Phone = request.Phone;
        entity.Address = request.Address;

        await _companyRepository.UpdateAsync(entity);
        await _companyRepository.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize)
    {
        var (items, totalCount) = await _companyRepository.GetPagedAsync(page, pageSize);
        return new PagedResult<CompanyDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    private static CompanyDto ToDto(Company company)
    {
        return new CompanyDto
        {
            Id = company.Id,
            Name = company.Name,
            ContactEmail = company.ContactEmail,
            Phone = company.Phone,
            Address = company.Address
        };
    }
}

