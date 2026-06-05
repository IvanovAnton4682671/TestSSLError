namespace TestSSLError.Proxy.Services;

/// <summary>
/// Отвечает за запуск и работу прокси
/// </summary>
internal class TCPProxy : BackgroundService
{
    private readonly ILogger<TCPProxy> _logger;
    private readonly ProxySettings _proxySettings;
    private readonly List<Task> _listenerTasks = [];
    private readonly TCPHandler _tcpHandler;

    public TCPProxy(ILogger<TCPProxy> logger, IOptions<ProxySettings> proxySettings, TCPHandler tcpHandler)
    {
        _logger = logger;
        _proxySettings = proxySettings.Value;
        _tcpHandler = tcpHandler;
    }

    /// <summary>
    /// Запуск прокси (стартуют слушатели портов)
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        foreach (MappingPorts mappingPorts in _proxySettings.MappingPorts)
        {
            TcpListener tcpListener = new TcpListener(IPAddress.Any, mappingPorts.ListenPort);
            tcpListener.Start();
            _logger.LogInformation("Прокси-слушатель запущен: {ListenPort} -> {TargetHost}:{TargetPort}",
                mappingPorts.ListenPort, _proxySettings.TargetHost, _proxySettings.TargetPort
            );

            _listenerTasks.Add(ListenAsync(tcpListener, mappingPorts, cancellationToken));
        }

        await Task.WhenAll(_listenerTasks);
    }

    /// <summary>
    /// Устанавливает соединение с клиентом и пробрасывает его на целевой сервер
    /// </summary>
    /// <param name="tcpListener">Слушатель, который устанавливает соединение с клиентом</param>
    /// <param name="mappingPorts">Пара портов для данного соединения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task ListenAsync(TcpListener tcpListener, MappingPorts mappingPorts, CancellationToken cancellationToken)
    {
        try
        {
            while (cancellationToken.IsCancellationRequested is false)
            {
                TcpClient tcpClient;

                try
                {
                    tcpClient = await tcpListener.AcceptTcpClientAsync(cancellationToken);
                    _logger.LogInformation("Установлено соединение с клиентом: ListenPort={ListenPort}", mappingPorts.ListenPort);
                    _ = _tcpHandler.HandleConnectionAsync(tcpClient, mappingPorts, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Отмена соединения по токену: {ListenPort} -> {TargetHost}:{TargetPort}",
                        mappingPorts.ListenPort, _proxySettings.TargetHost, _proxySettings.TargetPort
                    );
                    break;
                }
            }
        }
        finally
        {
            tcpListener.Stop();
        }
    }
}