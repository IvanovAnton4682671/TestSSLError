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
        // Ищем эндпоинт по порту
        EndpointsSettings? endpoint = _clientSettings.EndpointsSettings.FirstOrDefault(e => e.Port == port);
        if (endpoint == null)
        {
            return BadRequest($"Port={port} не найден в конфигурации клиента. Доступные порты: " +
                $"{string.Join(", ", _clientSettings.EndpointsSettings.Select(e => e.Port))}"
            );
        }

        string requestUrl = $"{endpoint.BaseUrl.TrimEnd('/')}/";

        if (Uri.TryCreate(requestUrl, UriKind.Absolute, out var uri) is false)
        {
            return BadRequest($"Некорректный URL: {requestUrl}");
        }

        HttpClient client = _httpClientFactory.CreateClient("TargetClient");

        try
        {
            using HttpResponseMessage response = await client.GetAsync(uri, cancellationToken);

            return Ok(new
            {
                Port = port,
                Url = requestUrl,
                response.StatusCode,
                Reason = response.ReasonPhrase
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при отправке запроса по Url={Url}", requestUrl);

            return StatusCode(500, new
            {
                Port = port,
                Url = requestUrl,
                Error = ex.Message,
                Type = ex.GetType().FullName
            });
        }
    }
}