namespace TestSSLError.Server;

public class TCPServer : BackgroundService
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

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        _tcpListener = new TcpListener(IPAddress.Any, _serverSettings.Port);
        _tcpListener.Start();
        _logger.LogInformation("Сервер запущен: Port={Port}, WorkingMode={WorkingMode}", _serverSettings.Port, _serverSettings.WorkingMode);

        while (cancellationToken.IsCancellationRequested is false)
        {
            try
            {
                TcpClient client = await _tcpListener.AcceptTcpClientAsync(cancellationToken);
                _logger.LogInformation("Соединение установлено: RemoteEndPoint={RemoteEndPoint}", client.Client.RemoteEndPoint);

                _ = Task.Run(() => _sslErrorService.HandleAsync(client, _serverSettings.WorkingMode, cancellationToken), cancellationToken);
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

    public override void Dispose()
    {
        _tcpListener?.Stop();
        base.Dispose();
    }
}