namespace hobio.worker;

using hobio.worker.Consumers;
using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

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