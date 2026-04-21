using StayNexus.Core.Enums;

namespace StayNexus.Core.Models;

public class Payment
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentGateway Gateway { get; set; }
    public PaymentStatus Status { get; set; } = PaymentStatus.Unpaid;
    public string GatewayReferenceId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Booking Booking { get; set; } = null!;
}
