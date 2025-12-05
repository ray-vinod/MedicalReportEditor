namespace MedicalReportEditor.Models;

public class ReportField
{
    [XmlElement("Name")]
    public string Name { get; set; } = "";

    [XmlElement("Section")]
    public int Section { get; set; } = 0;

    [XmlElement("Text")]
    public string Text { get; set; } = "";

    [XmlElement("Left")]
    public double Left { get; set; } = 0;

    [XmlElement("Top")]
    public double Top { get; set; } = 0;

    [XmlElement("Width")]
    public double Width { get; set; } = 100;

    [XmlElement("Height")]
    public double Height { get; set; } = 50;

    [XmlElement("Align")]
    public int Align { get; set; } = 0; // 0=Left, 1=Center, 2=Right

    [XmlElement("Font")]
    public FieldFont Font { get; set; } = new();

    [XmlElement("CanGrow")]
    public int CanGrow { get; set; } = 0;

    [XmlElement("CanShrink")]
    public int CanShrink { get; set; } = 0;

    [XmlElement("Visible")]
    public int Visible { get; set; } = 1;

    [XmlElement("ForeColor")]
    public int ForeColor { get; set; } = -16777216; // Black

    [XmlElement("BackColor")]
    public int BackColor { get; set; } = -1; // Transparent
}