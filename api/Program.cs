namespace hobio.api;

using hobio.api.Handlers;
using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Google.Cloud.Firestore;

public class Program
{
    public static async Task Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        var projectId = builder.Configuration["GOOGLE_CLOUD_PROJECT"] ?? "hobio-nonprod";
        
        // Add services to the container.
        // Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
        builder.Services.AddOpenApi();
        
        var databaseId = builder.Configuration["FIRESTORE_DATABASE_ID"] ?? "(default)";
        builder.Services.AddSingleton(new FirestoreDbBuilder 
        { 
            ProjectId = projectId, 
            DatabaseId = databaseId 
        }.Build());
        
        builder.Services.AddMassTransit(config =>
        {
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
        
        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }
        
        app.MapGet("/", () => Results.Ok("Healthy"));
        app.MapPost("/api/report", ReportHandler.HandleReportRequest);
        app.MapGet("/api/report/status/{jobId}", ReportHandler.GetReportStatus);
        
        await app.RunAsync();
    }
}
