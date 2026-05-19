namespace TestSSLError.Server.Enums;

/// <summary>
/// Режимы работы сервера
/// </summary>
internal enum WorkingModes
{
    /// <summary>
    /// Обычная работа, корректная обработка запроса
    /// </summary>
    Normal,

    /// <summary>
    /// Сервер не отвечает на запрос клиента
    /// </summary>
    TimeoutOnConnect,

    /// <summary>
    /// Сервер закрывает соединение после получения ClientHello от клиента
    /// </summary>
    EOFAfterClientHello
}