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
    /// Порт, по которому сервер принимает соединения
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Порт обязателен")]
    public int Port { get; set; }
}