namespace TestSSLError.Api;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Конфигурация
        builder.Configuration.AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);

        // Настройки клиента
        builder.Services.Configure<ClientSettings>(builder.Configuration.GetSection(ClientSettings.SectionName));

        // Сервисы
        builder.Services.AddSingleton<SSLErrorTestService>();
        builder.Services.AddControllers();

        // Swagger
        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo { Title = "SSL Error Client", Version = "v1" });
        });

        var app = builder.Build();

        app.UseSwagger();
        app.UseSwaggerUI();
        app.MapControllers();

        app.Run();
    }
}