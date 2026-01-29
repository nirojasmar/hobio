using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

builder.Services.AddMassTransit(config =>
{
    config.UsingRabbitMq((context, cfg) =>
    {
        var rabbitHost = builder.Configuration.GetConnectionString("RabbitMQ") ??  "localhost";
        cfg.Host(rabbitHost, "/");
        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
else
{
    app.UseHttpsRedirection();
}

app.MapPost("/api/report", async (
    [FromBody] ReportRequest request,
    IPublishEndpoint publishEndpoint, 
    ILogger<Program> logger) =>
{
    var jobId = Guid.NewGuid();
    
    var job = new ReportJob
    {
        JobId = jobId,
        UserId = "user-123",
        Year = request.Year,
        Sources = request.Sources,
        CreatedAt = DateTime.UtcNow,
        UpdatedAt = DateTime.UtcNow
    };
    
    await publishEndpoint.Publish(job);
    
    logger.LogInformation("Queued Job: {JobId}", jobId);

    return Results.Accepted($"/api/report/status/{jobId}", new { JobId = jobId });
});

app.Run();

public record ReportRequest(int Year, List<string> Sources);