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

        // HttpClient без фиксированного адреса
        builder.Services.AddHttpClient("TargetServerClient", (serviceProvider, client) =>
        {
            var settings = serviceProvider.GetRequiredService<IOptions<ClientSettings>>().Value;
            client.Timeout = TimeSpan.FromSeconds(settings.RequestTimeoutSeconds);
        })
        .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
        {
            ServerCertificateCustomValidationCallback = (sender, cert, chain, SslPolicyErrors) => true
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();

        app.Run();
    }
}