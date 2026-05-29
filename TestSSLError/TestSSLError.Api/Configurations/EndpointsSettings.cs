namespace TestSSLError.Client.Configurations;

/// <summary>
/// Настройки для конечных точек подключения (сервер и прокси)
/// </summary>
public class EndpointsSettings
{
    /// <summary>
    /// Порт, на который мы хотим подключиться (сервер или прокси)
    /// </summary>
    [Range(1, 65535, ErrorMessage = "Порт должен быть в диапазоне от 1 до 65535")]
    [Required(ErrorMessage = "Целевой порт обязателен")]
    public int Port { get; set; }

    /// <summary>
    /// Полный адрес конечой точки подключения (сервер или прокси)
    /// </summary>
    [Required(ErrorMessage = "Полный адрес конечной точки обязателен")]
    public required string BaseUrl { get; set; }
}