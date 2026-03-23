using ACT.Application.Dtos;
using ACT.Domain.Entities;
using ACT.Domain.Interfaces;

namespace ACT.Application.Services;

public class TreatmentService
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

    public async Task<IEnumerable<TreatmentDto>> GetDueAsync()
    {
        var treatments = await _treatments.GetDueAsync();
        return treatments.Select(ToDto);
    }

    public async Task<IEnumerable<TreatmentDto>> GetTodayAsync()
    {
        var treatments = await _treatments.GetTodayAsync();
        return treatments.Select(ToDto);
    }

    public async Task<IEnumerable<TreatmentDto>> GetByClientAsync(Guid clientId)
    {
        var treatments = await _treatments.GetByClientAsync(clientId);
        return treatments.Select(ToDto);
    }

    public async Task<TreatmentDto?> GetByIdAsync(Guid id)
    {
        var treatment = await _treatments.GetByIdAsync(id);
        if (treatment == null)
            return null;
        return ToDto(treatment);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<TreatmentDto> CreateAsync(CreateTreatmentRequest request)
    {
        var client = await _clients.GetByIdAsync(request.ClientId)
            ?? throw new KeyNotFoundException(
                   $"Client {request.ClientId} not found");

        var type = await _types.GetByIdAsync(request.TreatmentTypeId)
            ?? throw new KeyNotFoundException(
                   $"TreatmentType {request.TreatmentTypeId} not found");

        var treatment = Treatment.Create(
            clientId: client.Id,
            type:     type,
            date:     request.TreatmentDate,
            notes:    request.Notes
        );

        await _treatments.AddAsync(treatment);
        await _treatments.SaveChangesAsync();

        // reload with navigation properties for the response
        var saved = await _treatments.GetByIdAsync(treatment.Id)
            ?? throw new Exception("Failed to reload saved treatment");

        return ToDto(saved);
    }

    public async Task<TreatmentDto> CompleteFollowUpAsync(
        Guid id, CompleteFollowUpRequest request)
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

        await _treatments.AddAsync(next);
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