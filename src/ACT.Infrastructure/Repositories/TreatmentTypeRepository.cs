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

    public async Task<IEnumerable<TreatmentType>> GetAllActiveAsync(int? companyId)
    {
        var query = _context.TreatmentTypes.Where(tt => tt.IsActive);
        if (companyId.HasValue)
            query = query.Where(tt => tt.CompanyId == companyId.Value);
        return await query.ToListAsync();
    }

    public async Task<TreatmentType?> GetByIdAsync(int id)
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

    public async Task<(IEnumerable<TreatmentType> Items, int TotalCount)> GetPagedAsync(int? companyId, int page, int pageSize)
    {
        var query = _context.TreatmentTypes.AsQueryable();
        if (companyId.HasValue)
            query = query.Where(tt => tt.CompanyId == companyId.Value);
        query = query.OrderBy(tt => tt.Name);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }

    public async Task UpdateAsync(TreatmentType treatmentType)
    {
        _context.TreatmentTypes.Update(treatmentType);
        await Task.CompletedTask;
    }
}
