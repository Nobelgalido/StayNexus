using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface IPropertyRepository
{
    Task<IEnumerable<Property>> GetAllAsync();
    Task<Property?> GetByIdAsync(int id);
    Task<IEnumerable<Property>> GetByOwnerIdAsync(string ownerId);
    Task<Property> CreateAsync(Property property);
    Task<Property> UpdateAsync(Property property);
    Task DeleteAsync(int id);
}