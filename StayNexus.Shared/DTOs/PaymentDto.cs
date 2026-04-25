using StayNexus.Core.Enums;

namespace StayNexus.Shared.DTOs;

public class PaymentDto
{
    public int Id { get; set; }
    public int BookingId { get; set; }
    public decimal Amount { get; set; }
    public PaymentGateway Gateway { get; set; }
    public PaymentStatus Status { get; set; }
    public string GatewayReferenceId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}