using System.Net.Http.Json;
using Tasting.Admin.Features.Identity.Models;

namespace Tasting.Admin.Features.Identity.Services;

public interface IUsersApiClient
{
    Task<ListUsersResponse?> ListAsync(string? searchTerm = null);
    Task<UserDto?> GetAsync(int id);
    Task<UserDto?> CreateAsync(AddUserRequest request);
    Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request);
    Task<UserDto?> ChangeRoleAsync(int id, ChangeRoleRequest request);
    Task<UserDto?> ChangeStatusAsync(int id, ChangeStatusRequest request);
}

public class UsersApiClient : IUsersApiClient
{
    private readonly HttpClient _httpClient;

    public UsersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListUsersResponse?> ListAsync(string? searchTerm = null)
    {
        try
        {
            var url = "/api/v1/users";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            return await _httpClient.GetFromJsonAsync<ListUsersResponse>(url);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list users: {ex.Message}", ex);
        }
    }

    public async Task<UserDto?> GetAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<UserDto>($"/api/v1/users/{id}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get user {id}: {ex.Message}", ex);
        }
    }

    public async Task<UserDto?> CreateAsync(AddUserRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/users", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to create user: {ex.Message}", ex);
        }
    }

    public async Task<UserDto?> UpdateAsync(int id, UpdateUserRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/users/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to update user {id}: {ex.Message}", ex);
        }
    }

    public async Task<UserDto?> ChangeRoleAsync(int id, ChangeRoleRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/users/{id}/change-role", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to change role for user {id}: {ex.Message}", ex);
        }
    }

    public async Task<UserDto?> ChangeStatusAsync(int id, ChangeStatusRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/users/{id}/change-status", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<UserDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to change status for user {id}: {ex.Message}", ex);
        }
    }
}
