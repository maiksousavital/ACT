using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class TreatmentTypeRepository : ITreatmentTypeRepository
{
    private readonly AppDbContext _context;

    public TreatmentTypeRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TreatmentType>> GetAllActiveAsync()
    {
        return await _context.TreatmentTypes
            .Where(tt => tt.IsActive)
            .ToListAsync();
    }

    public async Task<TreatmentType?> GetByIdAsync(Guid id)
    {
        return await _context.TreatmentTypes
            .FirstOrDefaultAsync(tt => tt.Id == id);
    }

    public async Task AddAsync(TreatmentType treatmentType)
    {
        await _context.TreatmentTypes.AddAsync(treatmentType);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}

