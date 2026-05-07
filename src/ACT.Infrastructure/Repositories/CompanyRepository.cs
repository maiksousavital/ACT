using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class CompanyRepository : ICompanyRepository
{
    private readonly AppDbContext _context;

    public CompanyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Company>> GetAllAsync()
    {
        return await _context.Companies
            .OrderBy(c => c.Name)
            .ToListAsync();
    }

    public async Task<Company?> GetByIdAsync(int id)
    {
        return await _context.Companies
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Company company)
    {
        await _context.Companies.AddAsync(company);
    }

    public async Task UpdateAsync(Company company)
    {
        _context.Companies.Update(company);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Company> Items, int TotalCount)> GetPagedAsync(int page, int pageSize)
    {
        var query = _context.Companies.OrderBy(c => c.Name);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}

