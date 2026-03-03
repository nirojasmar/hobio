namespace hobio.worker;

using hobio.worker.Consumers;
using hobio.worker.Services;
using MassTransit;
using QuestPDF.Infrastructure;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        QuestPDF.Settings.License = LicenseType.Community;
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddSingleton<IStorageService, GcsService>();
        
        builder.Services.AddMassTransit(config =>
        {
            config.AddConsumer<ReportJobConsumer>();

            config.UsingRabbitMq((context, cfg) =>
            {
                var rabbitUri = builder.Configuration.GetConnectionString("RabbitMQ");
                if (!string.IsNullOrEmpty(rabbitUri))
                {
                    cfg.Host(new Uri(rabbitUri));
                }
                else
                {
                    cfg.Host("localhost", "/");
                }
                cfg.ConfigureEndpoints(context);
            });
        });

        var app = builder.Build();

        try
        {
            Console.WriteLine("[Boot] Fetching Application Default Credentials...");
            await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
            Console.WriteLine("[Boot] Successfully cached Application Default Credentials.");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Boot] Warning: Failed to fetch ADC on startup: {ex.Message}");
        }

        app.MapGet("/health", () => Results.Ok("Healthy"));

        await app.RunAsync();
    }
}
