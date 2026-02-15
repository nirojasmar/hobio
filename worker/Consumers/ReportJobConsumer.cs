using hobio.shared.Models;
using hobio.worker.PdfLayouts;
using MassTransit;
using Microsoft.Extensions.Logging;
using hobio.worker.Services;
using QuestPDF.Fluent;

namespace hobio.worker.Consumers;

public class ReportJobConsumer : IConsumer<ReportJob>
{
    private readonly ILogger<ReportJobConsumer> _logger;
    private readonly IFileService _fileService;
    
    public ReportJobConsumer(ILogger<ReportJobConsumer> logger, IFileService fileService)
    {
        _logger = logger;
        _fileService = fileService;
    }

    public async Task Consume(ConsumeContext<ReportJob> context)
    {
        var job = context.Message;
        _logger.LogInformation("Received Job: {JobId} for  User: {UserId}", job.JobId, job.UserId);
        
        var document = new ReportDocument(context.Message);

        try
        {
            _logger.LogInformation("Generating report for year {Year}", job.Year);
            byte[] pdfBytes = document.GeneratePdf();
            
            var fileName = $"report-{job.Year}.pdf";
            var filePath = Path.Combine("/tmp", fileName);
            await _fileService.WriteAllBytesAsync(filePath, pdfBytes);
            
            _logger.LogInformation("Report Generated Successfully at: {FilePath}", filePath);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Generating Report");
            throw new ApplicationException("Failed to generate report", e);
        }
    }
}