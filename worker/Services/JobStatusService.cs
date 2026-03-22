using Google.Cloud.Firestore;

namespace hobio.worker.Services;

public interface IJobStatusService
{
    Task SetProcessingAsync(Guid jobId);
    Task SetCompletedAsync(Guid jobId, string storageUrl);
    Task SetFailedAsync(Guid jobId);
}

public class JobStatusService : IJobStatusService
{
    private const string ReportJobsCollection = "ReportJobs";

    private readonly FirestoreDb _firestoreDb;

    public JobStatusService(FirestoreDb firestoreDb)
    {
        _firestoreDb = firestoreDb;
    }

    public Task SetProcessingAsync(Guid jobId) =>
        GetDocRef(jobId).SetAsync(new Dictionary<string, object>
        {
            { "Status", "Processing" },
            { "UpdatedAt", DateTime.UtcNow }
        }, SetOptions.MergeAll);

    public Task SetCompletedAsync(Guid jobId, string storageUrl) =>
        GetDocRef(jobId).SetAsync(new Dictionary<string, object>
        {
            { "Status", "Completed" },
            { "StorageUrl", storageUrl },
            { "UpdatedAt", DateTime.UtcNow }
        }, SetOptions.MergeAll);

    public Task SetFailedAsync(Guid jobId) =>
        GetDocRef(jobId).SetAsync(new Dictionary<string, object>
        {
            { "Status", "Failed" },
            { "UpdatedAt", DateTime.UtcNow }
        }, SetOptions.MergeAll);

    private DocumentReference GetDocRef(Guid jobId) =>
        _firestoreDb.Collection(ReportJobsCollection).Document(jobId.ToString());
}
