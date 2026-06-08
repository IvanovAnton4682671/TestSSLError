namespace TestSSLError.Proxy.Models;

internal class ProxyContext
{
    public string TargetHost { get; }
    public int TargetPort { get; }
    public WorkingModes Mode { get; }
    public ILogger Logger { get; }
    public byte[] ReadBuffer { get; set; }      // уже прочитанные данные (заголовки)
    public int ReadBufferLength { get; set; }   // сколько байт реально значимых

    public ProxyContext(string targetHost, int targetPort, WorkingModes mode, ILogger logger)
    {
        TargetHost = targetHost;
        TargetPort = targetPort;
        Mode = mode;
        Logger = logger;
    }
}