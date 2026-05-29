namespace TestSSLError.Proxy.Configurations;

/// <summary>
/// Настройки прокси
/// </summary>
internal class ProxySettings
{
    /// <summary>
    /// Название секции в конфигурационном файле (нужно для корректного получения конфигурации)
    /// </summary>
    public const string SectionName = "ProxySettings";

    /// <summary>
    /// Хост целевого сервера
    /// </summary>
    [Required(ErrorMessage = "Хост целевого сервера обязателен")]
    public required string TargetHost { get; set; }

    /// <summary>
    /// Список пар портов
    /// </summary>
    [Required(ErrorMessage = "Настройки пар портов обязательны")]
    public required List<MappingPorts> MappingPorts { get; set; }
}