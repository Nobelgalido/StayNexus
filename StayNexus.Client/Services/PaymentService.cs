using System.Net.Http.Json;
using StayNexus.Shared.Requests;
using StayNexus.Shared.Responses;

namespace StayNexus.Client.Services;

public class PaymentService
{
    private readonly AuthorizedHttpClientFactory _clientFactory;

    public PaymentService(AuthorizedHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<(bool Success, CheckoutSessionResponse? Session, string? Error)>
        CreateCheckoutAsync(int bookingId)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();

        var request = new CreatePaymentRequest { BookingId = bookingId };
        var response = await client.PostAsJsonAsync("api/payment/checkout", request);

        if (response.IsSuccessStatusCode)
        {
            var session = await response.Content
                .ReadFromJsonAsync<CheckoutSessionResponse>();
            return (true, session, null);
        }

        var error = await response.Content.ReadFromJsonAsync<ErrorResponse>();
        return (false, null, error?.Message ?? "Failed to create payment session.");
    }

    private class ErrorResponse
    {
        public string? Message { get; set; }
    }
}