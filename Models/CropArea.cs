namespace MedicalReportEditor.Models;

public class CropArea
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public double AspectRatio { get; set; } = 0; // 0 = Free, 1 = 1:1, 1.333 = 4:3, 1.777 = 16:9
}