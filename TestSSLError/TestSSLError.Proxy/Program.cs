namespace TestSSLError.Proxy;

internal class Program
{
    static async Task Main(string[] args)
    {
        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                services.Configure<ProxySettings>(context.Configuration.GetSection(ProxySettings.SectionName));

                services.AddSingleton<TCPHandler>();
                services.AddHostedService<TCPProxy>();
            })
            .Build();

        await host.RunAsync();
    }
}