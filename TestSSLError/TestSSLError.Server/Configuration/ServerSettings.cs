namespace TestSSLError.Server.Configuration;

/// <summary>
/// Настройки сервера
/// </summary>
internal class ServerSettings
{
    public const string SectionName = "ServerSettings";

    [Range(1, 65535)]
    [Required]
    public int Port { get; set; }

    /// <summary>
    /// Разрешить постоянные соединения (Keep-Alive)
    /// </summary>
    public bool EnableKeepAlive { get; set; } = false;

    /// <summary>
    /// Максимальное количество запросов на одно соединение (null = без ограничений)
    /// </summary>
    public int? MaxRequestsPerConnection { get; set; }

    /// <summary>
    /// Таймаут бездействия в секундах (null = без таймаута)
    /// </summary>
    public int? IdleTimeoutSeconds { get; set; }

    /// <summary>
    /// Таймаут keep-alive, отправляемый клиенту в заголовке (секунды)
    /// </summary>
    public int? KeepAliveTimeoutSeconds { get; set; }
}