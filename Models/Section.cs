namespace MedicalReportEditor.Models;

public class Section
{
    [XmlElement("Name")]
    public string Name { get; set; } = "";

    [XmlElement("Type")]
    public int Type { get; set; } // 0=Detail, 1=Header, 2=Footer, 3=PageHeader, 4=PageFooter

    [XmlElement("Height")]
    public double Height { get; set; } = 0;

    [XmlElement("Visible")]
    public int Visible { get; set; } = 1; // 0=false, 1=true
}