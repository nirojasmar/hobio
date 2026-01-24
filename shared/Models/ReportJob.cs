namespace hobio.shared.Models;

public class ReportJob
{
    public Guid JobId { get; set; } = Guid.NewGuid();
    public string UserId { get; set; }  = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public List<string> Sources { get; set; } = [];
    
    public int Year { get; set; } = DateTime.Now.Year;
    public int Month { get; set; } = DateTime.Now.Month;
    public int Day { get; set; } = DateTime.Now.Day;
    
    public string? Title { get; set; }
}