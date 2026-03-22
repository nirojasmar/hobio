using Google.Cloud.Firestore;
using hobio.shared.Models;
using hobio.worker.Services;
using Xunit;

namespace hobio.tests.Services;

/// <summary>
/// Integration tests for the Firestore persistence path in <see cref="JobStatusService"/>.
///
/// These tests require the Firestore emulator to be running and
/// <c>FIRESTORE_EMULATOR_HOST</c> to be set (e.g. "localhost:8080").
/// They are automatically <b>skipped</b> when the environment variable is absent.
/// </summary>
public class JobStatusServiceFirestoreTests : IAsyncLifetime
{
    private const string ProjectId = "hobio-test-worker";
    private const string ReportJobsCollection = "ReportJobs";

    private FirestoreDb? _db;

    public async Task InitializeAsync()
    {
        Skip.If(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")),
            "Skipped: FIRESTORE_EMULATOR_HOST is not set. Start the Firestore emulator to run these tests.");

        _db = await new FirestoreDbBuilder
        {
            ProjectId = ProjectId,
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
        }.BuildAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    private FirestoreDb Db => _db!;

    [SkippableFact]
    public async Task SetProcessingAsync_UpdatesStatusToProcessing()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var docRef = Db.Collection(ReportJobsCollection).Document(jobId.ToString());
        await docRef.SetAsync(new { Status = "Pending", UpdatedAt = DateTime.UtcNow });
        var service = new JobStatusService(Db);

        // Act
        await service.SetProcessingAsync(jobId);

        // Assert
        var snapshot = await docRef.GetSnapshotAsync();
        var status = snapshot.GetValue<string>("Status");
        Assert.Equal("Processing", status);
        
        await docRef.DeleteAsync();
    }

    [SkippableFact]
    public async Task SetCompletedAsync_UpdatesStatusAndStorageUrl()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var docRef = Db.Collection(ReportJobsCollection).Document(jobId.ToString());
        await docRef.SetAsync(new { Status = "Processing", UpdatedAt = DateTime.UtcNow });
        var service = new JobStatusService(Db);

        // Act
        await service.SetCompletedAsync(jobId, "http://example.com/test.pdf");

        // Assert
        var snapshot = await docRef.GetSnapshotAsync();
        var status = snapshot.GetValue<string>("Status");
        var url = snapshot.GetValue<string>("StorageUrl");
        Assert.Equal("Completed", status);
        Assert.Equal("http://example.com/test.pdf", url);
        
        await docRef.DeleteAsync();
    }

    [SkippableFact]
    public async Task SetFailedAsync_UpdatesStatusToFailed()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var docRef = Db.Collection(ReportJobsCollection).Document(jobId.ToString());
        await docRef.SetAsync(new { Status = "Processing", UpdatedAt = DateTime.UtcNow });
        var service = new JobStatusService(Db);

        // Act
        await service.SetFailedAsync(jobId);

        // Assert
        var snapshot = await docRef.GetSnapshotAsync();
        var status = snapshot.GetValue<string>("Status");
        Assert.Equal("Failed", status);
        
        await docRef.DeleteAsync();
    }
}
