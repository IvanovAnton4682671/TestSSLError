namespace TestSSLError.Client.Configurations;

public class ClientSettings
{
    public const string SectionName = "ClientSettings";

    [Range(1, 65535)]
    [Required]
    public int ProxyPort { get; set; }

    [Required]
    public required string ProxyBaseUrl { get; set; }

    [Required]
    public int RequestTimeoutSeconds { get; set; }

    public bool EnableConnectionLogging { get; set; } = false;
}