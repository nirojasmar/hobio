using hobio.shared.Models;
using hobio.worker.PdfLayouts;
using MassTransit;
using Microsoft.Extensions.Logging;
using hobio.worker.Services;
using QuestPDF.Fluent;

namespace hobio.worker.Consumers;

public class ReportJobConsumer : IConsumer<ReportJob>
{
    private readonly IStorageService _storageService;
    private readonly ILogger<ReportJobConsumer> _logger;
    
    public ReportJobConsumer(ILogger<ReportJobConsumer> logger, IStorageService storageService)
    {
        _logger = logger;
        _storageService = storageService;
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
            string fileName = $"report-{job.Year}_{Guid.NewGuid()}.pdf";
            
            await _storageService.UploadFileAsync(pdfBytes, fileName, "application/pdf");
            _logger.LogInformation("Report Generated Successfully: {FileName}", fileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Generating Report");
            throw new ApplicationException("Failed to generate report", e);
        }
    }
}