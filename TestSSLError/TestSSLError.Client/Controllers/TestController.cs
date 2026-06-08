namespace TestSSLError.Client.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly ILogger<TestController> _logger;
    private readonly ClientSettings _settings;
    private readonly IHttpClientFactory _httpClientFactory;

    public TestController(ILogger<TestController> logger, IOptions<ClientSettings> settings, IHttpClientFactory httpClientFactory)
    {
        _logger = logger;
        _settings = settings.Value;
        _httpClientFactory = httpClientFactory;
    }

    /// <summary>
    /// Отправить запрос через прокси с указанием сценария (заголовок X-Scenario)
    /// </summary>
    [HttpGet("request")]
    public async Task<ActionResult> SendRequest(
        [FromQuery] ScenarioMode scenario,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ProxyClient");
        using var request = new HttpRequestMessage(HttpMethod.Get, "/");
        request.Headers.TryAddWithoutValidation("X-Scenario", scenario.ToString());

        _logger.LogInformation("Отправка запроса со сценарием: {Scenario}", scenario);

        try
        {
            using var response = await client.SendAsync(request, cancellationToken);
            string body = await response.Content.ReadAsStringAsync(cancellationToken);

            return Ok(new
            {
                Scenario = scenario.ToString(),
                StatusCode = (int)response.StatusCode,
                Reason = response.ReasonPhrase,
                Body = body
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при сценарии {Scenario}", scenario);
            return StatusCode(500, new
            {
                Scenario = scenario.ToString(),
                Error = ex.Message,
                Type = ex.GetType().FullName
            });
        }
    }

    /// <summary>
    /// Стресс‑тест: несколько запросов подряд с одним сценарием
    /// </summary>
    [HttpGet("stress")]
    public async Task<ActionResult> StressTest(
        [FromQuery] ScenarioMode scenario,
        [FromQuery] int count = 10,
        [FromQuery] int delayMs = 500,
        CancellationToken cancellationToken = default)
    {
        var client = _httpClientFactory.CreateClient("ProxyClient");
        var results = new List<object>();

        for (int i = 0; i < count; i++)
        {
            using var request = new HttpRequestMessage(HttpMethod.Get, "/");
            request.Headers.TryAddWithoutValidation("X-Scenario", scenario.ToString());

            try
            {
                using var response = await client.SendAsync(request, cancellationToken);
                results.Add(new { Request = i + 1, Success = true, Status = (int)response.StatusCode });
            }
            catch (Exception ex)
            {
                results.Add(new { Request = i + 1, Success = false, Error = ex.Message });
            }

            if (delayMs > 0 && i < count - 1)
            {
                await Task.Delay(delayMs, cancellationToken);
            }
        }

        return Ok(new { Scenario = scenario.ToString(), Total = count, Results = results });
    }
}