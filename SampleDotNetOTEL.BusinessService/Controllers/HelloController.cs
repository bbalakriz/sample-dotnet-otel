using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;

namespace SampleDotNetOTEL.BusinessService.Controllers;

[ApiController]
[Route("[controller]")]
public class HelloController : ControllerBase
{
    private readonly ILogger logger;

    public HelloController(ILoggerFactory logFactory) 
    {
        logger = logFactory.CreateLogger<HelloController>();
    }

    [HttpGet]
    public IActionResult Get()
    {
        logger.LogInformation("some extra logging");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        logger.LogInformation("********************************");
        
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