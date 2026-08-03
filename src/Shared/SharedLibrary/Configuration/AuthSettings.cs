namespace SharedLibrary.Configuration;

public abstract class AuthSettings
{
    public string? Name { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? Scopes { get; set; }
}