namespace SharedLibrary.HttpClient;

public class LoggingHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
    {
        Console.WriteLine("REQUEST:");
        Console.WriteLine(request);

        if (request.Content != null)
            Console.WriteLine(await request.Content.ReadAsStringAsync(cancellationToken));

        var response = await base.SendAsync(request, cancellationToken);

        Console.WriteLine("RESPONSE:");
        Console.WriteLine(response);

        if (response.Content != null)
            Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));

        return response;
    }
}