using System.Net.Http.Json;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.Client.Services;

public class PropertyService
{
    private readonly HttpClient _httpClient;
    private readonly AuthorizedHttpClientFactory _clientFactory;

    public PropertyService(
        HttpClient httpClient,
        AuthorizedHttpClientFactory clientFactory)
    {
        _httpClient = httpClient;
        _clientFactory = clientFactory;
    }

    // Anonymous — storefront browsing
    public async Task<List<PropertyDto>> GetAllPropertiesAsync()
    {
        var result = await _httpClient
            .GetFromJsonAsync<List<PropertyDto>>("api/property");
        return result ?? new List<PropertyDto>();
    }

    public async Task<PropertyDto?> GetByIdAsync(int id)
    {
        return await _httpClient
            .GetFromJsonAsync<PropertyDto>($"api/property/{id}");
    }

    // Admin — requires auth
    public async Task<bool> CreatePropertyAsync(CreatePropertyRequest request)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/property", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeletePropertyAsync(int id)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/property/{id}");
        return response.IsSuccessStatusCode;
    }
}