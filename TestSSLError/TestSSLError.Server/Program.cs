namespace TestSSLError.Server;

internal class Program
{
    public static void Main(string[] args)
    {
        Host.CreateDefaultBuilder(args)
            .ConfigureServices((hostContext, services) =>
            {
                services.Configure<ServerSettings>(hostContext.Configuration.GetSection(ServerSettings.SectionName));

                services.AddSingleton<SSLErrorService>();
                services.AddHostedService<TCPServer>();
            })
            .Build()
            .Run();
    }
}