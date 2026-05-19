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
    /// Порт сервера
    /// </summary>
    [Required(ErrorMessage = "Порт обязателен")]
    public int Port { get; set; }

    /// <summary>
    /// Режим работы сервера
    /// </summary>
    [Required(ErrorMessage = "Режим работы обязателен")]
    public WorkingModes WorkingMode { get; set; }
}