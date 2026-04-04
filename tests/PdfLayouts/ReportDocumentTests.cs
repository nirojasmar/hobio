using hobio.shared.Models;
using hobio.worker.PdfLayouts;
using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace hobio.tests.PdfLayouts;

/// <summary>
/// Unit tests for <see cref="ReportDocument"/>.
/// GeneratePdf() is a QuestPDF extension method available on IDocument via QuestPDF.Fluent.
/// QuestPDF Community licence is set in the constructor.
/// </summary>
public class ReportDocumentTests
{
    public ReportDocumentTests()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    private static ReportJob MakeJob(int year = 2024, List<string>? sources = null) => new()
    {
        JobId = Guid.NewGuid(),
        UserId = "test-user",
        Year = year,
        Sources = sources ?? new List<string> { "Steam", "LastFm" },
        Status = "Pending"
    };

    [Fact]
    public void GeneratePdf_ReturnsNonEmptyByteArray()
    {
        IDocument doc = new ReportDocument(MakeJob());
        var bytes = doc.GeneratePdf();
        Assert.NotNull(bytes);
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void GeneratePdf_OutputStartsWithPdfMagicBytes()
    {
        IDocument doc = new ReportDocument(MakeJob());
        var bytes = doc.GeneratePdf();
        // PDF files always start with %PDF
        Assert.Equal((byte)'%', bytes[0]);
        Assert.Equal((byte)'P', bytes[1]);
        Assert.Equal((byte)'D', bytes[2]);
        Assert.Equal((byte)'F', bytes[3]);
    }

    [Fact]
    public void GeneratePdf_WithNoSources_StillProducesValidPdf()
    {
        IDocument doc = new ReportDocument(MakeJob(sources: new List<string>()));
        var bytes = doc.GeneratePdf();
        Assert.True(bytes.Length > 0);
    }

    [Fact]
    public void GeneratePdf_WithManySources_StillProducesValidPdf()
    {
        var sources = Enumerable.Range(1, 20).Select(i => $"Source{i}").ToList();
        IDocument doc = new ReportDocument(MakeJob(sources: sources));
        var bytes = doc.GeneratePdf();
        Assert.True(bytes.Length > 0);
    }
}
