using hobio.shared.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace hobio.worker.PdfLayouts;

public class ReportDocument : IDocument
{
    private readonly ReportJob _reportJob;
    public ReportDocument(ReportJob reportJob)
    {
        _reportJob = reportJob;
    }
    
    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Margin(1, Unit.Centimetre);
            page.PageColor(Colors.White);
            page.DefaultTextStyle(x => x.FontSize(12).FontFamily(Fonts.Verdana));

            page.Header().Row(row =>
            {
                row.RelativeItem().Text("HOBIO - Report")
                    .FontSize(20).SemiBold().FontColor(Colors.Blue.Medium);
                
                row.ConstantItem(100).Text($"{DateTime.Now:d}");
            });

            page.Content().PaddingVertical(1, Unit.Centimetre).Column(col =>
            {
                col.Spacing(5);
                col.Item().Text($"Year: {_reportJob.Year}").Bold();
                col.Item().Text("Analyzed Sources:").Underline();

                foreach (var source in _reportJob.Sources)
                {
                    col.Item().Text($"- {source}");
                }

                col.Item().PaddingTop(10).Element(PlaceholderBlock);
            });

            page.Footer().AlignCenter().Text(x =>
            {
                x.Span("Page ");
                x.CurrentPageNumber();
            });
        });
    }
    
    void PlaceholderBlock(IContainer container)
    {
        container.Background(Colors.Grey.Lighten3).Height(50).AlignCenter().AlignMiddle().Text("WIP Graphics");
    }
}