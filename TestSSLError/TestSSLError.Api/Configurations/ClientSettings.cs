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
    /// Адрес целевого TCP-сервера
    /// </summary>
    public required string TargetServerBaseUrl { get; set; }

    /// <summary>
    /// Таймаут для запросов в секундах
    /// </summary>
    public int RequestTimeoutSeconds { get; set; }
}