namespace TestSSLError.Api;

public class ClientSettings
{
    public const string SectionName = "ClientSettings";

    [Required(ErrorMessage = "Целевой адрес сервера обязателен")]
    public required string TargetServerBaseUrl { get; set; }

    [Required(ErrorMessage = "Таймаут запроса в секундах обязателен")]
    public int RequestTimeoutSeconds { get; set; }
}