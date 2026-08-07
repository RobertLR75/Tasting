using System.Net.Http.Json;
using Tasting.Admin.Features.Catalog.Models;

namespace Tasting.Admin.Features.Catalog.Services;

public interface IBreweriesApiClient
{
    Task<ListBreweriesResponse?> ListAsync(string? searchTerm = null);
    Task<BreweryDto?> GetAsync(Guid id);
    Task<BreweryDto?> CreateAsync(CreateBreweryRequest request);
    Task<BreweryDto?> UpdateAsync(Guid id, UpdateBreweryRequest request);
    Task<BreweryDto?> DeactivateAsync(Guid id);
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

    public async Task<BreweryDto?> GetAsync(Guid id)
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

    public async Task<BreweryDto?> CreateAsync(CreateBreweryRequest request)
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

    public async Task<BreweryDto?> UpdateAsync(Guid id, UpdateBreweryRequest request)
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

    public async Task<BreweryDto?> DeactivateAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/breweries/{id}/deactivate", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BreweryDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to deactivate brewery {id}: {ex.Message}", ex);
        }
    }
}

public interface IBeersApiClient
{
    Task<ListBeersResponse?> ListAsync(Guid? breweryId = null, string? searchTerm = null);
    Task<BeerDto?> GetAsync(Guid id);
    Task<BeerDto?> CreateAsync(CreateBeerRequest request);
    Task<BeerDto?> UpdateAsync(Guid id, UpdateBeerRequest request);
    Task<BeerDto?> DeactivateAsync(Guid id);
}

public class BeersApiClient : IBeersApiClient
{
    private readonly HttpClient _httpClient;

    public BeersApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ListBeersResponse?> ListAsync(Guid? breweryId = null, string? searchTerm = null)
    {
        try
        {
            string url;
            if (breweryId.HasValue)
            {
                url = $"/api/v1/breweries/{breweryId.Value}/beers";
            }
            else
            {
                url = "/api/v1/beers";
                if (!string.IsNullOrEmpty(searchTerm))
                {
                    url += $"?searchTerm={Uri.EscapeDataString(searchTerm)}";
                }
            }
            return await _httpClient.GetFromJsonAsync<ListBeersResponse>(url);
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list beers: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> GetAsync(Guid id)
    {
        try
        {
            return await _httpClient.GetFromJsonAsync<BeerDto>($"/api/v1/beers/{id}");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to get beer {id}: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> CreateAsync(CreateBeerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("/api/v1/beers", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BeerDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to create beer: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> UpdateAsync(Guid id, UpdateBeerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"/api/v1/beers/{id}", request);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BeerDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to update beer {id}: {ex.Message}", ex);
        }
    }

    public async Task<BeerDto?> DeactivateAsync(Guid id)
    {
        try
        {
            var response = await _httpClient.PostAsync($"/api/v1/beers/{id}/deactivate", null);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<BeerDto>();
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to deactivate beer {id}: {ex.Message}", ex);
        }
    }
}

public interface IBeerStylesApiClient
{
    Task<ListBeerStylesResponse?> ListAsync();
}

public class BeerStylesApiClient(HttpClient httpClient) : IBeerStylesApiClient
{
    public async Task<ListBeerStylesResponse?> ListAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ListBeerStylesResponse>("/api/v1/beer-styles");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list beer styles: {ex.Message}", ex);
        }
    }
}

public interface IBeerTypesApiClient
{
    Task<ListBeerTypesResponse?> ListAsync();
}

public class BeerTypesApiClient(HttpClient httpClient) : IBeerTypesApiClient
{
    public async Task<ListBeerTypesResponse?> ListAsync()
    {
        try
        {
            return await httpClient.GetFromJsonAsync<ListBeerTypesResponse>("/api/v1/beer-types");
        }
        catch (Exception ex)
        {
            throw new HttpRequestException($"Failed to list beer types: {ex.Message}", ex);
        }
    }
}
