namespace TestSSLError.Proxy.Configurations;

internal class ProxySettings
{
    public const string SectionName = "ProxySettings";

    [Range(1, 65535)]
    [Required]
    public int ListenPort { get; set; }

    [Required]
    public required string TargetHost { get; set; }

    [Range(1, 65535)]
    [Required]
    public int TargetPort { get; set; }
}