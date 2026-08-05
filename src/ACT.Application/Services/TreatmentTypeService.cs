using ACT.Application.Common;
using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class TreatmentTypeService : ITreatmentTypeService
{
    private readonly ITreatmentTypeRepository _treatmentTypeRepository;
    private readonly IAuditService _auditService;

    public TreatmentTypeService(ITreatmentTypeRepository treatmentTypeRepository, IAuditService auditService)
    {
        _treatmentTypeRepository = treatmentTypeRepository;
        _auditService = auditService;
    }

    public async Task<IEnumerable<TreatmentType>> GetAllActiveAsync(int? companyId)
    {
        return await _treatmentTypeRepository.GetAllActiveAsync(companyId);
    }

    public async Task<TreatmentTypeDto?> GetByIdAsync(int id)
    {
        var type = await _treatmentTypeRepository.GetByIdAsync(id);
        return type == null ? null : ToDto(type);
    }

    private static TreatmentTypeDto ToDto(TreatmentType type)
    {
        return new TreatmentTypeDto
        {
            Id = type.Id,
            CompanyId = type.CompanyId,
            Name = type.Name,
            FollowUpIntervalDays = type.FollowUpIntervalDays
        };
    }

    public async Task<TreatmentTypeDto> CreateAsync(int companyId, CreateTreatmentTypeRequest request, int? userId, string userEmail)
    {
        var entity = new TreatmentType
        {
            Name = request.Name,
            FollowUpIntervalDays = request.FollowUpIntervalDays,
            IsActive = request.IsActive,
            CompanyId = companyId
        };
        await _treatmentTypeRepository.AddAsync(entity);
        await _treatmentTypeRepository.SaveChangesAsync();
        await _auditService.LogAsync(userId, userEmail, companyId, "Create", "TreatmentType", entity.Id);
        return ToDto(entity);
    }

    public async Task<TreatmentTypeDto?> UpdateAsync(int id, UpdateTreatmentTypeRequest request, int? userId, string userEmail)
    {
        var entity = await _treatmentTypeRepository.GetByIdAsync(id);
        if (entity == null) return null;

        var changes = AuditDiff.Compare(
            ("Name", entity.Name, request.Name),
            ("FollowUpIntervalDays", entity.FollowUpIntervalDays, request.FollowUpIntervalDays),
            ("IsActive", entity.IsActive, request.IsActive)
        );

        entity.Name = request.Name;
        entity.FollowUpIntervalDays = request.FollowUpIntervalDays;
        entity.IsActive = request.IsActive;
        await _treatmentTypeRepository.UpdateAsync(entity);
        await _treatmentTypeRepository.SaveChangesAsync();
        await _auditService.LogChangesAsync(userId, userEmail, entity.CompanyId, "TreatmentType", id, changes);
        return ToDto(entity);
    }

    public async Task<PagedResult<TreatmentTypeDto>> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        (page, pageSize) = Paging.Clamp(page, pageSize);
        var (items, totalCount) = await _treatmentTypeRepository.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<TreatmentTypeDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }
}
