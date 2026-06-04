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
    /// Порт на который прокси пересылает данные (сервер)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт сервера должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Порт сервера обязателен")]
    public int TargetPort { get; set; }

    /// <summary>
    /// Список пар портов
    /// </summary>
    [Required(ErrorMessage = "Настройки пар портов обязательны")]
    public required List<MappingPorts> MappingPorts { get; set; }
}