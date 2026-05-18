namespace TestSSLError.Server;

public enum WorkingModes
{
    Normal,

    TimeoutOnConnect,

    EOFAfterClientHello
}