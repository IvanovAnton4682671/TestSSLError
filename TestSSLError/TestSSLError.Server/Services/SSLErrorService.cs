namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за обработку соединения
/// </summary>
internal class SSLErrorService
{
    private readonly ILogger<SSLErrorService> _logger;

    public SSLErrorService(ILogger<SSLErrorService> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Обработка соединения в зависимости от выбранного режима работы сервера
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="workingMode">Режим работы сервера</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task HandleAsync(TcpClient tcpClient, WorkingModes workingMode, CancellationToken cancellationToken)
    {
        try
        {
            switch (workingMode)
            {
                case WorkingModes.Normal:
                    _logger.LogInformation("Выбран нормальный режим работы сервера");
                    tcpClient.Close();
                    break;

                case WorkingModes.TimeoutOnConnect:
                    await HandleTimeoutOnConnectAsync(cancellationToken);
                    break;

                case WorkingModes.EOFAfterClientHello:
                    await HandleEOFAfterClientHelloAsync(tcpClient, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка при обработке соединения");
        }
    }

    /// <summary>
    /// Обработка режима таймаута при запросе соединения
    /// </summary>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleTimeoutOnConnectAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Выбран режим таймаута");

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Время симуляции таймаута истекло по токену");
        }
    }

    /// <summary>
    /// Обработка режима закрытия соединения после получения ClientHello от клиента
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleEOFAfterClientHelloAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Выбран режим EOF");

        var stream = tcpClient.GetStream();
        var buffer = new byte[1024];

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        _logger.LogInformation("Получено {Bytes} байт, закрытие соединения", bytesRead);

        stream.Close();
        tcpClient.Close();
    }
}