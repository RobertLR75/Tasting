using System.Net.Http.Json;
using System.Net;
using Tasting.Admin.Features.Arrangement.Models;

namespace Tasting.Admin.Features.Arrangement.Services;

public interface IArrangementsApiClient
{
    Task<ListArrangementsResponse?> ListAsync(string? searchTerm = null);
    Task<ArrangementDto?> GetAsync(Guid id);
    Task<ArrangementDto?> CreateAsync(CreateArrangementRequest request);
    Task<ArrangementDto?> UpdateAsync(Guid id, UpdateArrangementRequest request);
    Task<ArrangementDto?> ActivateAsync(Guid id);
    Task<ArrangementDto?> StartAsync(Guid id);
    Task<ArrangementDto?> CancelAsync(Guid id);
    Task<ArrangementDto?> ReopenAsync(Guid id);
    Task<ArrangementDto?> CompleteAsync(Guid id);
    Task<ArrangementDto?> AddBeerAsync(Guid id, AddBeerToArrangementRequest request);
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
            await EnsureSuccessWithApiMessageAsync(response);
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
            await EnsureArrangementSuccessAsync(id, response);
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to update arrangement {id}: {ex.Message}", ex);
        }
    }

    public Task<ArrangementDto?> ActivateAsync(Guid id)
        => PostLifecycleAsync(id, "activate", "activate arrangement");

    public Task<ArrangementDto?> StartAsync(Guid id)
        => PostLifecycleAsync(id, "start", "start arrangement");

    public Task<ArrangementDto?> CancelAsync(Guid id)
        => PostLifecycleAsync(id, "cancel", "cancel arrangement");

    public Task<ArrangementDto?> ReopenAsync(Guid id)
        => PostLifecycleAsync(id, "reopen", "reopen arrangement");

    public Task<ArrangementDto?> CompleteAsync(Guid id)
        => PostLifecycleAsync(id, "complete", "complete arrangement");

    public async Task<ArrangementDto?> AddBeerAsync(Guid id, AddBeerToArrangementRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/arrangements/{id}/beers", request);
            await EnsureArrangementSuccessAsync(id, response);
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (HttpRequestException)
        {
            throw;
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
            await EnsureArrangementSuccessAsync(id, response);
            return true;
        }
        catch (HttpRequestException)
        {
            throw;
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
            await EnsureArrangementSuccessAsync(id, response);
            return await response.Content.ReadFromJsonAsync<ArrangementParticipantDto>();
        }
        catch (HttpRequestException)
        {
            throw;
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
            await EnsureArrangementSuccessAsync(id, response);
            return true;
        }
        catch (HttpRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to remove participant from arrangement: {ex.Message}", ex);
        }
    }

    private async Task<ArrangementDto?> PostLifecycleAsync(
        Guid id,
        string action,
        string errorAction)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"/api/v1/arrangements/{id}/{action}",
                new ArrangementLifecycleRequest());
            await EnsureArrangementSuccessAsync(id, response);
            return await response.Content.ReadFromJsonAsync<ArrangementDto>();
        }
        catch (HttpRequestException ex)
        {
            throw new HttpRequestException($"Failed to {errorAction}: {ex.Message}", ex, ex.StatusCode);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to {errorAction}: {ex.Message}", ex);
        }
    }

    private static async Task EnsureSuccessWithApiMessageAsync(HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
        {
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        throw new HttpRequestException(
            error?.Message ?? $"The API returned {(int)response.StatusCode}.",
            null,
            response.StatusCode);
    }

    private async Task EnsureArrangementSuccessAsync(Guid id, HttpResponseMessage response)
    {
        if (response.StatusCode != HttpStatusCode.Conflict)
        {
            await EnsureSuccessWithApiMessageAsync(response);
            return;
        }

        var error = await response.Content.ReadFromJsonAsync<ApiError>();
        var freshArrangement = await GetAsync(id);
        throw new ArrangementConflictException(
            error?.Message ?? "Arrangement was modified concurrently. Please reload and retry.",
            freshArrangement);
    }

    private sealed record ApiError(string Code, string Message, string CorrelationId);
}
