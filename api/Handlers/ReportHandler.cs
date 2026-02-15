using hobio.shared.Models;
using MassTransit;

namespace hobio.api.Handlers;

public class ReportHandler
{
    public static async Task<IResult> HandleReportRequest(
        ReportRequest request,
        IPublishEndpoint publishEndpoint,
        ILogger<Program> logger)
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

        return Results.Accepted($"/api/report/status/{jobId}", new ReportResponse(jobId));
    }
}
