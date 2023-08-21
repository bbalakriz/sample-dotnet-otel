using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using SampleDotNetOTEL.ProxyService.ExternalServices;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// builder.Services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.All);

builder.Services.AddHttpClient<BusinessServiceClient>(c =>
{
    var urls = builder.Configuration["BusinessServiceBaseUrl"]!.Split(';', StringSplitOptions.RemoveEmptyEntries);
    c.BaseAddress = new Uri(urls[Random.Shared.Next(urls.Length)]);
});

// Otel
// builder.Services.AddOpenTelemetry()
//     .ConfigureResource(b =>
//     {
//         b.AddService(builder.Configuration["ServiceName"]!);
//     })
//     .WithTracing(b => b
//         .AddAspNetCoreInstrumentation(o =>
//         {
//             o.Filter = ctx => ctx.Request.Path != "/metrics";
//         })
//         .AddHttpClientInstrumentation()
//         .AddOtlpExporter())
//     .WithMetrics(b => b
//         .AddAspNetCoreInstrumentation(o =>
//         {
//             o.Filter = (_, ctx) => ctx.Request.Path != "/metrics";
//         })
//         .AddHttpClientInstrumentation()
//         .AddRuntimeInstrumentation()
//         .AddProcessInstrumentation());

// builder.Logging
//     .ClearProviders()
//     .AddOpenTelemetry(options =>
//     {
//         options.IncludeFormattedMessage = true;
//         options.IncludeScopes = true;

//         var resBuilder = ResourceBuilder.CreateDefault();
//         var serviceName = builder.Configuration["ServiceName"]!;
//         resBuilder.AddService(serviceName);
//         options.SetResourceBuilder(resBuilder);

//         options.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
//     });

builder.Logging.AddOpenTelemetry(builder =>
{
    builder.IncludeFormattedMessage = true;
    builder.IncludeScopes = true;
    builder.ParseStateValues = true;
    builder.AddOtlpExporter(options => options.Endpoint = new Uri("http://otel-collector:4317"));
});

var app = builder.Build();

app.Logger.LogInformation("********************************************************");
app.Logger.LogInformation("Starting the app ***************************************");
app.Logger.LogInformation("********************************************************");

var httpClient = new HttpClient();
var activitySource = new ActivitySource("MyApplicationActivitySource");
var meter = new Meter("MyApplicationMetrics");
var requestCounter = meter.CreateCounter<int>("compute_requests");

app.MapGet("/fire", async (ILogger<Program> logger) =>
{
    requestCounter.Add(1);

    using (var activity = activitySource.StartActivity("Get data"))
    {
        activity?.AddTag("sample", "value");

        var str1 = await httpClient.GetStringAsync("https://example.com");
        var str2 = await httpClient.GetStringAsync("https://www.meziantou.net");

        logger.LogInformation("********************************************************");
        logger.LogInformation("Response1 length: {Length}", str1.Length);
        logger.LogInformation("Response2 length: {Length}", str2.Length);
        logger.LogInformation("********************************************************");

    }

    return Results.Ok();
});

// app.UseHttpLogging();
// app.UseDeveloperExceptionPage();
app.MapControllers();
app.Run();