namespace MedicalReportEditor.Services;

public interface IPdfService
{
    Task<string> GenerateReportPreview(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData);
    Task<string> ExportReport(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData, string outputPath);
    Task<byte[]> GeneratePdfBytes(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData);
    Task<string> GenerateHtmlPreview(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData);
}