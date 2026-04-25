using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using StayNexus.Core.Enums;
using StayNexus.Core.Interfaces;
using StayNexus.Core.Models;

namespace StayNexus.Infrastructure.Services;

public class PaymongoPaymentService : IPaymentService
{
    private readonly HttpClient _httpClient;
    private readonly IBookingRepository _bookingRepository;
    private readonly IPaymentRepository _paymentRepository;
    private readonly PaymongoSettings _settings;
    private readonly ILogger<PaymongoPaymentService> _logger;

    public PaymongoPaymentService(
        HttpClient httpClient,
        IBookingRepository bookingRepository,
        IPaymentRepository paymentRepository,
        IOptions<PaymongoSettings> settings,
        ILogger<PaymongoPaymentService> logger)
    {
        _httpClient = httpClient;
        _bookingRepository = bookingRepository;
        _paymentRepository = paymentRepository;
        _settings = settings.Value;
        _logger = logger;

        ConfigureHttpClient();
    }

    private void ConfigureHttpClient()
    {
        _httpClient.BaseAddress = new Uri("https://api.paymongo.com/");

        var authBytes = Encoding.UTF8.GetBytes($"{_settings.SecretKey}:");
        var authValue = Convert.ToBase64String(authBytes);
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Basic", authValue);
    }

    public async Task<CheckoutSessionResult> CreateCheckoutSessionAsync(int bookingId)
    {
        var booking = await _bookingRepository.GetByIdAsync(bookingId)
            ?? throw new InvalidOperationException($"Booking {bookingId} not found.");

        if (booking.PaymentStatus == PaymentStatus.Paid)
            throw new InvalidOperationException("This booking is already paid.");

        var existingPayment = await _paymentRepository.GetByBookingIdAsync(bookingId);
        if (existingPayment is not null && existingPayment.Status == PaymentStatus.Paid)
            throw new InvalidOperationException("This booking is already paid.");

        var amountInCentavos = (int)(booking.TotalPrice * 100);

        var requestBody = new
        {
            data = new
            {
                attributes = new
                {
                    line_items = new[]
                    {
                        new
                        {
                            currency = "PHP",
                            amount = amountInCentavos,
                            name = $"{booking.Room.Property.Name} — {booking.Room.Name}",
                            quantity = 1,
                            description = $"Booking #{booking.Id}: " +
                                $"{booking.CheckInDate:MMM dd} — {booking.CheckOutDate:MMM dd}"
                        }
                    },
                    payment_method_types = new[] { "card", "gcash", "paymaya" },
                    success_url = _settings.SuccessUrl,
                    cancel_url = _settings.CancelUrl,
                    description = $"StayNexus Booking #{booking.Id}",
                    reference_number = booking.Id.ToString()
                }
            }
        };

        var response = await _httpClient.PostAsJsonAsync(
            "v1/checkout_sessions", requestBody);

        if (!response.IsSuccessStatusCode)
        {
            var errorBody = await response.Content.ReadAsStringAsync();
            _logger.LogError(
                "PayMongo checkout session creation failed for booking {BookingId}. " +
                "Status: {Status}. Body: {Body}",
                bookingId, response.StatusCode, errorBody);
            throw new InvalidOperationException(
                "Failed to create payment session. Please try again.");
        }

        var responseBody = await response.Content.ReadFromJsonAsync<JsonElement>();
        var sessionData = responseBody.GetProperty("data");
        var sessionId = sessionData.GetProperty("id").GetString()!;
        var checkoutUrl = sessionData
            .GetProperty("attributes")
            .GetProperty("checkout_url")
            .GetString()!;

        Payment payment;
        if (existingPayment is not null)
        {
            existingPayment.GatewayReferenceId = sessionId;
            existingPayment.Amount = booking.TotalPrice;
            payment = await _paymentRepository.UpdateAsync(existingPayment);
        }
        else
        {
            payment = await _paymentRepository.CreateAsync(new Payment
            {
                BookingId = bookingId,
                Amount = booking.TotalPrice,
                Gateway = PaymentGateway.PayMongo,
                Status = PaymentStatus.Unpaid,
                GatewayReferenceId = sessionId,
                CreatedAt = DateTime.UtcNow
            });
        }

        _logger.LogInformation(
            "PayMongo checkout session {SessionId} created for booking {BookingId}",
            sessionId, bookingId);

        return new CheckoutSessionResult(checkoutUrl, sessionId, payment);
    }

    public async Task<bool> HandleWebhookAsync(string payload, string signature)
    {
        if (!VerifySignature(payload, signature))
        {
            _logger.LogWarning("PayMongo webhook signature verification failed.");
            return false;
        }

        try
        {
            using var document = JsonDocument.Parse(payload);
            var root = document.RootElement;

            var eventType = root
                .GetProperty("data")
                .GetProperty("attributes")
                .GetProperty("type")
                .GetString();

            _logger.LogInformation("PayMongo webhook received: {EventType}", eventType);

            if (eventType != "checkout_session.payment.paid")
            {
                _logger.LogInformation(
                    "PayMongo webhook ignored — event type {EventType} not handled.",
                    eventType);
                return true;
            }

            var sessionData = root
                .GetProperty("data")
                .GetProperty("attributes")
                .GetProperty("data");

            var sessionId = sessionData.GetProperty("id").GetString()!;

            var payment = await _paymentRepository
                .GetByGatewayReferenceIdAsync(sessionId);

            if (payment is null)
            {
                _logger.LogWarning(
                    "PayMongo webhook — no payment found for session {SessionId}",
                    sessionId);
                return true;
            }

            if (payment.Status == PaymentStatus.Paid)
            {
                _logger.LogInformation(
                    "PayMongo webhook — payment {PaymentId} already paid, skipping.",
                    payment.Id);
                return true;
            }

            payment.Status = PaymentStatus.Paid;
            await _paymentRepository.UpdateAsync(payment);

            var booking = await _bookingRepository.GetByIdAsync(payment.BookingId);
            if (booking is not null)
            {
                booking.PaymentStatus = PaymentStatus.Paid;
                booking.BookingStatus = BookingStatus.Confirmed;
                await _bookingRepository.UpdateAsync(booking);
            }

            _logger.LogInformation(
                "PayMongo webhook processed — payment {PaymentId} for booking {BookingId} confirmed.",
                payment.Id, payment.BookingId);

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "PayMongo webhook processing failed.");
            return false;
        }
    }

    private bool VerifySignature(string payload, string signatureHeader)
    {
        if (string.IsNullOrWhiteSpace(signatureHeader))
            return false;

        var parts = signatureHeader.Split(',');
        string? timestamp = null;
        string? testSignature = null;
        string? liveSignature = null;

        foreach (var part in parts)
        {
            var keyValue = part.Split('=', 2);
            if (keyValue.Length != 2) continue;

            switch (keyValue[0])
            {
                case "t": timestamp = keyValue[1]; break;
                case "te": testSignature = keyValue[1]; break;
                case "li": liveSignature = keyValue[1]; break;
            }
        }

        if (timestamp is null)
            return false;

        var expectedSignature = testSignature ?? liveSignature;
        if (expectedSignature is null)
            return false;

        var signedPayload = $"{timestamp}.{payload}";
        var keyBytes = Encoding.UTF8.GetBytes(_settings.WebhookSecret);
        using var hmac = new System.Security.Cryptography.HMACSHA256(keyBytes);
        var hashBytes = hmac.ComputeHash(Encoding.UTF8.GetBytes(signedPayload));
        var computedSignature = Convert.ToHexString(hashBytes).ToLowerInvariant();

        return CryptographicEquals(computedSignature, expectedSignature);
    }

    private static bool CryptographicEquals(string a, string b)
    {
        if (a.Length != b.Length) return false;

        var result = 0;
        for (var i = 0; i < a.Length; i++)
            result |= a[i] ^ b[i];

        return result == 0;
    }
}