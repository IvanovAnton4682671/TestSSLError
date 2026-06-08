namespace TestSSLError.Client;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection(ClientSettings.SectionName));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // Общий SocketsHttpHandler с единым пулом
        builder.Services.AddSingleton(sp =>
        {
            var settings = sp.GetRequiredService<IOptions<ClientSettings>>().Value;
            var handler = new SocketsHttpHandler
            {
                PooledConnectionIdleTimeout = TimeSpan.FromSeconds(30),
                PooledConnectionLifetime = TimeSpan.FromMinutes(5),
                MaxConnectionsPerServer = 10,
                ConnectTimeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds)
            };

            if (settings.EnableConnectionLogging)
            {
                handler.ConnectCallback = async (context, ct) =>
                {
                    var socket = new Socket(SocketType.Stream, ProtocolType.Tcp);
                    var logger = sp.GetRequiredService<ILogger<Program>>();
                    try
                    {
                        logger.LogInformation("[Connect] Connecting to {Host}:{Port}", context.DnsEndPoint.Host, context.DnsEndPoint.Port);
                        await socket.ConnectAsync(context.DnsEndPoint, ct);
                        logger.LogInformation("[Connect] Connected from {LocalEndPoint}", socket.LocalEndPoint);
                        return new NetworkStream(socket, ownsSocket: true);
                    }
                    catch
                    {
                        socket.Dispose();
                        throw;
                    }
                };
            }
            return handler;
        });

        builder.Services.AddHttpClient("ProxyClient")
            .ConfigureHttpClient((sp, client) =>
            {
                var settings = sp.GetRequiredService<IOptions<ClientSettings>>().Value;
                client.BaseAddress = new Uri(settings.ProxyBaseUrl);
                client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
                client.DefaultRequestHeaders.ConnectionClose = false;
            })
            .ConfigurePrimaryHttpMessageHandler(sp => sp.GetRequiredService<SocketsHttpHandler>());

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();

        app.Run();
    }
}