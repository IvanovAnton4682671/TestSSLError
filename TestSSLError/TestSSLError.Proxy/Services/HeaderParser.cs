namespace TestSSLError.Proxy.Services;

internal static class HeaderParser
{
    /// <summary>
    /// Ищет в буфере заголовок X-Scenario и возвращает его значение (регистронезависимо)
    /// </summary>
    public static WorkingModes? GetScenarioFromHeader(byte[] buffer, int length)
    {
        // Ищем "X-Scenario:" в первых length байтах
        string text = Encoding.ASCII.GetString(buffer, 0, length);
        var lines = text.Split(new[] { "\r\n" }, StringSplitOptions.None);
        foreach (var line in lines)
        {
            if (line.StartsWith("X-Scenario:", StringComparison.OrdinalIgnoreCase))
            {
                string value = line.Substring("X-Scenario:".Length).Trim();
                if (Enum.TryParse<WorkingModes>(value, true, out var mode))
                    return mode;
            }
        }
        return null;
    }
}