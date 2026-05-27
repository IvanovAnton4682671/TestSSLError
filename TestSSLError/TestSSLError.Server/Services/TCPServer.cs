namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за работу TCP-сервера
/// </summary>
internal class TCPServer : BackgroundService
{
    private readonly ServerSettings _serverSettings;
    private readonly ILogger<TCPServer> _logger;
    private readonly SSLErrorService _sslErrorService;
    private readonly List<Task> _listenerTasks = [];

    public TCPServer(IOptions<ServerSettings> serverSettings, ILogger<TCPServer> logger, SSLErrorService sslErrorService)
    {
        _serverSettings = serverSettings.Value;
        _logger = logger;
        _sslErrorService = sslErrorService;
    }

    /// <summary>
    /// Основной цикл обработки входящих соединений
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (EndpointsSettings endpoint in _serverSettings.EndpointsSettings)
        {
            TcpListener listener = new TcpListener(IPAddress.Any, endpoint.Port);
            listener.Start();
            _logger.LogInformation("Слушатель запущен: Port={Port}, WorkingMode={WorkingMode}", endpoint.Port, endpoint.WorkingMode);

            // Запускаем асинхронную задачу для этого порта
            Task task = ListenAsync(listener, endpoint.WorkingMode, cancellationToken);
            _listenerTasks.Add(task);
        }

        // Ждём завершения всех задач
        await Task.WhenAll(_listenerTasks);
    }

    private async Task ListenAsync(TcpListener listener, WorkingModes workingMode, CancellationToken cancellationToken)
    {
        try
        {
            while (cancellationToken.IsCancellationRequested is false)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = await listener.AcceptTcpClientAsync(cancellationToken);

                    _logger.LogInformation("Подключение: {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);

                    //Обрабатываем соединение без ожидания
                    _ = Task.Run(() => _sslErrorService.HandleAsync(tcpClient, workingMode, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Слушатель остановлен по CancellationToken: Port={Port}", ((IPEndPoint)listener.LocalEndpoint).Port);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в слушателе: Port={Port}", ((IPEndPoint)listener.LocalEndpoint).Port);
        }
        finally
        {
            listener.Stop();
            _logger.LogInformation("Слушатель остановлен: Port={Port}", ((IPEndPoint)listener.LocalEndpoint).Port);
        }
    }
}