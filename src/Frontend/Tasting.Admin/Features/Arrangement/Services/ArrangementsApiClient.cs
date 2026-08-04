using System.Net.Http.Json;
using Tasting.Admin.Features.Arrangement.Models;

namespace Tasting.Admin.Features.Arrangement.Services;

public interface IArrangementsApiClient
{
    Task<ListArrangementsResponse?> ListAsync(string? searchTerm = null);
    Task<ArrangementDto?> GetAsync(Guid id);
    Task<ArrangementDto?> CreateAsync(CreateArrangementRequest request);
    Task<ArrangementDto?> UpdateAsync(Guid id, UpdateArrangementRequest request);
    Task<ArrangementDto?> ChangeStatusAsync(Guid id, ChangeArrangementStatusRequest request);
    Task<ArrangementBeerDto?> AddBeerAsync(Guid id, AddBeerToArrangementRequest request);
    Task<bool> RemoveBeerAsync(Guid id, Guid beerId);
    Task<ArrangementParticipantDto?> AddParticipantAsync(Guid id, AddParticipantToArrangementRequest request);
    Task<bool> RemoveParticipantAsync(Guid id, Guid userId);
}

public class ArrangementsApiClient : IArrangementsApiClient
{
    private readonly HttpClient _httpClient;

    public ArrangementsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListArrangementsResponse?> ListAsync(string? searchTerm = null)
    {
        try
        {
            var url = "/api/v1/arrangements";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            return await _httpClient.GetFromJsonAsync<ListArrangementsResponse>(url);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list arrangements: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDto?> GetAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ArrangementDto>($"/api/v1/arrangements/{id}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDto?> CreateAsync(CreateArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/arrangements", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to create arrangement: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDto?> UpdateAsync(Guid id, UpdateArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/arrangements/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to update arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDto?> ChangeStatusAsync(Guid id, ChangeArrangementStatusRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/change-status", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to change arrangement status: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementBeerDto?> AddBeerAsync(Guid id, AddBeerToArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/beers", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementBeerDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to add beer to arrangement: {ex.Message}", ex);
        }
    }

    public async Task<bool> RemoveBeerAsync(Guid id, Guid beerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/arrangements/{id}/beers/{beerId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to remove beer from arrangement: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementParticipantDto?> AddParticipantAsync(Guid id, AddParticipantToArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/participants", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementParticipantDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to add participant to arrangement: {ex.Message}", ex);
        }
    }

    public async Task<bool> RemoveParticipantAsync(Guid id, Guid userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"/api/v1/arrangements/{id}/participants/{userId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to remove participant from arrangement: {ex.Message}", ex);
        }
    }
}
