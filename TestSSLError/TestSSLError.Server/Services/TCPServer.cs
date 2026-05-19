namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за работу TCP-сервера
/// </summary>
internal class TCPServer : BackgroundService
{
    private readonly ServerSettings _serverSettings;
    private readonly ILogger<TCPServer> _logger;
    private readonly SSLErrorService _sslErrorService;
    private TcpListener? _tcpListener;

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
        _tcpListener = new TcpListener(IPAddress.Any, _serverSettings.Port);
        _tcpListener.Start();
        _logger.LogInformation("Сервер запущен: Port={Port}, WorkingMode={WorkingMode}", _serverSettings.Port, _serverSettings.WorkingMode);

        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                TcpClient tcpClient = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                _logger.LogInformation("Соединение установлено: RemoteEndPoint={RemoteEndPoint}", tcpClient.Client.RemoteEndPoint);

                _ = Task.Run(() => _sslErrorService.HandleAsync(tcpClient, _serverSettings.WorkingMode, cancellationToken), cancellationToken);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Произошла ошибка при работе сервера");
            }
        }
    }

    /// <summary>
    /// Освобождение ресурсов
    /// </summary>
    public override void Dispose()
    {
        _tcpListener?.Stop();
        base.Dispose();
    }
}