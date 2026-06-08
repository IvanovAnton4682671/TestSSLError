namespace TestSSLError.Proxy.Services;

internal class TCPProxy : BackgroundService
{
    private readonly ILogger<TCPProxy> _logger;
    private readonly ProxySettings _settings;
    private readonly TCPHandler _handler;

    public TCPProxy(ILogger<TCPProxy> logger, IOptions<ProxySettings> settings, TCPHandler handler)
    {
        _logger = logger;
        _settings = settings.Value;
        _handler = handler;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        var listener = new TcpListener(IPAddress.Any, _settings.ListenPort);
        listener.Start();
        _logger.LogInformation("Прокси запущен на порту {ListenPort}, перенаправляет на {TargetHost}:{TargetPort}",
            _settings.ListenPort, _settings.TargetHost, _settings.TargetPort);

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await listener.AcceptTcpClientAsync(cancellationToken);
                _logger.LogInformation("Новое подключение от {EndPoint}", client.Client.RemoteEndPoint);
                _ = Task.Run(() => _handler.HandleClientAsync(client, cancellationToken), cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Прокси остановлен");
        }
        finally
        {
            listener.Stop();
        }
    }
}