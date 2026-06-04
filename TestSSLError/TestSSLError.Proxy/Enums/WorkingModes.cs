namespace TestSSLError.Proxy.Enums;

/// <summary>
/// Режимы работы прокси
/// </summary>
internal enum WorkingModes
{
    /// <summary>
    /// Обычная работа, корректная обработка запроса
    /// </summary>
    Normal,

    /// <summary>
    /// Прокси закрывает соединение после получения ClientHello от клиента
    /// </summary>
    EOFAfterClientHello,

    /// <summary>
    /// Прокси не реагирует на запрос клиента
    /// </summary>
    TimeoutOnConnect
}