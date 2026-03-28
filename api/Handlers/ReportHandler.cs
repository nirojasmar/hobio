using hobio.shared.Models;
using MassTransit;
using Google.Cloud.Firestore;
using hobio.api.Services;

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
            try
            {
                docRef = firestoreDb.Collection(ReportJobsCollection).Document(jobId.ToString());
                await docRef.SetAsync(job);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Failed to create document for job {JobId}", jobId);
                return Results.Problem("Failed to create document for job", statusCode: 500);
            }
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
            return Results.Problem("Failed to publish job", statusCode: 500);
        }
        
        logger.LogInformation("Queued Job: {JobId}", jobId);

        return Results.Accepted($"/api/report/status/{jobId}", new ReportResponse(jobId));
    }

    public static async Task<IResult> GetReportStatus(
        Guid jobId,
        FirestoreDb firestoreDb,
        IStorageService storageService,
        ILogger<Program> logger)
    {
        if (firestoreDb == null)
        {
            logger.LogError("FirestoreDb is null or not configured");
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
            
            string? downloadUrl = job.StorageUrl;
            if (job.Status == "Completed" && !string.IsNullOrEmpty(job.StorageUrl))
            {
                try
                {
                    downloadUrl = await storageService.GetSignedDownloadUrlAsync(job.StorageUrl, TimeSpan.FromMinutes(15));
                }
                catch (Exception ex)
                {
                    logger.LogWarning(ex, "Failed to generate signed URL for job {JobId}", jobId);
                    // Fall back to returning the raw URL if signing fails.
                }
            }
            
            return Results.Ok(new ReportStatusResponse(jobId, job.Status, downloadUrl, null));
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to get report status for job {JobId}", jobId);
            return Results.Problem("Failed to get report status", statusCode: 500);
        }
    }
}
