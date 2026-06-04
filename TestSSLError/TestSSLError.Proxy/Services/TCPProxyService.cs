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
                    _ = HandleConnectionAsync(tcpClient, mappingPorts, cancellationToken);
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

    /// <summary>
    /// Обработка соединения между клиентом и сервером
    /// </summary>
    /// <param name="tcpClient">Клиент, с которым уже установлено соединение</param>
    /// <param name="mappingPorts">Пара портов для данного соединения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleConnectionAsync(TcpClient tcpClient, MappingPorts mappingPorts, CancellationToken cancellationToken)
    {
        try
        {
            switch (mappingPorts.WorkingMode)
            {
                case WorkingModes.Normal:
                    await ProxyToServerAsync(tcpClient, cancellationToken);
                    break;

                case WorkingModes.EOFAfterClientHello:
                    await HandleEOFAfterClientHelloAsync(tcpClient, cancellationToken);
                    break;

                case WorkingModes.TimeoutOnConnect:
                    await HandleTimeoutOnConnectAsync(cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при проксировании соединения: {ListenPort} -> {TargetHost}:{TargetPort}",
                mappingPorts.ListenPort, _proxySettings.TargetHost, _proxySettings.TargetPort
            );
        }
        finally
        {
            tcpClient.Close();
        }
    }

    /// <summary>
    /// Обработка соединения между клиентом и сервером
    /// </summary>
    /// <param name="tcpClient">Клиент, с которым уже установлено соединение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task ProxyToServerAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Режим нормального проксирования");

        using TcpClient target = new TcpClient();
        await target.ConnectAsync(_proxySettings.TargetHost, _proxySettings.TargetPort, cancellationToken);

        using NetworkStream clientStream = tcpClient.GetStream();
        using NetworkStream targetStream = target.GetStream();

        Task clientTask = clientStream.CopyToAsync(targetStream, cancellationToken);
        Task targetTask = targetStream.CopyToAsync(clientStream, cancellationToken);

        await Task.WhenAny(clientTask, targetTask);
    }

    /// <summary>
    /// Обработка режима закрытия соединения: прокси закрывает соединение сразу после получения ClientHello от клиента,
    /// из-за чего TLS handshake не завершается и клиент получает ошибку EOF
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleEOFAfterClientHelloAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Режим EOF");

        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];

        int bytesRead = await stream.ReadAsync(buffer, cancellationToken);
        _logger.LogInformation("Получено {Bytes} байт, закрытие соединения", bytesRead);

        stream.Close();
        tcpClient.Close();
    }

    /// <summary>
    /// Обработка режима таймаута: прокси не отвечает на ClientHello от клиента,
    /// а просто "зависает" пока не истечёт время ожидания клиента
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleTimeoutOnConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Режим таймаута");

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Время симуляции таймаута истекло по токену");
        }
    }
}