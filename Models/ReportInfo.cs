using System.Xml.Serialization;

namespace MedicalReportEditor.Models;
public class ReportInfo
{
    [XmlElement("Author")]
    public string Author { get; set; } = "System";
}