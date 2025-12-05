using System.Xml.Serialization;

namespace MedicalReportEditor.Models;

public class FieldFont
{
    [XmlElement("Name")]
    public string Name { get; set; } = "Cambria";

    [XmlElement("Size")]
    public double Size { get; set; } = 11;

    [XmlElement("Bold")]
    public int Bold { get; set; } = 0;

    [XmlElement("Italic")]
    public int Italic { get; set; } = 0;

    [XmlElement("Underline")]
    public int Underline { get; set; } = 0;
}