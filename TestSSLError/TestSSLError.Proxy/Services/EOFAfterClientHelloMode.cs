namespace TestSSLError.Proxy.Services;

internal class EOFAfterClientHelloMode : IProxyMode
{
    public async Task HandleAsync(TcpClient client, ProxyContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("EOFAfterClientHelloMode: получено {Length} байт (ClientHello), закрываем соединение", context.ReadBufferLength);
        // Просто закрываем соединение, ничего не отправляем
        await Task.CompletedTask;
    }
}