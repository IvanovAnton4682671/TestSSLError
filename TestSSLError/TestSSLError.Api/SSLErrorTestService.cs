namespace TestSSLError.Api;

public class SSLErrorTestService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SSLErrorTestService> _logger;

    public SSLErrorTestService(IOptions<ClientSettings> settings, ILogger<SSLErrorTestService> logger)
    {
        _logger = logger;
        var handler = new HttpClientHandler
        {
            // Отключаем проверку сертификата — тестируем не его
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };

        _httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri(settings.Value.TargetServerBaseUrl),
            Timeout = TimeSpan.FromSeconds(settings.Value.RequestTimeoutSeconds)
        };
    }

    public async Task<TestResult> RunTestAsync(TestModes testMode)
    {
        var result = new TestResult
        {
            TestMode = testMode.ToString()
        };

        try
        {
            _logger.LogInformation("Running test: {TestMode}", testMode);
            await _httpClient.GetAsync("/"); // любой путь, нам важен сам факт подключения
                                             // Если дошли сюда — ошибки не было, но мы ожидали исключение
            result.Success = false;
            result.Message = "Expected an exception, but request succeeded.";
        }
        catch (Exception ex)
        {
            //result.ActualException = ex.GetType().Name;
            //result.Message = ex.InnerException?.Message ?? ex.Message;

            //// Сопоставляем ожидаемое исключение со сценарием
            //result.ExpectedException = testMode switch
            //{
            //    TestModes.Normal => "OK",
            //    TestModes.TimeoutOnConnect => "TaskCanceledException (TimeoutException)",
            //    TestModes.EOFAfterClientHello => "HttpRequestException (Inner: IOException)",
            //    _ => "Unknown"
            //};

            //result.Success = testMode switch
            //{
            //    TestModes.TimeoutOnConnect =>
            //        ex is TaskCanceledException &&
            //        (ex.InnerException is TimeoutException ||
            //         ex.Message.Contains("canceled")),

            //    TestModes.EOFAfterClientHello =>
            //        ex is HttpRequestException &&
            //        ex.InnerException is IOException &&
            //        (ex.InnerException.Message.Contains("EOF") ||
            //         ex.InnerException.Message.Contains("0 bytes")),

            //    _ => false
            //};

            //_logger.LogInformation("Test {TestMode}: {Result}", testMode, result.Success ? "PASS" : "FAIL");

            _logger.LogError(ex, "Test {TestMode} failed with exception", testMode);
        }

        return result;
    }
}