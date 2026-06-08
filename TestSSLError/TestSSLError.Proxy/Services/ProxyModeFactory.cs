namespace TestSSLError.Proxy.Services;

internal static class ProxyModeFactory
{
    public static IProxyMode Create(WorkingModes mode)
    {
        return mode switch
        {
            WorkingModes.Normal => new NormalMode(),
            WorkingModes.TimeoutOnConnect => new TimeoutOnConnectMode(),
            WorkingModes.EOFAfterClientHello => new EOFAfterClientHelloMode(),
            _ => throw new NotSupportedException($"Режим {mode} не поддерживается")
        };
    }
}