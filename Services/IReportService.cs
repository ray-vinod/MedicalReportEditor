namespace MedicalReportEditor.Services;

public interface IReportService
{
    Task<ReportCollection> LoadTemplatesAsync();
    Task SaveTemplatesAsync(ReportCollection templates);
    Task<ReportTemplate> CreateTemplate(string name, ReportType type);
    Task<bool> DeleteTemplate(string name);
    Task<ReportTemplate> DuplicateTemplate(string sourceName, string newName);
    Task<bool> TemplateExists(string name);
    Task<List<ReportTemplate>> GetTemplatesByType(ReportType type);
}