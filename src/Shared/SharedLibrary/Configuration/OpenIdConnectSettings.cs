namespace SharedLibrary.Configuration;

public class OpenIdConnectSettings : AuthSettings
{
    public Uri? Authority { get; set; }
    public string ResponseType { get; set; } = "code";
}