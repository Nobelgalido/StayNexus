using System.Net.Http.Json;
using StayNexus.Shared.DTOs;
using StayNexus.Shared.Requests;

namespace StayNexus.Client.Services;

public class RoomService
{
    private readonly AuthorizedHttpClientFactory _clientFactory;

    public RoomService(AuthorizedHttpClientFactory clientFactory)
    {
        _clientFactory = clientFactory;
    }

    public async Task<List<RoomDto>> GetAllRoomsAsync()
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var result = await client.GetFromJsonAsync<List<RoomDto>>("api/room");
        return result ?? new List<RoomDto>();
    }

    public async Task<bool> CreateRoomAsync(CreateRoomRequest request)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.PostAsJsonAsync("api/room", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> UpdateRoomAsync(int id, UpdateRoomRequest request)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.PutAsJsonAsync($"api/room/{id}", request);
        return response.IsSuccessStatusCode;
    }

    public async Task<bool> DeleteRoomAsync(int id)
    {
        var client = await _clientFactory.CreateAuthorizedClientAsync();
        var response = await client.DeleteAsync($"api/room/{id}");
        return response.IsSuccessStatusCode;
    }
}