namespace TestSSLError.Api;

[ApiController]
[Route("test")]
public class TestController : ControllerBase
{
    private readonly SSLErrorTestService _testService;

    public TestController(SSLErrorTestService testService)
    {
        _testService = testService;
    }

    [HttpGet("{scenario}")]
    public async Task<ActionResult<TestResult>> RunTest(TestModes testMode)
    {
        var result = await _testService.RunTestAsync(testMode);
        return Ok(result);
    }
}