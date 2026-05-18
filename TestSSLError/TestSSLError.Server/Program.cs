namespace TestSSLError.Server;

public class Program
{
    public static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ServerSettings>(hostContext.Configuration.GetSection(ServerSettings.SectionName));

                services.AddSingleton<SSLErrorService>();
                services.AddHostedService<TCPServer>();
            })
            .ConfigureLogging(logging =>
            {
                logging.AddConsole();
            });
}