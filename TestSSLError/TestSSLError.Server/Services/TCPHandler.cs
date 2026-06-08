namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за обработку одного клиентского соединения.
/// Поддерживает Keep-Alive, лимит запросов и idle-таймаут.
/// </summary>
internal class TCPHandler
{
    private readonly ILogger<TCPHandler> _logger;
    private readonly X509Certificate2 _serverCertificate;
    private readonly ServerSettings _settings;

    public TCPHandler(ILogger<TCPHandler> logger, X509Certificate2 serverCertificate, IOptions<ServerSettings> settings)
    {
        _logger = logger;
        _serverCertificate = serverCertificate;
        _settings = settings.Value;
    }

    /// <summary>
    /// Обработка соединения: TLS handshake, цикл приёма запросов и отправки ответов.
    /// </summary>
    public async Task HandleAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        using var _ = tcpClient;
        try
        {
            _logger.LogInformation("Начало обработки нового клиента");

            using var networkStream = tcpClient.GetStream();
            using var sslStream = new SslStream(networkStream, false);

            // TLS handshake
            await sslStream.AuthenticateAsServerAsync(
                _serverCertificate,
                clientCertificateRequired: false,
                enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13,
                checkCertificateRevocation: false
            );

            _logger.LogInformation("TLS handshake завершён успешно");

            // Цикл обработки запросов
            int requestCount = 0;
            bool keepAlive = _settings.EnableKeepAlive;
            var idleCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Сброс таймаута бездействия перед чтением следующего запроса
                if (_settings.IdleTimeoutSeconds.HasValue)
                {
                    idleCts.CancelAfter(TimeSpan.FromSeconds(_settings.IdleTimeoutSeconds.Value));
                }

                // Чтение HTTP-запроса (заголовки до \r\n\r\n)
                byte[] buffer = new byte[8192];
                int totalRead = 0;
                bool headerEndFound = false;

                try
                {
                    while (totalRead < buffer.Length)
                    {
                        int bytesRead = await sslStream.ReadAsync(
                            buffer.AsMemory(totalRead, buffer.Length - totalRead),
                            idleCts.Token
                        );

                        if (bytesRead == 0)
                        {
                            // Клиент закрыл соединение
                            _logger.LogDebug("Клиент закрыл соединение");
                            return;
                        }

                        totalRead += bytesRead;

                        // Ищем конец заголовков \r\n\r\n
                        for (int i = 0; i < totalRead - 3; i++)
                        {
                            if (buffer[i] == '\r' && buffer[i + 1] == '\n' &&
                                buffer[i + 2] == '\r' && buffer[i + 3] == '\n')
                            {
                                headerEndFound = true;
                                break;
                            }
                        }

                        if (headerEndFound)
                            break;
                    }
                }
                catch (OperationCanceledException) when (idleCts.IsCancellationRequested)
                {
                    _logger.LogWarning("Idle timeout, закрытие соединения");
                    break;
                }

                if (!headerEndFound)
                {
                    _logger.LogWarning("Не удалось прочитать корректный HTTP-запрос, закрытие соединения");
                    break;
                }

                requestCount++;
                _logger.LogInformation("Запрос #{RequestCount} получен", requestCount);

                // Определяем, хочет ли клиент закрыть соединение
                bool clientCloseRequested = CheckClientConnectionClose(buffer, totalRead);

                // Формируем ответ
                string responseBody = "OK";
                var sb = new StringBuilder();
                sb.AppendLine("HTTP/1.1 200 OK");
                sb.AppendLine("Content-Type: text/plain");
                sb.AppendLine($"Content-Length: {responseBody.Length}");

                // Решаем, закрывать соединение после этого ответа
                bool shouldClose = !keepAlive || clientCloseRequested ||
                                   (_settings.MaxRequestsPerConnection.HasValue &&
                                    requestCount >= _settings.MaxRequestsPerConnection.Value);

                if (shouldClose)
                {
                    sb.AppendLine("Connection: close");
                }
                else
                {
                    sb.AppendLine("Connection: keep-alive");
                    if (_settings.KeepAliveTimeoutSeconds.HasValue)
                    {
                        sb.AppendLine($"Keep-Alive: timeout={_settings.KeepAliveTimeoutSeconds}");
                    }
                }

                sb.AppendLine(); // пустая строка
                sb.Append(responseBody);

                byte[] responseBytes = Encoding.ASCII.GetBytes(sb.ToString());
                await sslStream.WriteAsync(responseBytes, cancellationToken);
                await sslStream.FlushAsync(cancellationToken);

                // Если нужно закрыть – выходим из цикла
                if (shouldClose)
                {
                    _logger.LogInformation("Закрытие соединения после {RequestCount} запросов", requestCount);
                    break;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при обработке соединения");
        }
        finally
        {
            tcpClient.Close();
            _logger.LogInformation("Соединение закрыто");
        }
    }

    /// <summary>
    /// Проверяет, есть ли в заголовках запроса "Connection: close"
    /// </summary>
    private static bool CheckClientConnectionClose(byte[] buffer, int length)
    {
        string headerPart = Encoding.ASCII.GetString(buffer, 0, length);
        // Ищем "Connection: close" (регистронезависимо)
        const string pattern = "connection: close";
        int idx = headerPart.IndexOf(pattern, StringComparison.OrdinalIgnoreCase);
        return idx >= 0;
    }
}