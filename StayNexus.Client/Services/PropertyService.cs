using System.Net.Http.Json;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.Client.Services;

public class PropertyService
{
    private readonly AuthorizedHttpClientFactory _clientFactory;

    public PropertyService(AuthorizedHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<List<PropertyDto>> GetAllPropertiesAsync()
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var result = await client.GetFromJsonAsync<List<PropertyDto>>("api/property");
        return result ?? new List<PropertyDto>();
    }

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