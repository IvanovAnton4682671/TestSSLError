namespace TestSSLError.Proxy.Configurations;

/// <summary>
/// Настройки пар портов
/// </summary>
internal class MappingPorts
{
    /// <summary>
    /// Порт который прокси слушает (клиент)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт клиента должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Порт клиента обязателен")]
    public int ListenPort { get; set; }

    /// <summary>
    /// Порт на который прокси пересылает данные (сервер)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт сервера должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Порт сервера обязателен")]
    public int TargetPort { get; set; }
}