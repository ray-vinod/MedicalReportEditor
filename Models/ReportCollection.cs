using System.Collections.ObjectModel;
using System.Xml.Serialization;

namespace MedicalReportEditor.Models;

[XmlRoot("Reports")]
public class ReportCollection
{
    [XmlElement("Report")]
    public ObservableCollection<ReportTemplate> Reports { get; set; } = new();
}