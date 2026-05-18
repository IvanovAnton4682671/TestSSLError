namespace TestSSLError.Server;

public class SSLErrorService
{
    private readonly ILogger<SSLErrorService> _logger;

    public SSLErrorService(ILogger<SSLErrorService> logger)
    {
        _logger = logger;
    }

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
                    await HandleTimeoutOnConnect(cancellationToken);
                    break;

                case WorkingModes.EOFAfterClientHello:
                    await HandleEOFAfterClientHello(tcpClient, cancellationToken);
                    break;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка при обработке соединения");
        }
    }

    private async Task HandleTimeoutOnConnect(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Выбран режим таймаута при запросе соединения");

        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            _logger.LogInformation("Время симуляции таймауьа истекло по токену");
        }
    }

    private async Task HandleEOFAfterClientHello(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Выбран режим EOF");

        var stream = tcpClient.GetStream();
        var buffer = new byte[4096];

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        _logger.LogInformation("Получено {Bytes} байт, закрытие соединения", bytesRead);

        stream.Close();
        tcpClient.Close();
    }
}