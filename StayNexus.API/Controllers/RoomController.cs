using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class RoomController : ControllerBase
{
    private readonly IRoomRepository _roomRepository;
    private readonly IPropertyRepository _propertyRepository;

    public RoomController(
        IRoomRepository roomRepository,
        IPropertyRepository propertyRepository)
    {
        _roomRepository = roomRepository;
        _propertyRepository = propertyRepository;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var rooms = await _roomRepository.GetAllAsync();

        var result = rooms.Select(r => new RoomDto
        {
            Id = r.Id,
            Name = r.Name,
            Description = r.Description,
            RoomType = r.RoomType,
            PricePerNight = r.PricePerNight,
            MaxGuests = r.MaxGuests,
            IsAvailable = r.IsAvailable,
            PropertyId = r.PropertyId,
            PropertyName = r.Property.Name,
            CreatedAt = r.CreatedAt
        });

        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound(new { message = "Room not found." });
        }

        var result = new RoomDto
        {
            Id = room.Id,
            Name = room.Name,
            Description = room.Description,
            RoomType = room.RoomType,
            PricePerNight = room.PricePerNight,
            MaxGuests = room.MaxGuests,
            IsAvailable = room.IsAvailable,
            PropertyId = room.PropertyId,
            PropertyName = room.Property.Name,
            CreatedAt = room.CreatedAt
        };

        return Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRoomRequest request)
    {
        var property = await _propertyRepository.GetByIdAsync(request.PropertyId);

        if (property is null)
        {
            return NotFound(new { message = "Property not found." });
        }

        var room = new Room
        {
            Name = request.Name,
            Description = request.Description,
            RoomType = request.RoomType,
            PricePerNight = request.PricePerNight,
            MaxGuests = request.MaxGuests,
            PropertyId = request.PropertyId
        };

        var created = await _roomRepository.CreateAsync(room);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, new { created.Id });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateRoomRequest request)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound(new { message = "Room not found." });
        }

        room.Name = request.Name;
        room.Description = request.Description;
        room.RoomType = request.RoomType;
        room.PricePerNight = request.PricePerNight;
        room.MaxGuests = request.MaxGuests;
        room.IsAvailable = request.IsAvailable;

        await _roomRepository.UpdateAsync(room);

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var room = await _roomRepository.GetByIdAsync(id);

        if (room is null)
        {
            return NotFound(new { message = "Room not found." });
        }

        await _roomRepository.DeleteAsync(id);

        return NoContent();
    }
}