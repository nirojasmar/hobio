using hobio.api.Handlers;
using hobio.shared.Models;
using MassTransit;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.Extensions.Logging;
using NSubstitute;

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
}
