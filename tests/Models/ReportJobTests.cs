using hobio.shared.Models;

namespace hobio.tests.Models;

public class ReportJobTests
{
    [Fact]
    public void ReportJob_ShouldHaveCorrectProperties()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var userId = "user1";
        var year = 2023;
        var sources = new List<string> { "Steam" };

        // Act
        var job = new ReportJob
        {
            JobId = jobId,
            UserId = userId,
            Year = year,
            Sources = sources,
            Month = 10,
            Day = 15,
            Title = "My Report"
        };

        // Assert
        Assert.Equal(jobId, job.JobId);
        Assert.Equal(userId, job.UserId);
        Assert.Equal(year, job.Year);
        Assert.Equal(sources, job.Sources);
        Assert.Equal(10, job.Month);
        Assert.Equal(15, job.Day);
        Assert.Equal("My Report", job.Title);
        
        // Check defaults are valid (CreatedAt/UpdatedAt)
        Assert.True(job.CreatedAt <= DateTime.UtcNow);
        Assert.True(job.UpdatedAt <= DateTime.UtcNow);
    }
}
