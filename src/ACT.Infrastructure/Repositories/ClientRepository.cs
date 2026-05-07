using ACT.Domain.Entities;
using ACT.Domain.Interfaces;
using ACT.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace ACT.Infrastructure.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly AppDbContext _context;

    public ClientRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Client>> GetAllAsync(int companyId, bool includeArchived = false)
    {
        return await _context.Clients
            .Where(c => c.CompanyId == companyId && (includeArchived || !c.IsArchived))
            .ToListAsync();
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _context.Clients
            .Include(c => c.Treatments)
            .FirstOrDefaultAsync(c => c.Id == id);
    }

    public async Task AddAsync(Client client)
    {
        await _context.Clients.AddAsync(client);
    }

    public async Task UpdateAsync(Client client)
    {
        _context.Clients.Update(client);
        await Task.CompletedTask;
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }

    public async Task<(IEnumerable<Client> Items, int TotalCount)> GetPagedAsync(int companyId, int page, int pageSize)
    {
        var query = _context.Clients
            .Where(c => c.CompanyId == companyId)
            .OrderBy(c => c.LastName);
        var totalCount = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return (items, totalCount);
    }
}
