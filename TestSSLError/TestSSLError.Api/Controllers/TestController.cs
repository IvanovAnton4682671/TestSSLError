namespace TestSSLError.Client.Controllers;

/// <summary>
/// Основной контроллер для тестового GET-запроса к целевому TCP-серверу
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly ClientSettings _clientSettings;
    private readonly ILogger<TestController> _logger;

    public TestController(IHttpClientFactory httpClientFactory, IOptions<ClientSettings> clientSettings, ILogger<TestController> logger)
    {
        _httpClientFactory = httpClientFactory;
        _clientSettings = clientSettings.Value;
        _logger = logger;
    }

    /// <summary>
    /// Тестовый GET-запрос к целевому TCP-серверу на указанный порт
    /// </summary>
    /// <param name="port">Порт целевого сервера</param>
    /// <param name="cancellationToken">Токен отмены</param>
    [HttpGet("request")]
    public async Task<ActionResult> SendRequest([FromQuery, Required, Range(1, 65535)] int port, CancellationToken cancellationToken)
    {
        string requestUrl = $"{_clientSettings.TargetServerBaseUrl}:{port}/";

        if (Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri) is false)
        {
            return BadRequest($"Некорректный URL: {requestUrl}");
        }

        HttpClient client = _httpClientFactory.CreateClient("TargetServerClient");

        try
        {
            using HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);

            return Ok(new
            {
                Port = port,
                response.StatusCode,
                Reason = response.ReasonPhrase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса к целевому серверу: Port={Port}", port);

            return StatusCode(500, new
            {
                Port = port,
                Error = ex.Message,
                Type = ex.GetType().FullName
            });
        }
    }
}