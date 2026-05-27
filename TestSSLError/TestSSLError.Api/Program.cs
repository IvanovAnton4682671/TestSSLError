namespace TestSSLError.Client;

public class Program
{
    public static void Main(string[] args)
    {
        WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection(ClientSettings.SectionName));

        builder.Services.AddControllers();
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        // HttpClient без фиксированного адреса
        builder.Services.AddHttpClient("TargetServerClient", (serviceProvider, client) =>
        {
            ClientSettings settings = serviceProvider.GetRequiredService<IOptions<ClientSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
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