using StayNexus.Core.Enums;

namespace StayNexus.Core.Models;

public class Booking
{
    public int Id { get; set; }
    public string GuestId { get; set; } = string.Empty;
    public int RoomId { get; set; }
    public DateTime CheckInDate { get; set; }
    public DateTime CheckOutDate { get; set; }
    public int NumberOfGuests { get; set; }
    public decimal TotalPrice { get; set; }
    public BookingStatus BookingStatus { get; set; } = BookingStatus.Pending;
    public PaymentStatus PaymentStatus { get; set; } = PaymentStatus.Unpaid;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ApplicationUser Guest { get; set; } = null!;
    public Room Room { get; set; } = null!;
    public Payment? Payment { get; set; }
}