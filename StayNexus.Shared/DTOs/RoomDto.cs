using StayNexus.Core.Enums;

namespace StayNexus.Shared.DTOs;

public class RoomDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public bool IsAvailable { get; set; }
    public int PropertyId { get; set; }
    public string PropertyName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}