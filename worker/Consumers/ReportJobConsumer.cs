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
    private readonly IJobStatusService _jobStatusService;
    private readonly ILogger<ReportJobConsumer> _logger;
    
    public ReportJobConsumer(ILogger<ReportJobConsumer> logger, IStorageService storageService, IJobStatusService jobStatusService)
    {
        _logger = logger;
        _storageService = storageService;
        _jobStatusService = jobStatusService;
    }

    public async Task Consume(ConsumeContext<ReportJob> context)
    {
        var job = context.Message;
        _logger.LogInformation("Received Job: {JobId} for User: {UserId}", job.JobId, job.UserId);

        await _jobStatusService.SetProcessingAsync(job.JobId);
        _logger.LogInformation("Job {JobId} status set to Processing", job.JobId);

        var document = new ReportDocument(context.Message);

        try
        {
            _logger.LogInformation("Generating report for year {Year}", job.Year);
            byte[] pdfBytes = document.GeneratePdf();
            string fileName = $"report-{job.Year}_{Guid.NewGuid()}.pdf";
            
            await _storageService.UploadFileAsync(pdfBytes, fileName, "application/pdf");
            _logger.LogInformation("Report Generated Successfully: {FileName}", fileName);

            await _jobStatusService.SetCompletedAsync(job.JobId, fileName);
            _logger.LogInformation("Job {JobId} status set to Completed, StorageUrl: {FileName}", job.JobId, fileName);
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Generating Report for Job {JobId}", job.JobId);

            await _jobStatusService.SetFailedAsync(job.JobId);
            _logger.LogInformation("Job {JobId} status set to Failed", job.JobId);

            throw new InvalidOperationException("Failed to generate report", e);
        }
    }
}
