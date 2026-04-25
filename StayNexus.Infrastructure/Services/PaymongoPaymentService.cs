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

        // PayMongo expects amount in centavos (multiply by 100)
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

        var payment = existingPayment ?? new Payment
        {
            BookingId = bookingId,
            Amount = booking.TotalPrice,
            Gateway = PaymentGateway.PayMongo,
            Status = PaymentStatus.Unpaid,
            GatewayReferenceId = sessionId,
            CreatedAt = DateTime.UtcNow
        };

        if (existingPayment is not null)
        {
            existingPayment.GatewayReferenceId = sessionId;
            existingPayment.Amount = booking.TotalPrice;
            payment = await _paymentRepository.UpdateAsync(existingPayment);
        }
        else
        {
            payment = await _paymentRepository.CreateAsync(payment);
        }

        _logger.LogInformation(
            "PayMongo checkout session {SessionId} created for booking {BookingId}",
            sessionId, bookingId);

        return new CheckoutSessionResult(checkoutUrl, sessionId, payment);
    }

    public Task<bool> HandleWebhookAsync(string payload, string signature)
    {
        // Implemented in Step 5
        throw new NotImplementedException();
    }
}