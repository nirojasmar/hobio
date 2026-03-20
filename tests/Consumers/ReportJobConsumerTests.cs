using hobio.shared.Models;
using hobio.worker.Consumers;
using hobio.worker.Services;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using QuestPDF.Infrastructure;
using Xunit;

namespace hobio.tests.Consumers;

public class ReportJobConsumerTests
{
    public ReportJobConsumerTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    [Fact]
    public async Task Consume_ShouldLogAndComplete_WhenSuccessful()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ReportJobConsumer>>();
        var storageService = Substitute.For<IStorageService>();
        var jobStatusService = Substitute.For<IJobStatusService>();
        var consumer = new ReportJobConsumer(logger, storageService, jobStatusService);
        var context = Substitute.For<ConsumeContext<ReportJob>>();
        
        var job = new ReportJob { JobId = Guid.NewGuid(), UserId = "user-123", Year = 2023, Sources = new List<string> { "Test Source" } };
        context.Message.Returns(job);

        // Act
        await consumer.Consume(context);

        // Assert
        await storageService.Received(1).UploadFileAsync(Arg.Any<byte[]>(), Arg.Any<string>(), "application/pdf");
        await jobStatusService.Received(1).SetProcessingAsync(job.JobId);
        await jobStatusService.Received(1).SetCompletedAsync(job.JobId, Arg.Any<string>());
        await jobStatusService.DidNotReceive().SetFailedAsync(Arg.Any<Guid>());
    }

    [Fact]
    public async Task Consume_ShouldSetFailedStatus_WhenExceptionOccurs()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ReportJobConsumer>>();
        var storageService = Substitute.For<IStorageService>();
        var jobStatusService = Substitute.For<IJobStatusService>();
        var consumer = new ReportJobConsumer(logger, storageService, jobStatusService);
        var context = Substitute.For<ConsumeContext<ReportJob>>();

        var job = new ReportJob { JobId = Guid.NewGuid(), UserId = "user-failure", Year = 2023, Sources = new List<string> { "Test Source" } };
        context.Message.Returns(job);

        storageService
            .UploadFileAsync(Arg.Any<byte[]>(), Arg.Any<string>(), Arg.Any<string>())
            .ThrowsAsync(new InvalidOperationException("Simulated upload failure"));

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => consumer.Consume(context));
        Assert.Equal("Failed to generate report", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);

        await jobStatusService.Received(1).SetProcessingAsync(job.JobId);
        await jobStatusService.Received(1).SetFailedAsync(job.JobId);
        await jobStatusService.DidNotReceive().SetCompletedAsync(Arg.Any<Guid>(), Arg.Any<string>());
    }
}

