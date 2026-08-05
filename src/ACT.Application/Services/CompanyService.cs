using ACT.Application.Common;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class CompanyService : ICompanyService
{
    private readonly ICompanyRepository _companyRepository;
    private readonly IAuditService _auditService;

    public CompanyService(ICompanyRepository companyRepository, IAuditService auditService)
    {
        _companyRepository = companyRepository;
        _auditService = auditService;
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

    public async Task<CompanyDto> CreateAsync(CreateCompanyRequest request, int? userId, string userEmail)
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
        await _auditService.LogAsync(userId, userEmail, entity.Id, "Create", "Company", entity.Id);
        return ToDto(entity);
    }

    public async Task<CompanyDto?> UpdateAsync(int id, UpdateCompanyRequest request, int? userId, string userEmail)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return null;

        var changes = AuditDiff.Compare(
            ("Name", entity.Name, request.Name),
            ("ContactEmail", entity.ContactEmail, request.ContactEmail),
            ("Phone", entity.Phone, request.Phone),
            ("Address", entity.Address, request.Address)
        );

        entity.Name = request.Name;
        entity.ContactEmail = request.ContactEmail;
        entity.Phone = request.Phone;
        entity.Address = request.Address;

        await _companyRepository.UpdateAsync(entity);
        await _companyRepository.SaveChangesAsync();
        await _auditService.LogChangesAsync(userId, userEmail, id, "Company", id, changes);
        return ToDto(entity);
    }

    public async Task<PagedResult<CompanyDto>> GetPagedAsync(int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _companyRepository.GetPagedAsync(page, pageSize);
        return new PagedResult<CompanyDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<bool> DeleteAsync(int id, int? userId, string userEmail)
    {
        var entity = await _companyRepository.GetByIdAsync(id);
        if (entity == null) return false;

        entity.IsDeleted = true;
        await _companyRepository.UpdateAsync(entity);
        await _companyRepository.SaveChangesAsync();
        await _auditService.LogAsync(userId, userEmail, id, "Delete", "Company", id);
        return true;
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

