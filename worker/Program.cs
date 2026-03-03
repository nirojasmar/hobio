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
        
        // Eagerly fetch Google Cloud Credentials during startup (unthrottled CPU boost phase)
        Console.WriteLine("[Boot] Fetching Application Default Credentials...");
        var googleCredential = await Google.Apis.Auth.OAuth2.GoogleCredential.GetApplicationDefaultAsync();
        builder.Services.AddSingleton(googleCredential);
        Console.WriteLine("[Boot] Successfully cached Application Default Credentials in DI.");
        
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

        app.MapGet("/health", () => Results.Ok("Healthy"));

        await app.RunAsync();
    }
}
