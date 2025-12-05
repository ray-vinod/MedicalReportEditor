namespace MedicalReportEditor.Models;

public class Layout
{
    [XmlElement("Width")]
    public int Width { get; set; } = 12390;

    [XmlElement("MarginLeft")]
    public double MarginLeft { get; set; } = 504;

    [XmlElement("MarginTop")]
    public double MarginTop { get; set; } = 403.2;

    [XmlElement("MarginRight")]
    public double MarginRight { get; set; } = 230.4;

    [XmlElement("MarginBottom")]
    public double MarginBottom { get; set; } = 144;

    [XmlElement("Orientation")]
    public int Orientation { get; set; } = 1; // 1=Portrait, 2=Landscape

    [XmlElement("PaperSize")]
    public int PaperSize { get; set; } = 9; // A4=9, Letter=1
}