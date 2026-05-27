namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за обработку соединения
/// </summary>
internal class SSLErrorService
{
    private readonly ILogger<SSLErrorService> _logger;
    private readonly X509Certificate2 _serverCertificate;

    public SSLErrorService(ILogger<SSLErrorService> logger, X509Certificate2 serverCertificate)
    {
        _logger = logger;
        _serverCertificate = serverCertificate;
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
                    await HandleNormalAsync(tcpClient, cancellationToken);
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
    /// Ищет в данных последовательность, которая означает конец HTTP-заголовков
    /// </summary>
    /// <param name="data">Данные, в которых ищем конец заголовков</param>
    /// <returns>true если нашли последовательность, иначе false</returns>
    private static bool ContainsHeaderEnd(Span<byte> data)
    {
        // Ищем последовательность \r\n\r\n
        for (int i = 0; i < data.Length - 3; i++)
        {
            if (data[i] == '\r' && data[i + 1] == '\n' && data[i + 2] == '\r' && data[i + 3] == '\n')
            {
                return true;
            }
        }
        return false;
    }

    /// <summary>
    /// Обработка обычного режима: выполняем полный TLS handshake и отвечаем 200 OK
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="ct">Токен отмены</param>
    private async Task HandleNormalAsync(TcpClient tcpClient, CancellationToken ct)
    {
        _logger.LogInformation("Нормальный режим");

        using NetworkStream stream = tcpClient.GetStream();
        using SslStream sslStream = new SslStream(stream, false);

        // Выполняем TLS handshake (серверная сторона)
        await sslStream.AuthenticateAsServerAsync(
            _serverCertificate,
            clientCertificateRequired: false,
            enabledSslProtocols: SslProtocols.Tls12 | SslProtocols.Tls13,
            checkCertificateRevocation: false
        );

        _logger.LogInformation("TLS handshake завершён успешно");

        // Читаем HTTP-запрос (до пустой строки, завершающей заголовки)
        byte[] buffer = new byte[4096];
        int totalRead = 0;
        while (totalRead < buffer.Length)
        {
            int bytesRead = await sslStream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), ct);

            // Клиент закрыл соединение
            if (bytesRead == 0)
            {
                break;
            }

            totalRead += bytesRead;

            // Проверяем, получили ли мы полный заголовок (пустая строка \r\n\r\n)
            if (ContainsHeaderEnd(buffer.AsSpan(0, totalRead)))
            {
                break;
            }
        }

        // Формируем простой HTTP-ответ
        string response = "HTTP/1.1 200 OK\r\n" +
                          "Content-Type: text/plain\r\n" +
                          "Content-Length: 13\r\n" +
                          "Connection: close\r\n" +
                          "\r\n" +
                          "Normal mode OK";
        byte[] responseBytes = Encoding.ASCII.GetBytes(response);
        await sslStream.WriteAsync(responseBytes, ct);
        await sslStream.FlushAsync(ct);

        tcpClient.Close();
    }

    /// <summary>
    /// Обработка режима таймаута: сервер не отвечает на ClientHello от клиента,
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

    /// <summary>
    /// Обработка режима закрытия соединения: сервер закрывает соединение сразу после получения ClientHello от клиента,
    /// из-за чего TLS handshake не завершается и клиент получает ошибку EOF
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    private async Task HandleEOFAfterClientHelloAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Режим EOF");

        NetworkStream stream = tcpClient.GetStream();
        byte[] buffer = new byte[1024];

        int bytesRead = await stream.ReadAsync(buffer, 0, buffer.Length, cancellationToken);
        _logger.LogInformation("Получено {Bytes} байт, закрытие соединения", bytesRead);

        stream.Close();
        tcpClient.Close();
    }
}