namespace TestSSLError.Proxy.Interfaces;

internal interface IProxyMode
{
    Task HandleAsync(TcpClient client, ProxyContext context, CancellationToken cancellationToken);
}