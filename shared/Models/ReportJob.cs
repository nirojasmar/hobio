using Google.Cloud.Firestore;

namespace hobio.shared.Models;

[FirestoreData]
public class ReportJob
{
    [FirestoreProperty(ConverterType = typeof(GuidConverter))]
    public Guid JobId { get; set; } = Guid.NewGuid();

    [FirestoreProperty]
    public string UserId { get; set; }  = string.Empty;

    [FirestoreProperty]
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    [FirestoreProperty]
    public List<string> Sources { get; set; } = [];
    
    [FirestoreProperty]
    public int Year { get; set; } = DateTime.UtcNow.Year;

    [FirestoreProperty]
    public int Month { get; set; } = DateTime.UtcNow.Month;

    [FirestoreProperty]
    public int Day { get; set; } = DateTime.UtcNow.Day;
    
    [FirestoreProperty]
    public string? Title { get; set; }

    [FirestoreProperty]
    public string Status { get; set; } = "Pending";

    [FirestoreProperty]
    public string? StorageUrl { get; set; }
}

public class GuidConverter : IFirestoreConverter<Guid>
{
    public object ToFirestore(Guid value) => value.ToString();
    public Guid FromFirestore(object value) => value switch
    {
        string s when Guid.TryParse(s, out var g) => g,
        _ => throw new ArgumentException($"Cannot convert {value} to Guid")
    };
}