using Microsoft.EntityFrameworkCore;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Infrastructure.Data;

namespace StayNexus.Infrastructure.Repositories;

public class PropertyRepository : IPropertyRepository
{
    private readonly AppDbContext _context;

    public PropertyRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Property>> GetAllAsync()
    {
        return await _context.Properties
            .Include(p => p.Owner)
            .ToListAsync();
    }

    public async Task<Property?> GetByIdAsync(int id)
    {
        return await _context.Properties
            .Include(p => p.Owner)
            .Include(p => p.Rooms)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId)
    {
        return await _context.Properties
            .Include(p => p.Rooms)
            .Where(p => p.OwnerId == ownerId)
            .ToListAsync();
    }

    public async Task<Property> CreateAsync(Property property)
    {
        _context.Properties.Add(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task<Property> UpdateAsync(Property property)
    {
        _context.Properties.Update(property);
        await _context.SaveChangesAsync();
        return property;
    }

    public async Task DeleteAsync(int id)
    {
        var property = await _context.Properties.FindAsync(id);
        if (property is not null)
        {
            _context.Properties.Remove(property);
            await _context.SaveChangesAsync();
        }
    }
}