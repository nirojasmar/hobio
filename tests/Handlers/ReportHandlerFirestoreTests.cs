using Google.Cloud.Firestore;
using hobio.api.Handlers;
using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace hobio.tests.Handlers;

/// <summary>
/// Integration tests for the Firestore persistence path in <see cref="ReportHandler"/>.
///
/// These tests require the Firestore emulator to be running and
/// <c>FIRESTORE_EMULATOR_HOST</c> to be set (e.g. "localhost:8080").
/// They are automatically <b>skipped</b> when the environment variable is absent,
/// so they never fail on developer machines that don't have the emulator.
///
/// To run locally:
///   gcloud beta emulators firestore start --host-port=localhost:8787
///   $env:FIRESTORE_EMULATOR_HOST = "localhost:8787"
///   dotnet test --filter "FullyQualifiedName~ReportHandlerFirestoreTests"
/// </summary>
public class ReportHandlerFirestoreTests : IAsyncLifetime
{
    private const string ProjectId = "hobio-test";
    private const string ReportJobsCollection = "ReportJobs";

    private FirestoreDb? _db;

    // -------------------------------------------------------------------------
    // Lifecycle
    // -------------------------------------------------------------------------

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

    // -------------------------------------------------------------------------
    // Helpers
    // -------------------------------------------------------------------------

    private FirestoreDb Db => _db!; // safe after InitializeAsync guard

    private (IPublishEndpoint, ILogger<hobio.api.Program>) CreateInfra()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        return (publishEndpoint, logger);
    }

    // -------------------------------------------------------------------------
    // Tests
    // -------------------------------------------------------------------------

    /// <summary>
    /// HandleReportRequest with a real FirestoreDb should write a Pending document
    /// whose fields match the request.
    /// </summary>
    [SkippableFact]
    public async Task HandleReportRequest_WithFirestoreDb_WritesJobDocument()
    {
        // Arrange
        var (publishEndpoint, logger) = CreateInfra();
        var request = new ReportRequest(2024, new List<string> { "Steam", "LastFm" });

        // Act
        var result = await ReportHandler.HandleReportRequest(request, publishEndpoint, Db, logger);

        // Assert – HTTP response
        var accepted = Assert.IsType<Accepted<ReportResponse>>(result);
        var jobId = accepted.Value!.JobId;
        Assert.NotEqual(Guid.Empty, jobId);
        Assert.StartsWith("/api/report/status/", accepted.Location);

        // Assert – Firestore document was written
        var docRef = Db.Collection(ReportJobsCollection).Document(jobId.ToString());
        var snapshot = await docRef.GetSnapshotAsync();

        Assert.True(snapshot.Exists, "ReportJob document should exist in Firestore after HandleReportRequest");

        var stored = snapshot.ConvertTo<ReportJob>();
        Assert.Equal(jobId, stored.JobId);
        Assert.Equal("user-123", stored.UserId);
        Assert.Equal(2024, stored.Year);
        Assert.Equal(new List<string> { "Steam", "LastFm" }, stored.Sources);
        Assert.Equal("Pending", stored.Status);

        // clean up
        await docRef.DeleteAsync();
    }

    /// <summary>
    /// GetReportStatus for a job that exists in Firestore should return 200 OK
    /// with the correct status and jobId.
    /// </summary>
    [SkippableFact]
    public async Task GetReportStatus_ExistingJob_ReturnsOkWithStatus()
    {
        // Arrange – seed a known document
        var jobId = Guid.NewGuid();
        var docRef = Db.Collection(ReportJobsCollection).Document(jobId.ToString());

        var job = new ReportJob
        {
            JobId = jobId,
            UserId = "user-123",
            Year = 2024,
            Sources = new List<string> { "Steam" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Completed",
            StorageUrl = "https://storage.example.com/report.pdf"
        };
        await docRef.SetAsync(job);

        // Act
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();
        storageService.GetSignedDownloadUrlAsync("https://storage.example.com/report.pdf", Arg.Any<TimeSpan>())
            .Returns(Task.FromResult("https://storage.example.com/signed-url.pdf"));

        var result = await ReportHandler.GetReportStatus(jobId, Db, storageService, logger);

        // Assert
        var ok = Assert.IsType<Ok<ReportStatusResponse>>(result);
        Assert.Equal(jobId, ok.Value!.JobId);
        Assert.Equal("Completed", ok.Value.Status);
        Assert.Equal("https://storage.example.com/signed-url.pdf", ok.Value.DownloadUrl);

        // clean up
        await docRef.DeleteAsync();
    }

    /// <summary>
    /// GetReportStatus for a job that does NOT exist in Firestore should return 404.
    /// </summary>
    [SkippableFact]
    public async Task GetReportStatus_MissingJob_ReturnsNotFound()
    {
        // Arrange – use a random ID that was never written
        var missingJobId = Guid.NewGuid();

        // Act
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();
        var result = await ReportHandler.GetReportStatus(missingJobId, Db, storageService, logger);

        // Assert
        Assert.IsType<NotFound>(result);
    }
}
