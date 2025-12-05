using System.Xml.Serialization;

namespace MedicalReportEditor.Models;

public class ReportFont
{
    [XmlElement("Name")]
    public string Name { get; set; } = "Arial";

    [XmlElement("Size")]
    public double Size { get; set; } = 9;
}