namespace TestSSLError.Server.Configuration;

/// <summary>
/// Настройки сервера
/// </summary>
internal class ServerSettings
{
    /// <summary>
    /// Название секции в конфигурационном файле (нужно для корректного получения конфигурации)
    /// </summary>
    public const string SectionName = "ServerSettings";

    /// <summary>
    /// Настройки эндпоинтов сервера
    /// </summary>
    [Required(ErrorMessage = "Настройки эндпоинтов обязательны")]
    public required List<EndpointsSettings> EndpointsSettings { get; set; }
}