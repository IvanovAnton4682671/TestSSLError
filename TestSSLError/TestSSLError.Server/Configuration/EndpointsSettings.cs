namespace TestSSLError.Server.Configuration;

/// <summary>
/// Настройки эндпоинта сервера
/// </summary>
internal class EndpointsSettings
{
    /// <summary>
    /// Порт, по которому работает эндпоинт
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Порт обязателен")]
    public int Port { get; set; }

    /// <summary>
    /// Режим работы эндпоинта
    /// </summary>
    [Required(ErrorMessage = "Режим работы обязателен")]
    public WorkingModes WorkingMode { get; set; }
}