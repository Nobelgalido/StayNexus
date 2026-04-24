using System.Net.Http.Json;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.Client.Services;

public class BookingService
{
    private readonly HttpClient _httpClient;
    private readonly AuthorizedHttpClientFactory _clientFactory;

    public BookingService(
        HttpClient httpClient,
        AuthorizedHttpClientFactory clientFactory)
    {
        _httpClient = httpClient;
        _clientFactory = clientFactory;
    }

    // Anonymous — no token needed
    public async Task<bool> CheckAvailabilityAsync(CheckAvailabilityRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/booking/check-availability", request);

        if (!response.IsSuccessStatusCode)
            return false;

        var result = await response.Content
            .ReadFromJsonAsync<AvailabilityResponse>();

        return result?.IsAvailable ?? false;
    }

    // Requires auth
    public async Task<List<BookingDto>> GetMyBookingsAsync()
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var result = await client.GetFromJsonAsync<List<BookingDto>>(
            "api/booking/my-bookings");
        return result ?? new List<BookingDto>();
    }

    public async Task<BookingDto?> GetByIdAsync(int id)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        return await client.GetFromJsonAsync<BookingDto>($"api/booking/{id}");
    }

    public async Task<(bool Success, BookingDto? Booking, string? Error)>
        CreateBookingAsync(CreateBookingRequest request)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/booking", request);

        if (response.IsSuccessStatusCode)
        {
            var booking = await response.Content
                .ReadFromJsonAsync<BookingDto>();
            return (true, booking, null);
        }

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();
        return (false, null, error?.Message ?? "Booking failed.");
    }

    public async Task<(bool Success, string? Error)> CancelBookingAsync(int id)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.PatchAsync(
            $"api/booking/{id}/cancel", null);

        if (response.IsSuccessStatusCode)
            return (true, null);

        var error = await response.Content
            .ReadFromJsonAsync<ErrorResponse>();
        return (false, error?.Message ?? "Cancellation failed.");
    }

    // Local response models for deserializing API shape
    private class AvailabilityResponse
    {
        public bool IsAvailable { get; set; }
    }

    private class ErrorResponse
    {
        public string? Message { get; set; }
    }
}