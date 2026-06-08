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

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        TcpListener tcpListener = new TcpListener(IPAddress.Any, _serverSettings.Port);
        tcpListener.Start();
        _logger.LogInformation("Слушатель запущен: Port={Port}, KeepAlive={KeepAlive}, MaxRequests={MaxRequests}, IdleTimeout={IdleTimeout}",
            _serverSettings.Port, _serverSettings.EnableKeepAlive, _serverSettings.MaxRequestsPerConnection, _serverSettings.IdleTimeoutSeconds);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                TcpClient tcpClient;
                try
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
                    _logger.LogInformation("Подключение: {RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);
                    _ = Task.Run(() => _tcpHandler.HandleAsync(tcpClient, cancellationToken), cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Слушатель остановлен по CancellationToken");
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
            tcpListener.Stop();
            _logger.LogInformation("Слушатель остановлен: Port={Port}", _serverSettings.Port);
        }
    }
}