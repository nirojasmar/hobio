using hobio.api.Handlers;
using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;

namespace hobio.tests.Handlers;

public class ReportHandlerTests
{
    [Fact]
    public async Task HandleReportRequest_ShouldPublishJobAndReturnAccepted()
    {
        // Arrange
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        Google.Cloud.Firestore.FirestoreDb firestoreDb = null!;
        var request = new ReportRequest(2023, new List<string> { "Steam", "LastFm" });

        // Act
        var result = await ReportHandler.HandleReportRequest(request, publishEndpoint, firestoreDb, logger);

        // Assert
        await publishEndpoint.Received(1).Publish(Arg.Is<ReportJob>(j => 
            j.Year == request.Year && 
            j.UserId == "user-123" &&
            j.Sources == request.Sources));

        var acceptedResult = Assert.IsType<Accepted<ReportResponse>>(result);
        Assert.StartsWith("/api/report/status/", acceptedResult.Location);
        Assert.NotEqual(Guid.Empty, acceptedResult.Value.JobId);
    }

    [Fact]
    public async Task HandleReportRequest_WhenPublishFails_ReturnsProblem()
    {
        // Arrange
        var request = new ReportRequest(2024, new List<string> { "Steam" });
        var publishEndpoint = Substitute.For<IPublishEndpoint>();
        publishEndpoint.Publish(Arg.Any<ReportJob>()).ThrowsAsync(new Exception("Simulated publish failure"));
        var logger = Substitute.For<ILogger<hobio.api.Program>>();

        // Act
        var result = await ReportHandler.HandleReportRequest(request, publishEndpoint, null, logger);

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(500, problem.StatusCode);
        Assert.Equal("Failed to publish job", problem.ProblemDetails.Detail);
    }

    [Fact]
    public async Task GetReportStatus_WhenFirestoreIsNull_ReturnsServiceUnavailable()
    {
        // Arrange
        var logger = Substitute.For<ILogger<hobio.api.Program>>();
        var storageService = Substitute.For<hobio.api.Services.IStorageService>();

        // Act
        var result = await ReportHandler.GetReportStatus(Guid.NewGuid(), null, storageService, logger);

        // Assert
        var problem = Assert.IsType<ProblemHttpResult>(result);
        Assert.Equal(503, problem.StatusCode);
        Assert.Equal("FirestoreDb is null or not configured", problem.ProblemDetails.Detail);
    }
}
