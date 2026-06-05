namespace TestSSLError.Client;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<List<EndpointsSettings>>(builder.Configuration.GetSection(EndpointsSettings.SectionName));
        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection(ClientSettings.SectionName));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // HttpClient без фиксированного адреса
        builder.Services.AddHttpClient("TargetClient", (serviceProvider, client) =>
        {
            ClientSettings clientSettings = serviceProvider.GetRequiredService<IOptions<ClientSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(clientSettings.RequestTimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true
        });

        WebApplication app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();

        app.Run();
    }
}