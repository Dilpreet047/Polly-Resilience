using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/demo")]
public class DemoController : ControllerBase
{
    private readonly ExternalApiService _service;

    public DemoController(ExternalApiService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> Get([FromHeader] string serviceURL)
    {
        var result = await _service.GetDataAsync(serviceURL);

        if (!result.Success)
            return StatusCode(500, result);

        return Ok(result);
    }
}