using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics.Metrics;
using System.Diagnostics;
using OpenTelemetry.Resources;

namespace SampleDotNetOTEL.BusinessService.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{

    private ILoggerFactory _Factory;
    private ILogger _Logger;

    //set by dependency injection
    public HelloController(ILoggerFactory factory, ILogger logger)
    {
        _Factory = factory;
        _Logger = logger;
    }

    [HttpGet]
    public IActionResult Get()
    {
        var loggerFromDI = _Factory.CreateLogger("Hello");            
        _Logger.LogDebug("From direct dependency injection");
        loggerFromDI.LogDebug("From dependency injection factory");

        _Logger.LogInformation("Some extra logging");
        _Logger.LogInformation("********INFO************************");
        _Logger.LogInformation("********************************");
        _Logger.LogInformation("********************************");
        _Logger.LogInformation("********************************");
        loggerFromDI.LogDebug("*********DEBUG***********************");
        
        return Ok("Hello World");
    }
    
    [HttpPost]
    public IActionResult Post([FromBody] HelloRequest request)
    {
        return Ok($"Hello {request.Name}");
    }
    
    public sealed class HelloRequest
    {
        [Required]
        [MinLength(2)]
        public string? Name { get; set; }
    }
}