using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PropertyController : ControllerBase
{
    private readonly IPropertyRepository _propertyRepository;

    public PropertyController(IPropertyRepository propertyRepository)
    {
        _propertyRepository = propertyRepository;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        var properties = await _propertyRepository.GetAllAsync();

        var result = properties.Select(p => new PropertyDto
        {
            Id = p.Id,
            Name = p.Name,
            Description = p.Description,
            Address = p.Address,
            City = p.City,
            Province = p.Province,
            OwnerName = $"{p.Owner.FirstName} {p.Owner.LastName}",
            CreatedAt = p.CreatedAt,
            PrimaryColor = p.PrimaryColor,
            LogoUrl = p.LogoUrl,
            HeroImageUrl = p.HeroImageUrl,
            Tagline = p.Tagline

        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    [AllowAnonymous]
    public async Task<IActionResult> GetById(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);

        if (property is null)
            return NotFound(new { message = "Property not found." });

        var result = new PropertyDto
        {
            Id = property.Id,
            Name = property.Name,
            Description = property.Description,
            Address = property.Address,
            City = property.City,
            Province = property.Province,
            OwnerName = $"{property.Owner.FirstName} {property.Owner.LastName}",
            CreatedAt = property.CreatedAt,
            PrimaryColor = property.PrimaryColor,
            LogoUrl = property.LogoUrl,
            HeroImageUrl = property.HeroImageUrl,
            Tagline = property.Tagline,
            Rooms = property.Rooms.Select(r => new RoomDto
            {
                Id = r.Id,
                Name = r.Name,
                Description = r.Description,
                RoomType = r.RoomType,
                PricePerNight = r.PricePerNight,
                MaxGuests = r.MaxGuests,
                IsAvailable = r.IsAvailable,
                PropertyId = r.PropertyId,
                CreatedAt = r.CreatedAt
            }).ToList()
        };

        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] CreatePropertyRequest request)
    {
        var ownerId = User.FindFirstValue(ClaimTypes.NameIdentifier)!;

        var property = new Property
        {
            Name = request.Name,
            Description = request.Description,
            Address = request.Address,
            City = request.City,
            Province = request.Province,
            OwnerId = ownerId
        };

        var created = await _propertyRepository.CreateAsync(property);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { created.Id });
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var property = await _propertyRepository.GetByIdAsync(id);

        if (property is null)
            return NotFound(new { message = "Property not found." });

        await _propertyRepository.DeleteAsync(id);

        return NoContent();
    }
}