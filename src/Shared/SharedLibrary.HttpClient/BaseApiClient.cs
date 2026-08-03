using System.Net.Http.Json;
using SharedLibrary.Interfaces;

namespace SharedLibrary.HttpClient;

public abstract class BaseApiClient<TRequest, TDetailResponse, TListResponse>(IHttpClientFactory httpClientFactory) : BaseApiClient<TRequest, TDetailResponse>(httpClientFactory), IApiService<TRequest, TDetailResponse, List<TListResponse>>
{
    public virtual async Task<List<TListResponse>> GetAllAsync(IApiSpecification<TRequest>? specification, CancellationToken cancellationToken = default)
    {
        var url = $"/{Name}";
        
        if (specification != null)
        {
            var queryString = SpecificationQueryStringBuilder.BuildQueryString(specification);
            if (!string.IsNullOrWhiteSpace(queryString))
                url += $"?{queryString}";
        }
        
        var result = await HttpClient.GetFromJsonAsync<List<TListResponse>>(url, cancellationToken);
        return result ?? [];
    }
}

public abstract class BaseApiClient<TRequest, TDetailResponse> : IApiService<TRequest, TDetailResponse>
{
    protected abstract string Name { get; }

    protected readonly System.Net.Http.HttpClient HttpClient;

    protected BaseApiClient(IHttpClientFactory httpClientFactory)
    {
        HttpClient = httpClientFactory.CreateClient(Name);
    }

    
    public virtual async Task<TDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken)
    {
        var url = $"/{Name}/{id}";
        var result = await HttpClient.GetFromJsonAsync<TDetailResponse>(url, cancellationToken);
        return result;
    }

    public virtual async Task<TDetailResponse> CreateAsync(TRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"/{Name}";

        var response = await HttpClient.PostAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TDetailResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException($"The '{Name}' create response did not contain a {typeof(TDetailResponse).Name} payload.");
    }

    public virtual async Task<TDetailResponse> UpdateAsync(Guid id, TRequest request, CancellationToken cancellationToken = default)
    {
        var url = $"/{Name}/{id}";

        var response = await HttpClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TDetailResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException($"The '{Name}' update response did not contain a {typeof(TDetailResponse).Name} payload.");
    }
    
    
    
    public virtual async Task<TDetailResponse> UpdateAsync(TRequest request, string url, CancellationToken cancellationToken = default)
    {
        var response = await HttpClient.PutAsJsonAsync(url, request, cancellationToken);
        response.EnsureSuccessStatusCode();

        var result = await response.Content.ReadFromJsonAsync<TDetailResponse>(cancellationToken: cancellationToken);
        return result ?? throw new InvalidOperationException($"The '{Name}' update response did not contain a {typeof(TDetailResponse).Name} payload.");
    }

    public virtual async Task DeleteAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var url = $"/{Name}/{id}";
        var response = await HttpClient.DeleteAsync(url, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}