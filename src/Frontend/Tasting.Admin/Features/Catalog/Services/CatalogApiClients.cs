using System.Net.Http.Json;
using Tasting.Admin.Features.Catalog.Models;

namespace Tasting.Admin.Features.Catalog.Services;

public interface IBreweriesApiClient
{
    Task<ListBreweriesResponse?> ListAsync(string? searchTerm = null);
    Task<BreweryDto?> GetAsync(int id);
    Task<BreweryDto?> CreateAsync(AddBreweryRequest request);
    Task<BreweryDto?> UpdateAsync(int id, UpdateBreweryRequest request);
}

public class BreweriesApiClient : IBreweriesApiClient
{
    private readonly HttpClient _httpClient;

    public BreweriesApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListBreweriesResponse?> ListAsync(string? searchTerm = null)
    {
        try
        {
            var url = "/api/v1/breweries";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            return await _httpClient.GetFromJsonAsync<ListBreweriesResponse>(url);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list breweries: {ex.Message}", ex);
        }
    }

    public async Task<BreweryDto?> GetAsync(int id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BreweryDto>($"/api/v1/breweries/{id}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get brewery {id}: {ex.Message}", ex);
        }
    }

    public async Task<BreweryDto?> CreateAsync(AddBreweryRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/breweries", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BreweryDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to create brewery: {ex.Message}", ex);
        }
    }

    public async Task<BreweryDto?> UpdateAsync(int id, UpdateBreweryRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/breweries/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BreweryDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to update brewery {id}: {ex.Message}", ex);
        }
    }
}

public interface IBeersApiClient
{
    Task<ListBeersResponse?> ListByBreweryAsync(int breweryId, string? searchTerm = null);
    Task<BeerDto?> GetAsync(int breweryId, int beerId);
    Task<BeerDto?> CreateAsync(int breweryId, AddBeerRequest request);
}

public class BeersApiClient : IBeersApiClient
{
    private readonly HttpClient _httpClient;

    public BeersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListBeersResponse?> ListByBreweryAsync(int breweryId, string? searchTerm = null)
    {
        try
        {
            var url = $"/api/v1/breweries/{breweryId}/beers";
            if (!string.IsNullOrEmpty(searchTerm))
            {
                url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
            }
            return await _httpClient.GetFromJsonAsync<ListBeersResponse>(url);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list beers for brewery {breweryId}: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> GetAsync(int breweryId, int beerId)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BeerDto>($"/api/v1/breweries/{breweryId}/beers/{beerId}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get beer {beerId}: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> CreateAsync(int breweryId, AddBeerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"/api/v1/breweries/{breweryId}/beers", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BeerDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to create beer for brewery {breweryId}: {ex.Message}", ex);
        }
    }
}
