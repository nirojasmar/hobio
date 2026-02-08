using hobio.worker.Consumers;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

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

var host = builder.Build();
await host.RunAsync();