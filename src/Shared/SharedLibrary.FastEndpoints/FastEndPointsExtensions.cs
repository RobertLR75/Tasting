using FastEndpoints;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Scalar.AspNetCore;

namespace SharedLibrary.FastEndpoints;


public static class FastEndPointsExtensions
{
    
    
    public static void ConfigureFastEndPoints(this IHostApplicationBuilder builder)
    {
        builder.Services.AddOpenApi();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddFastEndpoints();
    }

    public static void UseEndpoints(this WebApplication app)
    {
        app.UseFastEndpoints();
        app.UseHttpsRedirection();
        
        // app.UseExceptionHandler(errorApp =>
        // {
        //     errorApp.Run(async context =>
        //     {
        //         var exceptionHandlerFeature = 
        //             context.Features.Get<IExceptionHandlerFeature>();
        //
        //         var exception = exceptionHandlerFeature?.Error;
        //
        //         var problem = new ProblemDetails
        //         {
        //             Title = "En uventet feil oppstod",
        //             Detail = exception?.Message,
        //             Status = StatusCodes.Status500InternalServerError,
        //             Instance = context.Request.Path
        //         };
        //
        //         context.Response.StatusCode = problem.Status.Value;
        //         context.Response.ContentType = "application/problem+json";
        //
        //         await context.Response.WriteAsJsonAsync(problem);
        //     });
        // });

        
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
            app.MapScalarApiReference();
            
        }
    }
}