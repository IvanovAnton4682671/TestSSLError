namespace TestSSLError.Proxy.Services;

internal class TCPHandler
{
    private readonly ILogger<TCPHandler> _logger;
    private readonly ProxySettings _proxySettings;

    public TCPHandler(ILogger<TCPHandler> logger, IOptions<ProxySettings> proxySettings)
    {
        _logger = logger;
        _proxySettings = proxySettings.Value;
    }

    /// <summary>
    /// Обработка соединения между клиентом и сервером
    /// </summary>
    /// <param name="tcpClient">Клиент, с которым уже установлено соединение</param>
    /// <param name="mappingPorts">Пара портов для данного соединения</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task HandleConnectionAsync(TcpClient tcpClient, MappingPorts mappingPorts, CancellationToken cancellationToken)
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