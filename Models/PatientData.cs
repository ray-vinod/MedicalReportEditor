namespace MedicalReportEditor.Models;

public class PatientData
{
    public string PatientId { get; set; } = "";
    public string PatientName { get; set; } = "";
    public string AgeGender { get; set; } = "";
    public DateTime VisitDate { get; set; } = DateTime.Now;
    public string ReferredBy { get; set; } = "";
    public string ConsultedBy { get; set; } = "";
    public string ProcedureType { get; set; } = "Upper GI Endoscopy";
    public string Premedication { get; set; } = "";
    public string Instrument { get; set; } = "";
    public Dictionary<string, string> Findings { get; set; } = new();
    public string Impression { get; set; } = "";
    public List<string> Images { get; set; } = new();
    public bool BiopsyTaken { get; set; } = false;
    public string BiopsyResult { get; set; } = "";
}