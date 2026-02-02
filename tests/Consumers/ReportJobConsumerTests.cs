using hobio.shared.Models;
using hobio.worker.Consumers;
using MassTransit;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit;

namespace hobio.tests.Consumers;

public class ReportJobConsumerTests
{
    [Fact]
    public async Task Consume_ShouldLogAndComplete_WhenSuccessful()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ReportJobConsumer>>();
        var consumer = new ReportJobConsumer(logger);
        var context = Substitute.For<ConsumeContext<ReportJob>>();
        
        var job = new ReportJob { JobId = Guid.NewGuid(), UserId = "user-123", Year = 2023 };
        context.Message.Returns(job);

        // Act
        await consumer.Consume(context);

        // Assert
        // Success if no exception
    }

    [Fact]
    public async Task Consume_ShouldLogAndRethrow_WhenExceptionOccurs()
    {
        // Arrange
        var logger = Substitute.For<ILogger<ReportJobConsumer>>();
        var consumer = new ReportJobConsumer(logger);
        var context = Substitute.For<ConsumeContext<ReportJob>>();
        
        var job = new ReportJob { JobId = Guid.NewGuid(), UserId = "user-failure" };
        context.Message.Returns(job);

        // Setup logger to throw
        // Setup logger to throw on the second call (inside try block)
        var callCount = 0;
        logger.When(x => x.Log(
                LogLevel.Information,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception?>(),
                Arg.Any<Func<object, Exception?, string>>()))
            .Do(x => 
            {
                callCount++;
                if (callCount == 2)
                {
                    throw new InvalidOperationException("Simulated Failure");
                }
            });

        // Act & Assert
        var ex = await Assert.ThrowsAsync<ApplicationException>(() => consumer.Consume(context));
        Assert.Equal("Failed to generate report", ex.Message);
        Assert.IsType<InvalidOperationException>(ex.InnerException);
    }
}
