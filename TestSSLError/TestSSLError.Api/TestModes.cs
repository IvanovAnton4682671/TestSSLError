namespace TestSSLError.Api;

public enum TestModes
{
    Normal,

    TimeoutOnConnect,

    EOFAfterClientHello
}