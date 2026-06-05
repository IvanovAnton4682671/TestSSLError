namespace TestSSLError.Client.Configurations;

/// <summary>
/// Настройки клиента
/// </summary>
public class ClientSettings
{
    /// <summary>
    /// Название секции в конфигурационном файле (нужно для корректного получения конфигурации)
    /// </summary>
    public const string SectionName = "ClientSettings";

    /// <summary>
    /// Таймаут для запросов в секундах
    /// </summary>
    [Required(ErrorMessage = "Таймаут для запросов обязателен")]
    public int RequestTimeoutSeconds { get; set; }
}