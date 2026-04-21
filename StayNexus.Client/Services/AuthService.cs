using System.Net.Http.Json;
using Blazored.LocalStorage;
using Microsoft.AspNetCore.Components.Authorization;
using StayNexus.Shared.Requests;
using StayNexus.Shared.Responses;

namespace StayNexus.Client.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly ILocalStorageService _localStorage;
    private readonly AuthenticationStateProvider _authStateProvider;

    public AuthService(
        HttpClient httpClient,
        ILocalStorageService localStorage,
        AuthenticationStateProvider authStateProvider)
    {
        _httpClient = httpClient;
        _localStorage = localStorage;
        _authStateProvider = authStateProvider;
    }

    public async Task<AuthResponse?> LoginAsync(LoginRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/login", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse is null)
        {
            return null;
        }

        await _localStorage.SetItemAsync("authToken", authResponse.Token);
        ((JwtAuthStateProvider)_authStateProvider).NotifyAuthStateChanged();

        return authResponse;
    }

    public async Task<AuthResponse?> RegisterAsync(RegisterRequest request)
    {
        var response = await _httpClient.PostAsJsonAsync("api/auth/register", request);

        if (!response.IsSuccessStatusCode)
        {
            return null;
        }

        var authResponse = await response.Content.ReadFromJsonAsync<AuthResponse>();

        if (authResponse is null)
        {
            return null;
        }

        await _localStorage.SetItemAsync("authToken", authResponse.Token);
        ((JwtAuthStateProvider)_authStateProvider).NotifyAuthStateChanged();

        return authResponse;
    }

    public async Task LogoutAsync()
    {
        await _localStorage.RemoveItemAsync("authToken");
        ((JwtAuthStateProvider)_authStateProvider).NotifyAuthStateChanged();
    }
}