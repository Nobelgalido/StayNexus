using StayNexus.Core.Enums;

namespace StayNexus.Shared.Requests;

public class CreateRoomRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public int PropertyId { get; set; }
}