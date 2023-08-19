using Microsoft.AspNetCore.HttpLogging;
using Microsoft.EntityFrameworkCore;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using SampleDotNetOTEL.BusinessService.Controllers;

var builder = WebApplication.CreateBuilder(args);

builder.Logging
    .ClearProviders()
    .AddOpenTelemetry(options =>
    {
        options.IncludeFormattedMessage = true;
        options.IncludeScopes = true;

        var resBuilder = ResourceBuilder.CreateDefault();
        var serviceName = builder.Configuration["ServiceName"]!;
        resBuilder.AddService(serviceName);
        options.SetResourceBuilder(resBuilder);

        options.AddOtlpExporter();
    });

builder.Services.AddControllers();

builder.Services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.All);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(b =>
    {
        b.AddService(builder.Configuration["ServiceName"]!);
    })
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = ctx => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddEntityFrameworkCoreInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = (_, ctx) => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation()
        .AddPrometheusExporter())
    .StartWithHost();

// Configure logging
// builder.Logging.AddOpenTelemetry(builder =>
// {
//     builder.IncludeFormattedMessage = true;
//     builder.IncludeScopes = true;
//     builder.ParseStateValues = true;
//     builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
// });

var app = builder.Build();

// app.MapGet("/test", (ILogger<Program> logger) =>
// {
//     logger.LogInformation("some extra logging from program.cs");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");
//     logger.LogInformation("********************************");

//     return Results.Ok("Done");
// });

app.UseOpenTelemetryPrometheusScrapingEndpoint();
app.UseHttpLogging();
app.UseDeveloperExceptionPage();
app.MapControllers();


var appResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("dotnet-web-simple", "1.0");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(appResourceBuilder);
        options.AddOtlpExporter(option =>
        {
            option.Endpoint = new Uri("http://otel-collector:4317");
        });
    });
});

var logger = loggerFactory.CreateLogger<Program>();

logger.LogDebug("This is a debug message from dotnet-web-simple", LogLevel.Debug);
logger.LogInformation("Information messages from dotnet-web-simple are used to provide contextual information", LogLevel.Information);
logger.LogError(new Exception("Application exception"), "dotnet-web-simple ==> These are usually accompanied by an exception");

app.Run();
