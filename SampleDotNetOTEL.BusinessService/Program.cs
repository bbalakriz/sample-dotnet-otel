using Microsoft.AspNetCore.HttpLogging;
using OpenTelemetry;
using OpenTelemetry.Logs;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using SampleDotNetOTEL.BusinessService.Controllers;

AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

// builder.Services.AddHttpLogging(o => o.LoggingFields = HttpLoggingFields.All);

builder.Services.AddOpenTelemetry()
    .ConfigureResource(b =>
    {
        b.AddService(serviceName: builder.Configuration.GetValue("ServiceName", defaultValue: "otel-test")!);
    })
    .WithTracing(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = ctx => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddOtlpExporter())
    .WithMetrics(b => b
        .AddAspNetCoreInstrumentation(o =>
        {
            o.Filter = (_, ctx) => ctx.Request.Path != "/metrics";
        })
        .AddHttpClientInstrumentation()
        .AddRuntimeInstrumentation()
        .AddProcessInstrumentation());

var appResourceBuilder = ResourceBuilder.CreateDefault()
    .AddService("sample-dontet-otel", "1.0");

using var loggerFactory = LoggerFactory.Create(builder =>
{
    builder.AddOpenTelemetry(options =>
    {
        options.SetResourceBuilder(appResourceBuilder);
        options.AddConsoleExporter();
        options.AddOtlpExporter(option =>
        {
            option.Endpoint = new Uri("http://otel-collector:4317");
        });
    });
});

var app = builder.Build();
var logger = loggerFactory.CreateLogger<Program>();

logger.LogInformation("********************************************************");
logger.LogInformation("Starting the app ***************************************");
logger.LogInformation("********************************************************");

var httpClient = new HttpClient();
var activitySource = new ActivitySource("MyApplicationActivitySource");
var meter = new Meter("MyApplicationMetrics");
var requestCounter = meter.CreateCounter<int>("compute_requests");

// app.MapControllerRoute(
//     name: "default",
//     pattern: "/");

// app.MapGet("/", async () =>
// {
//     requestCounter.Add(1);
//     using (var activity = activitySource.StartActivity("Get data"))
//     {
//         activity?.AddTag("sample", "value");

//         var str1 = await httpClient.GetStringAsync("https://example.com");
//         var str2 = await httpClient.GetStringAsync("https://www.meziantou.net");

//         logger.LogInformation("********************************************************");
//         logger.LogInformation("Response1 length: {Length}", str1.Length);
//         logger.LogInformation("Response2 length: {Length}", str2.Length);
//         logger.LogInformation("********************************************************");

//     }

//     return Results.Ok();
// });

// app.UseHttpLogging();
app.UseDeveloperExceptionPage();
// app.MapControllers();

app.Run();
