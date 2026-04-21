using StayNexus.Core.Enums;

namespace StayNexus.Core.Models;

public class Room
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public RoomType RoomType { get; set; }
    public decimal PricePerNight { get; set; }
    public int MaxGuests { get; set; }
    public bool IsAvailable { get; set; } = true;
    public int PropertyId { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Property Property { get; set; } = null!;
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
