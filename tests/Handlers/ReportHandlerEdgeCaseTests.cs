using Google.Cloud.Firestore;
using hobio.api.Handlers;
using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace hobio.tests.Handlers;

/// <summary>
/// Unit tests for edge-case branches in <see cref="ReportHandler"/> that do
/// not require a live Firestore connection (FirestoreDb is always null or
/// substituted via the Firestore emulator path covered in
/// ReportHandlerFirestoreTests).
/// </summary>
public class ReportHandlerEdgeCaseTests
{
    // -------------------------------------------------------------------------
    // HandleReportRequest edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// When Firestore is null the handler should still publish the job and
    /// return Accepted (existing test covers this, but exercising the
    /// null-firestoreDb fast-path explicitly here for clarity).
    /// </summary>
    [Fact]
    public async Task HandleReportRequest_NullFirestore_StillPublishesAndReturnsAccepted()
    {
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var request = new ReportRequest(2025, new List<string> { "Steam" });

        var result = await ReportHandler.HandleReportRequest(request, publishEndpoint, null!, logger);

        var accepted = Assert.IsType<Accepted<ReportResponse>>(result);
        Assert.NotEqual(Guid.Empty, accepted.Value!.JobId);
        await publishEndpoint.Received(1).Publish(Arg.Any<ReportJob>());
    }

    // -------------------------------------------------------------------------
    // GetReportStatus edge cases
    // -------------------------------------------------------------------------

    /// <summary>
    /// When the job's Status is "Completed" but StorageUrl is null,
    /// the handler must NOT call GetSignedDownloadUrlAsync and should return
    /// Ok with a null DownloadUrl.
    /// </summary>
    [SkippableFact]
    public async Task GetReportStatus_CompletedJobWithNullStorageUrl_ReturnsOkWithNullDownloadUrl()
    {
        // Arrange – seed a "Completed" job that has no StorageUrl yet
        // We exercise the null-firestoreDb guard path we already have a test for,
        // but we still need to hit the branch inside the try block.
        // Use the Firestore emulator when available; skip otherwise.
        Skip.If(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")),
            "Skipped: FIRESTORE_EMULATOR_HOST is not set.");

        var db = await new FirestoreDbBuilder
        {
            ProjectId = "hobio-test-edge",
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
        }.BuildAsync();

        var jobId = Guid.NewGuid();
        var docRef = db.Collection("ReportJobs").Document(jobId.ToString());
        var job = new ReportJob
        {
            JobId = jobId,
            UserId = "user-123",
            Year = 2024,
            Sources = new List<string> { "Steam" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Completed",
            StorageUrl = null   // ← no URL
        };
        await docRef.SetAsync(job);

        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();

        // Act
        var result = await ReportHandler.GetReportStatus(jobId, db, storageService, logger);

        // Assert
        var ok = Assert.IsType<Ok<ReportStatusResponse>>(result);
        Assert.Equal(jobId, ok.Value!.JobId);
        Assert.Equal("Completed", ok.Value.Status);
        Assert.Null(ok.Value.DownloadUrl);
        await storageService.DidNotReceive().GetSignedDownloadUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>());

        // clean up
        await docRef.DeleteAsync();
    }

    /// <summary>
    /// When the job's Status is NOT "Completed" (e.g. "Pending"),
    /// the handler must skip signed-URL generation and return Ok with null DownloadUrl.
    /// </summary>
    [SkippableFact]
    public async Task GetReportStatus_PendingJob_ReturnsOkWithNullDownloadUrl()
    {
        Skip.If(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")),
            "Skipped: FIRESTORE_EMULATOR_HOST is not set.");

        var db = await new FirestoreDbBuilder
        {
            ProjectId = "hobio-test-edge",
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
        }.BuildAsync();

        var jobId = Guid.NewGuid();
        var docRef = db.Collection("ReportJobs").Document(jobId.ToString());
        var job = new ReportJob
        {
            JobId = jobId,
            UserId = "user-123",
            Year = 2024,
            Sources = new List<string> { "Steam" },
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = "Pending",
            StorageUrl = null
        };
        await docRef.SetAsync(job);

        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();

        var result = await ReportHandler.GetReportStatus(jobId, db, storageService, logger);

        var ok = Assert.IsType<Ok<ReportStatusResponse>>(result);
        Assert.Equal("Pending", ok.Value!.Status);
        Assert.Null(ok.Value.DownloadUrl);
        await storageService.DidNotReceive().GetSignedDownloadUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>());

        await docRef.DeleteAsync();
    }

    /// <summary>
    /// When the signed-URL generation throws, the handler swallows the exception,
    /// logs a warning, and returns Ok with a null DownloadUrl (graceful degradation).
    /// </summary>
    [SkippableFact]
    public async Task GetReportStatus_SignedUrlGenerationFails_ReturnsOkWithNullDownloadUrl()
    {
        Skip.If(
            string.IsNullOrEmpty(Environment.GetEnvironmentVariable("FIRESTORE_EMULATOR_HOST")),
            "Skipped: FIRESTORE_EMULATOR_HOST is not set.");

        var db = await new FirestoreDbBuilder
        {
            ProjectId = "hobio-test-edge",
            EmulatorDetection = Google.Api.Gax.EmulatorDetection.EmulatorOnly
        }.BuildAsync();

        var jobId = Guid.NewGuid();
        var docRef = db.Collection("ReportJobs").Document(jobId.ToString());
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

        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();
        storageService
            .GetSignedDownloadUrlAsync(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .ThrowsAsync(new InvalidOperationException("Signing failed"));

        var result = await ReportHandler.GetReportStatus(jobId, db, storageService, logger);

        var ok = Assert.IsType<Ok<ReportStatusResponse>>(result);
        Assert.Equal("Completed", ok.Value!.Status);
        Assert.Null(ok.Value.DownloadUrl);   // gracefully degraded

        await docRef.DeleteAsync();
    }
}
