using System.Text.Json;
using Microsoft.JSInterop;
using Tasting.Admin.Features.Auth.Models;

namespace Tasting.Admin.Features.Auth.Services;

public sealed class SessionStorageAdminSessionStore(IJSRuntime jsRuntime) : IAdminSessionStore
{
    private const string StorageKey = "tasting.admin.session";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task<StoredAdminSession?> LoadAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var json = await jsRuntime.InvokeAsync<string?>("sessionStorage.getItem", cancellationToken, StorageKey);
            return string.IsNullOrWhiteSpace(json)
                ? null
                : JsonSerializer.Deserialize<StoredAdminSession>(json, JsonOptions);
        }
        catch (InvalidOperationException)
        {
            throw new AdminSessionStoreUnavailableException();
        }
        catch (JSException)
        {
            throw new AdminSessionStoreUnavailableException();
        }
        catch (JsonException)
        {
            await ClearAsync(cancellationToken);
            return null;
        }
    }

    public async Task SaveAsync(StoredAdminSession session, CancellationToken cancellationToken = default)
    {
        var json = JsonSerializer.Serialize(session, JsonOptions);
        await jsRuntime.InvokeVoidAsync("sessionStorage.setItem", cancellationToken, StorageKey, json);
    }

    public async Task ClearAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            await jsRuntime.InvokeVoidAsync("sessionStorage.removeItem", cancellationToken, StorageKey);
        }
        catch (InvalidOperationException)
        {
        }
        catch (JSException)
        {
        }
    }
}

public sealed class AdminSessionStoreUnavailableException : Exception;
