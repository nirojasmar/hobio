using hobio.worker.Consumers;
using MassTransit;

var builder = Host.CreateApplicationBuilder(args);

builder.Services.AddMassTransit(config =>
{
    config.AddConsumer<ReportJobConsumer>();

    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration.GetConnectionString("RabbitMQ") ??  "localhost";
        cfg.Host(rabbitHost, "/");
        cfg.ConfigureEndpoints(context);
    });
});

var host = builder.Build();
host.Run();