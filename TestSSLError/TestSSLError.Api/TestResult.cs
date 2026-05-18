namespace TestSSLError.Api;

public class TestResult
{
    public bool Success { get; set; }

    public string TestMode { get; set; } = string.Empty;

    public string ExpectedException { get; set; } = string.Empty;

    public string ActualException { get; set; } = string.Empty;

    public string Message { get; set; } = string.Empty;
}