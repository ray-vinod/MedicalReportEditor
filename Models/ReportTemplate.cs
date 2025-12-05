namespace MedicalReportEditor.Models;

public class ReportTemplate
{
    [XmlAttribute("version")]
    public string Version { get; set; } = "2.6.20092.52106";

    [XmlElement("Name")]
    public string Name { get; set; } = "New Template";

    [XmlElement("ReportInfo")]
    public ReportInfo ReportInfo { get; set; } = new();

    [XmlElement("DataSource")]
    public string DataSource { get; set; } = "";

    [XmlElement("Layout")]
    public Layout Layout { get; set; } = new();

    [XmlElement("Font")]
    public ReportFont Font { get; set; } = new();

    [XmlArray("Sections")]
    [XmlArrayItem("Section")]
    public ObservableCollection<Section> Sections { get; set; } = new();

    [XmlArray("Fields")]
    [XmlArrayItem("Field")]
    public ObservableCollection<ReportField> Fields { get; set; } = new();

    [XmlIgnore]
    public ReportType Type { get; set; } = ReportType.UpperGIEndoscopy;
}