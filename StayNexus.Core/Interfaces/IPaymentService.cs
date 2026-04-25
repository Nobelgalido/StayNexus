using StayNexus.Core.Models;

namespace StayNexus.Core.Interfaces;

public interface IPaymentService
{
    Task<CheckoutSessionResult> CreateCheckoutSessionAsync(int bookingId);
    Task<bool> HandleWebhookAsync(string payload, string signature);
}

public record CheckoutSessionResult(
    string CheckoutUrl,
    string SessionId,
    Payment Payment);