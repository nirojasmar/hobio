using hobio.shared.Models;
using MassTransit;
using Google.Cloud.Firestore;

namespace hobio.api.Handlers;

public class ReportHandler
{
    private const string ReportJobsCollection = "ReportJobs";
    
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
        
        DocumentReference? docRef = null;
        if (firestoreDb != null)
        {
            docRef = firestoreDb.Collection(ReportJobsCollection).Document(jobId.ToString());
            await docRef.SetAsync(job);
        }

        try
        {
            await publishEndpoint.Publish(job);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to publish job {JobId}; removing stale Firestore document", jobId);
            if (docRef != null)
            {
                await docRef.DeleteAsync();
            }
            throw;
        }
        
        logger.LogInformation("Queued Job: {JobId}", jobId);

        return Results.Accepted($"/api/report/status/{jobId}", new ReportResponse(jobId));
    }

    public static async Task<IResult> GetReportStatus(
        Guid jobId,
        FirestoreDb firestoreDb)
    {
        if (firestoreDb == null)
        {
            return Results.Problem("FirestoreDb is null or not configured", statusCode: 503);
        }

        try
        {
            var docRef = firestoreDb.Collection(ReportJobsCollection).Document(jobId.ToString());
            var snapshot = await docRef.GetSnapshotAsync();
        
            if (!snapshot.Exists) 
            {
                return Results.NotFound();
            }
        
            var job = snapshot.ConvertTo<ReportJob>();
            return Results.Ok(new ReportStatusResponse(jobId, job.Status, job.StorageUrl, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get report status for job {JobId}", jobId);
            return Results.Problem("Failed to get report status", statusCode: 500);
        }
    }
}
