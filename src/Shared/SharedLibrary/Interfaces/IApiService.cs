namespace SharedLibrary.Interfaces;

public interface IApiService<TDetailResponse> : IApiService
{
    public Task<TDetailResponse?> GetAsync(Guid id, CancellationToken cancellationToken=default);
}

public interface IApiService<TRequest, TDetailResponse> : IApiService<TDetailResponse>
{
    public Task<TDetailResponse> CreateAsync(TRequest request, CancellationToken cancellationToken=default);
    public Task<TDetailResponse> UpdateAsync(Guid id, TRequest request, CancellationToken cancellationToken=default);
    public Task<TDetailResponse> UpdateAsync(TRequest request, string url, CancellationToken cancellationToken = default);
    public Task DeleteAsync(Guid id, CancellationToken cancellationToken=default);
}

public interface IApiService<TRequest, TDetailResponse, TListResponse> : IApiService<TRequest, TDetailResponse>
{
    public Task<TListResponse> GetAllAsync(IApiSpecification<TRequest> specification, CancellationToken cancellationToken=default);
}

public interface IApiService
{
}


