namespace MedicalReportEditor.Models;

public class PaperSize
{
    public string Name { get; set; } = string.Empty;
    public int Value { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }

    public static List<PaperSize> GetStandardSizes()
    {
        return new List<PaperSize>
            {
                new() { Name = "Letter (8.5x11 in)", Value = 1, Width = 8.5, Height = 11 },
                new() { Name = "Legal (8.5x14 in)", Value = 5, Width = 8.5, Height = 14 },
                new() { Name = "A4 (8.27x11.69 in)", Value = 9, Width = 8.27, Height = 11.69 }
            };
    }
}