namespace TestSSLError.Proxy.Services;

/// <summary>
/// Отвечает за запуск и работу прокси
/// </summary>
internal class TCPProxyService : BackgroundService
{
    private readonly ILogger<TCPProxyService> _logger;
    private readonly ProxySettings _proxySettings;
    private readonly List<Task> _listenerTasks = [];

    public TCPProxyService(ILogger<TCPProxyService> logger, IOptions<ProxySettings> proxySettings)
    {
        _logger = logger;
        _proxySettings = proxySettings.Value;
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
                mappingPorts.ListenPort, _proxySettings.TargetHost, mappingPorts.TargetPort
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
                    _ = HandleConnection(tcpClient, mappingPorts, cancellationToken);
                }
                catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
                {
                    _logger.LogInformation("Отмена соединения по токену: {ListenPort} -> {TargetHost}:{TargetPort}",
                        mappingPorts.ListenPort, _proxySettings.TargetHost, mappingPorts.TargetPort
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

    /// <summary>
    /// Обработка соединения между клиентом и сервером
    /// </summary>
    /// <param name="tcpClient">Клиент, с которым уже установлено соединение</param>
    /// <param name="mappingPorts">Пара портов для данного соединения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleConnection(TcpClient tcpClient, MappingPorts mappingPorts, CancellationToken cancellationToken)
    {
        try
        {
            using TcpClient target = new TcpClient();
            await target.ConnectAsync(_proxySettings.TargetHost, mappingPorts.TargetPort, cancellationToken);

            using NetworkStream clientStream = tcpClient.GetStream();
            using NetworkStream targetStream = target.GetStream();

            Task clientTask = clientStream.CopyToAsync(targetStream, cancellationToken);
            Task targetTask = targetStream.CopyToAsync(clientStream, cancellationToken);

            await Task.WhenAny(clientTask, targetTask);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проксировании соединения: {ListenPort} -> {TargetHost}:{TargetPort}",
                mappingPorts.ListenPort, _proxySettings.TargetHost, mappingPorts.TargetPort
            );
        }
        finally
        {
            tcpClient.Close();
        }
    }
}