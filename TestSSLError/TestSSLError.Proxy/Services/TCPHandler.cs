namespace TestSSLError.Proxy.Services;

internal class TCPHandler
{
    private readonly ILogger<TCPHandler> _logger;
    private readonly ProxySettings _settings;

    public TCPHandler(ILogger<TCPHandler> logger, IOptions<ProxySettings> settings)
    {
        _logger = logger;
        _settings = settings.Value;
    }

    public async Task HandleClientAsync(TcpClient client, CancellationToken cancellationToken)
    {
        try
        {
            var stream = client.GetStream();
            var buffer = new byte[8192];
            int totalRead = 0;
            bool headerEndFound = false;

            // Читаем до тех пор, пока не найдём \r\n\r\n или не заполним буфер
            while (totalRead < buffer.Length)
            {
                int bytesRead = await stream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken);
                if (bytesRead == 0) break; // клиент закрыл соединение
                totalRead += bytesRead;

                // Проверяем наличие конца заголовков
                for (int i = 0; i < totalRead - 3; i++)
                {
                    if (buffer[i] == '\r' && buffer[i + 1] == '\n' && buffer[i + 2] == '\r' && buffer[i + 3] == '\n')
                    {
                        headerEndFound = true;
                        break;
                    }
                }
                if (headerEndFound) break;
            }

            if (totalRead == 0)
            {
                _logger.LogWarning("Клиент закрыл соединение без передачи данных");
                return;
            }

            // Пытаемся определить режим по заголовку X-Scenario
            var mode = HeaderParser.GetScenarioFromHeader(buffer, totalRead) ?? WorkingModes.Normal;
            _logger.LogInformation("Определён режим: {Mode} (получено байт: {Bytes})", mode, totalRead);

            var context = new ProxyContext(_settings.TargetHost, _settings.TargetPort, mode, _logger)
            {
                ReadBuffer = buffer,
                ReadBufferLength = totalRead
            };

            var strategy = ProxyModeFactory.Create(mode);
            await strategy.HandleAsync(client, context, cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке клиента");
        }
        finally
        {
            client.Close();
        }
    }
}