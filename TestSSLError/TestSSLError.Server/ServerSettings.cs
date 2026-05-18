namespace TestSSLError.Server;

public class ServerSettings
{
    public const string SectionName = "ServerSettings";

    [Required(ErrorMessage = "Порт обязателен")]
    public int Port { get; set; }

    [Required(ErrorMessage = "Режим работы обязателен")]
    public WorkingModes WorkingMode { get; set; }
}