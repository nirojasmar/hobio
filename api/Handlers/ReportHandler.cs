using hobio.shared.Models;
using MassTransit;
using Google.Cloud.Firestore;

namespace hobio.api.Handlers;

public class ReportHandler
{
    public static async Task<IResult> HandleReportRequest(
        ReportRequest request,
        IPublishEndpoint publishEndpoint,
        FirestoreDb firestoreDb,
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
            UpdatedAt = DateTime.UtcNow,
            Status = "Pending"
        };
        
        if (firestoreDb != null)
        {
            var docRef = firestoreDb.Collection("ReportJobs").Document(jobId.ToString());
            await docRef.SetAsync(job);
        }
        
        await publishEndpoint.Publish(job);
        
        logger.LogInformation("Queued Job: {JobId}", jobId);

        return Results.Accepted($"/api/report/status/{jobId}", new ReportResponse(jobId));
    }

    public static async Task<IResult> GetReportStatus(
        Guid jobId,
        FirestoreDb firestoreDb)
    {
        var docRef = firestoreDb.Collection("ReportJobs").Document(jobId.ToString());
        var snapshot = await docRef.GetSnapshotAsync();
        
        if (!snapshot.Exists) 
        {
            return Results.NotFound();
        }
        
        var job = snapshot.ConvertTo<ReportJob>();
        return Results.Ok(new ReportStatusResponse(jobId, job.Status, job.StorageUrl, null));
    }
}
