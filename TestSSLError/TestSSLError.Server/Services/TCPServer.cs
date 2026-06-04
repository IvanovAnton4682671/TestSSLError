namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за работу TCP-сервера
/// </summary>
internal class TCPServer : BackgroundService
{
    private readonly ILogger<TCPServer> _logger;
    private readonly ServerSettings _serverSettings;
    private readonly TCPHandler _tcpHandler;

    public TCPServer(ILogger<TCPServer> logger, IOptions<ServerSettings> serverSettings, TCPHandler tcpHandler)
    {
        _logger = logger;
        _serverSettings = serverSettings.Value;
        _tcpHandler = tcpHandler;
    }

    /// <summary>
    /// Основной цикл обработки входящих соединений
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        TcpListener listener = new TcpListener(IPAddress.Any, _serverSettings.Port);
        listener.Start();
        _logger.LogInformation("Слушатель запущен: Port={Port}", _serverSettings.Port);

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
                    _ = Task.Run(() => _tcpHandler.HandleAsync(tcpClient, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Слушатель остановлен по CancellationToken: Port={Port}", _serverSettings.Port);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка в слушателе: Port={Port}", _serverSettings.Port);
        }
        finally
        {
            listener.Stop();
            _logger.LogInformation("Слушатель остановлен: Port={Port}", _serverSettings.Port);
        }
    }
}