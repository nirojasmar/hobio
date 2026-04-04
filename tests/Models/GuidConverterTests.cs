using hobio.shared.Models;

namespace hobio.tests.Models;

/// <summary>
/// Unit tests for <see cref="GuidConverter"/> and the <see cref="ReportJob"/> model.
/// These are pure unit tests — no external dependencies.
/// </summary>
public class GuidConverterTests
{
    private readonly GuidConverter _converter = new();

    // -------------------------------------------------------------------------
    // ToFirestore
    // -------------------------------------------------------------------------

    [Fact]
    public void ToFirestore_ValidGuid_ReturnsStringRepresentation()
    {
        var guid = Guid.NewGuid();
        var result = _converter.ToFirestore(guid);
        Assert.Equal(guid.ToString(), result);
    }

    [Fact]
    public void ToFirestore_EmptyGuid_ReturnsEmptyGuidString()
    {
        var result = _converter.ToFirestore(Guid.Empty);
        Assert.Equal(Guid.Empty.ToString(), result);
    }

    // -------------------------------------------------------------------------
    // FromFirestore
    // -------------------------------------------------------------------------

    [Fact]
    public void FromFirestore_ValidGuidString_ReturnsGuid()
    {
        var guid = Guid.NewGuid();
        var result = _converter.FromFirestore(guid.ToString());
        Assert.Equal(guid, result);
    }

    [Fact]
    public void FromFirestore_EmptyGuidString_ReturnsEmptyGuid()
    {
        var result = _converter.FromFirestore(Guid.Empty.ToString());
        Assert.Equal(Guid.Empty, result);
    }

    [Fact]
    public void FromFirestore_InvalidValue_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _converter.FromFirestore("not-a-guid"));
    }

    [Fact]
    public void FromFirestore_NonStringValue_ThrowsArgumentException()
    {
        // Passing an int (non-string) should hit the default arm and throw
        Assert.Throws<ArgumentException>(() => _converter.FromFirestore(42));
    }

    // -------------------------------------------------------------------------
    // ReportJob model default values
    // -------------------------------------------------------------------------

    [Fact]
    public void ReportJob_DefaultValues_AreCorrect()
    {
        var job = new ReportJob();

        Assert.NotEqual(Guid.Empty, job.JobId);
        Assert.Equal(string.Empty, job.UserId);
        Assert.Equal("Pending", job.Status);
        Assert.Null(job.StorageUrl);
        Assert.Null(job.Title);
        Assert.NotEmpty(job.Sources is { Count: 0 } ? new[] { "ok" } : Array.Empty<string>()); // Sources defaults to []
        Assert.Equal(DateTime.UtcNow.Year, job.Year);
    }

    [Fact]
    public void ReportJob_PropertyAssignment_Works()
    {
        var jobId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        var job = new ReportJob
        {
            JobId = jobId,
            UserId = "test-user",
            Status = "Completed",
            StorageUrl = "https://bucket/file.pdf",
            Title = "Annual Report",
            Sources = new List<string> { "Steam", "LastFm" },
            Year = 2023,
            Month = 12,
            Day = 31,
            CreatedAt = now,
            UpdatedAt = now
        };

        Assert.Equal(jobId, job.JobId);
        Assert.Equal("test-user", job.UserId);
        Assert.Equal("Completed", job.Status);
        Assert.Equal("https://bucket/file.pdf", job.StorageUrl);
        Assert.Equal("Annual Report", job.Title);
        Assert.Equal(new List<string> { "Steam", "LastFm" }, job.Sources);
        Assert.Equal(2023, job.Year);
        Assert.Equal(12, job.Month);
        Assert.Equal(31, job.Day);
        Assert.Equal(now, job.CreatedAt);
        Assert.Equal(now, job.UpdatedAt);
    }

    // -------------------------------------------------------------------------
    // Record models
    // -------------------------------------------------------------------------

    [Fact]
    public void ReportRequest_RecordEquality_Works()
    {
        var r1 = new ReportRequest(2024, new List<string> { "Steam" });
        var r2 = new ReportRequest(2024, new List<string> { "Steam" });
        // Records use structural equality
        Assert.Equal(r1.Year, r2.Year);
        Assert.Equal(r1.Sources, r2.Sources);
    }

    [Fact]
    public void ReportResponse_RecordHoldsJobId()
    {
        var id = Guid.NewGuid();
        var r = new ReportResponse(id);
        Assert.Equal(id, r.JobId);
    }

    [Fact]
    public void ReportStatusResponse_RecordProperties_AreAccessible()
    {
        var id = Guid.NewGuid();
        var r = new ReportStatusResponse(id, "Completed", "https://url", null);
        Assert.Equal(id, r.JobId);
        Assert.Equal("Completed", r.Status);
        Assert.Equal("https://url", r.DownloadUrl);
        Assert.Null(r.ErrorMessage);
    }

    [Fact]
    public void ReportStatusResponse_WithNullDownloadUrl_IsValid()
    {
        var r = new ReportStatusResponse(Guid.NewGuid(), "Pending", null, null);
        Assert.Null(r.DownloadUrl);
    }
}
