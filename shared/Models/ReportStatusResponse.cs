namespace hobio.shared.Models;

public record ReportStatusResponse(
    Guid JobId,
    string Status,
    string? DownloadUrl,
    string? ErrorMessage
);