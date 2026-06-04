namespace TestSSLError.Server.Services;

/// <summary>
/// Отвечает за обработку соединения
/// </summary>
internal class TCPHandler
{
    private readonly ILogger<TCPHandler> _logger;
    private readonly X509Certificate2 _serverCertificate;

    public TCPHandler(ILogger<TCPHandler> logger, X509Certificate2 serverCertificate)
    {
        _logger = logger;
        _serverCertificate = serverCertificate;
    }

    /// <summary>
    /// Обработка запроса: выполняем полный TLS handshake и отвечаем 200 OK
    /// </summary>
    /// <param name="tcpClient">Клиент, который устанавливает соединение</param>
    /// <param name="cancellationToken">Токен отмены</param>
    public async Task HandleAsync(TcpClient tcpClient, CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation("Начало обработки запроса");

            using NetworkStream stream = tcpClient.GetStream();
            using SslStream sslStream = new SslStream(stream, false);

            // Выполняем TLS handshake
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
                int bytesRead = await sslStream.ReadAsync(buffer.AsMemory(totalRead, buffer.Length - totalRead), cancellationToken);

                // Клиент закрыл соединение
                if (bytesRead == 0)
                {
                    break;
                }

                totalRead += bytesRead;

                // Проверяем, получили ли мы полный заголовок (пустая строка \r\n\r\n)
                bool headerEndFound = false;
                for (int i = 0; i < totalRead - 3; i++)
                {
                    if (buffer[i] == '\r' && buffer[i + 1] == '\n' && buffer[i + 2] == '\r' && buffer[i + 3] == '\n')
                    {
                        headerEndFound = true;
                        break;
                    }
                }

                if (headerEndFound)
                {
                    break;
                }
            }

            // Формируем простой HTTP-ответ
            string responseBody = "OK";
            string response = "HTTP/1.1 200 OK\r\n" +
                              "Content-Type: text/plain\r\n" +
                              $"Content-Length: {responseBody.Length}\r\n" +
                              "Connection: close\r\n" +
                              "\r\n" +
                              responseBody;
            byte[] responseBytes = Encoding.ASCII.GetBytes(response);
            await sslStream.WriteAsync(responseBytes, cancellationToken);
            await sslStream.FlushAsync(cancellationToken);

            tcpClient.Close();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Произошла ошибка при обработке соединения");
        }
    }
}
