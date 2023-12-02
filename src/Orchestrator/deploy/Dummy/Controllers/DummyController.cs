using Microsoft.AspNetCore.Mvc;

namespace Dummy.Controllers;

[Route("api/[controller]")]
public class DummyController : ControllerBase
{
    private readonly ILogger<DummyController> _logger;

    public DummyController(ILogger<DummyController> logger)
    {
        _logger = logger;
    }

    [HttpGet("foo/{arg}")]
    public IActionResult Foo(string arg)
    {
        _logger.LogInformation($"================ Foo {arg} =================");
        return Ok();
    }
}
