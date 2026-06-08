namespace TestSSLError.Proxy.Services;

internal class TimeoutOnConnectMode : IProxyMode
{
    public async Task HandleAsync(TcpClient client, ProxyContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("TimeoutOnConnectMode: соединение зависает навсегда");
        try
        {
            await Task.Delay(Timeout.Infinite, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            context.Logger.LogInformation("TimeoutOnConnectMode прерван");
        }
    }
}