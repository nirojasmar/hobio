using hobio.shared.Models;

namespace hobio.tests.Models;

public class ReportStatusResponseTests
{
    [Fact]
    public void ReportStatusResponse_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var jobId = Guid.NewGuid();
        var status = "Completed";
        var downloadUrl = "http://example.com/report.pdf";
        var errorMessage = "None";

        // Act
        var response = new ReportStatusResponse(jobId, status, downloadUrl, errorMessage);

        // Assert
        Assert.Equal(jobId, response.JobId);
        Assert.Equal(status, response.Status);
        Assert.Equal(downloadUrl, response.DownloadUrl);
        Assert.Equal(errorMessage, response.ErrorMessage);
    }
}
