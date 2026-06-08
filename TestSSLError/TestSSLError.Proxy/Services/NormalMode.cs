namespace TestSSLError.Proxy.Services;

internal class NormalMode : IProxyMode
{
    public async Task HandleAsync(TcpClient client, ProxyContext context, CancellationToken cancellationToken)
    {
        context.Logger.LogInformation("NormalMode: проксирование к {TargetHost}:{TargetPort}", context.TargetHost, context.TargetPort);

        using var target = new TcpClient();
        await target.ConnectAsync(context.TargetHost, context.TargetPort, cancellationToken);

        using var clientStream = client.GetStream();
        using var targetStream = target.GetStream();

        // Если уже есть прочитанные данные (часть запроса), сначала отправляем их
        if (context.ReadBufferLength > 0)
        {
            await targetStream.WriteAsync(context.ReadBuffer.AsMemory(0, context.ReadBufferLength), cancellationToken);
        }

        // Запускаем двунаправленное копирование
        var clientToTarget = clientStream.CopyToAsync(targetStream, cancellationToken);
        var targetToClient = targetStream.CopyToAsync(clientStream, cancellationToken);

        await Task.WhenAny(clientToTarget, targetToClient);
    }
}