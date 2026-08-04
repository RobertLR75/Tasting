using System.Net.Http.Json;
using Tasting.Admin.Features.Arrangement.Models;

namespace Tasting.Admin.Features.Arrangement.Services;

public interface IArrangementsApiClient
{
    Task<ListArrangementsResponse?> ListAsync();
    Task<ArrangementDetailsDto?> GetAsync(int id);
    Task<ArrangementDto?> CreateAsync(AddArrangementRequest request);
    Task<ArrangementDto?> UpdateAsync(int id, UpdateArrangementRequest request);
    Task<ArrangementDetailsDto?> ChangeStatusAsync(int id, ChangeArrangementStatusRequest request);
    Task<ArrangementDetailsDto?> AddBeerAsync(int id, AddBeerToArrangementRequest request);
    Task<ArrangementDetailsDto?> RemoveBeerAsync(int id, RemoveBeerFromArrangementRequest request);
    Task<ArrangementDetailsDto?> AddParticipantAsync(int id, AddParticipantToArrangementRequest request);
    Task<ArrangementDetailsDto?> RemoveParticipantAsync(int id, RemoveParticipantFromArrangementRequest request);
}

public class ArrangementsApiClient : IArrangementsApiClient
{
    private readonly HttpClient _httpClient;

    public ArrangementsApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListArrangementsResponse?> ListAsync()
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ListArrangementsResponse>("/api/v1/arrangements");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list arrangements: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDetailsDto?> GetAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<ArrangementDetailsDto>($"/api/v1/arrangements/{id}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDto?> CreateAsync(AddArrangementRequest request)
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

    public async Task<ArrangementDto?> UpdateAsync(int id, UpdateArrangementRequest request)
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

    public async Task<ArrangementDetailsDto?> ChangeStatusAsync(int id, ChangeArrangementStatusRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/change-status", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDetailsDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to change status for arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDetailsDto?> AddBeerAsync(int id, AddBeerToArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/beers", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDetailsDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to add beer to arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDetailsDto?> RemoveBeerAsync(int id, RemoveBeerFromArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/beers/remove", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDetailsDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to remove beer from arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDetailsDto?> AddParticipantAsync(int id, AddParticipantToArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/participants", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDetailsDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to add participant to arrangement {id}: {ex.Message}", ex);
        }
    }

    public async Task<ArrangementDetailsDto?> RemoveParticipantAsync(int id, RemoveParticipantFromArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/participants/remove", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<ArrangementDetailsDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to remove participant from arrangement {id}: {ex.Message}", ex);
        }
    }
}
