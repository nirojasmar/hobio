using hobio.shared.Models;
using hobio.shared.Enums;
using MassTransit;
using Microsoft.Extensions.Logging;

namespace hobio.worker.Consumers;

public class ReportJobConsumer : IConsumer<ReportJob>
{
    private readonly ILogger<ReportJobConsumer> _logger;
    
    public ReportJobConsumer(ILogger<ReportJobConsumer> logger)
    {
        _logger = logger;
    }

    public async Task Consume(ConsumeContext<ReportJob> context)
    {
        var job = context.Message;
        _logger.LogInformation("Received Job: {JobId} for  User: {UserId}", job.JobId, job.UserId);

        try
        {
            _logger.LogInformation("Generating report for year {Year}", job.Year);
            await Task.Delay(2000);
            _logger.LogInformation("Report Generated Successfully");
        }
        catch (Exception e)
        {
            _logger.LogError(e, "Error Generating Report");
            throw;
        }
    }
}