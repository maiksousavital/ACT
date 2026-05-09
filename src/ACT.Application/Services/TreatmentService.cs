using ACT.Application.Dtos;
using ACT.Application.Services.Interfaces;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class TreatmentService : ITreatmentService
{
    private readonly ITreatmentRepository     _treatments;
    private readonly IClientRepository        _clients;
    private readonly ITreatmentTypeRepository _types;

    public TreatmentService(
        ITreatmentRepository     treatments,
        IClientRepository        clients,
        ITreatmentTypeRepository types)
    {
        _treatments = treatments;
        _clients    = clients;
        _types      = types;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<TreatmentDto>> GetDueAsync(int? companyId)
    {
        var treatments = await _treatments.GetDueAsync(companyId);
        return treatments.Select(ToDto);
    }

    public async Task<IEnumerable<TreatmentDto>> GetTodayAsync(int? companyId)
    {
        var treatments = await _treatments.GetTodayAsync(companyId);
        return treatments.Select(ToDto);
    }

    public async Task<IEnumerable<TreatmentDto>> GetByClientAsync(int clientId)
    {
        var treatments = await _treatments.GetByClientAsync(clientId);
        return treatments.Select(ToDto);
    }

    public async Task<TreatmentDto?> GetByIdAsync(int id)
    {
        var treatment = await _treatments.GetByIdAsync(id);
        return treatment == null ? null : ToDto(treatment);
    }

    public async Task<PagedResult<TreatmentDto>> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var (items, totalCount) = await _treatments.GetPagedAsync(companyId, page, pageSize);
        return new PagedResult<TreatmentDto>
        {
            Items = items.Select(ToDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<TreatmentDto> CreateAsync(int companyId, CreateTreatmentRequest request)
    {
        var client = await _clients.GetByIdAsync(request.ClientId)
            ?? throw new KeyNotFoundException($"Client {request.ClientId} not found");

        var type = await _types.GetByIdAsync(request.TreatmentTypeId)
            ?? throw new KeyNotFoundException($"TreatmentType {request.TreatmentTypeId} not found");

        var treatment = Treatment.Create(
            clientId: client.Id,
            type:     type,
            date:     request.TreatmentDate,
            notes:    request.Notes
        );
        treatment.CompanyId = companyId;

        await _treatments.AddAsync(treatment);
        await _treatments.SaveChangesAsync();

        // reload with navigation properties for the response
        var saved = await _treatments.GetByIdAsync(treatment.Id)
            ?? throw new Exception("Failed to reload saved treatment");

        return ToDto(saved);
    }

    public async Task<TreatmentDto> CompleteFollowUpAsync(
        int id, CompleteFollowUpRequest request)
    {
        var treatment = await _treatments.GetByIdAsync(id)
            ?? throw new KeyNotFoundException($"Treatment {id} not found");

        if (treatment.IsFollowedUp)
            throw new InvalidOperationException(
                "This follow-up has already been completed.");

        // mark the current treatment as followed up
        treatment.FollowedUpAt   = DateTime.UtcNow;
        treatment.FollowUpNotes  = request.FollowUpNotes;

        // immediately schedule the next follow-up as a new treatment record
        var next = Treatment.Create(
            clientId: treatment.ClientId,
            type:     treatment.TreatmentType,
            date:     DateTime.UtcNow,
            notes:    null
        );
        next.CompanyId = treatment.CompanyId;

        await _treatments.AddAsync(next);
        await _treatments.SaveChangesAsync();

        return ToDto(treatment);
    }

    public async Task<TreatmentDto?> UpdateAsync(int id, UpdateTreatmentRequest request)
    {
        var treatment = await _treatments.GetByIdAsync(id);
        if (treatment == null)
            return null;

        // Update properties
        treatment.ClientId = request.ClientId;
        treatment.TreatmentTypeId = request.TreatmentTypeId;
        treatment.TreatmentDate = request.TreatmentDate;
        treatment.Notes = request.Notes;
        treatment.NextFollowUpDate = request.NextFollowUpDate;
        treatment.FollowedUpAt = request.FollowedUpAt;
        treatment.FollowUpNotes = request.FollowUpNotes;

        await _treatments.UpdateAsync(treatment);
        await _treatments.SaveChangesAsync();
        return ToDto(treatment);
    }

    // ── Mapping ───────────────────────────────────────────────────────────────

    private static TreatmentDto ToDto(Treatment t)
    {
        if (t.Client == null)
            throw new InvalidOperationException(
                $"Treatment {t.Id} was loaded without its Client. " +
                "All repository queries must include .Include(t => t.Client).");

        if (t.TreatmentType == null)
            throw new InvalidOperationException(
                $"Treatment {t.Id} was loaded without its TreatmentType. " +
                "All repository queries must include .Include(t => t.TreatmentType).");

        return new TreatmentDto
        {
            Id                = t.Id,
            ClientId          = t.ClientId,
            ClientName        = t.Client.FullName,
            TreatmentTypeId   = t.TreatmentTypeId,
            TreatmentTypeName = t.TreatmentType.Name,
            Phone             = t.Client.Phone,
            TreatmentDate     = t.TreatmentDate,
            NextFollowUpDate  = t.NextFollowUpDate,
            Notes             = t.Notes,
            FollowedUpAt      = t.FollowedUpAt,
            FollowUpNotes     = t.FollowUpNotes,
            IsDue             = t.IsDue,
            IsFollowedUp      = t.IsFollowedUp,
        };
    }
}