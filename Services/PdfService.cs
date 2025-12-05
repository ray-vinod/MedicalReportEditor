using MedicalReportEditor.Models;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.IO;
using System.Text;

namespace MedicalReportEditor.Services;

public class PdfService : IPdfService
{
    private readonly ILogger<PdfService> _logger;

    public PdfService(ILogger<PdfService> logger)
    {
        _logger = logger;
        QuestPDF.Settings.License = LicenseType.Professional;
    }

    public async Task<string> GenerateReportPreview(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData)
    {
        return await Task.Run(() =>
        {
            var tempPath = Path.GetTempFileName();
            tempPath = Path.ChangeExtension(tempPath, ".pdf");

            try
            {
                var document = CreateDocument(template, hospitalInfo, patientData);
                document.GeneratePdf(tempPath);

                _logger.LogInformation("PDF preview generated: {Path}", tempPath);
                return tempPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF preview");
                throw;
            }
        });
    }

    public async Task<string> ExportReport(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData, string outputPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var document = CreateDocument(template, hospitalInfo, patientData);
                document.GeneratePdf(outputPath);

                _logger.LogInformation("Report exported: {Path}", outputPath);
                return outputPath;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error exporting report");
                throw;
            }
        });
    }

    public async Task<string> GenerateHtmlPreview(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData)
    {
        return await Task.Run(() =>
        {
            try
            {
                var document = CreateDocument(template, hospitalInfo, patientData);

                // Generate PDF as byte array first
                using var pdfStream = new MemoryStream();
                document.GeneratePdf(pdfStream);
                pdfStream.Position = 0;

                // Convert PDF pages to images (requires additional libraries)
                return GenerateHtmlFromPdf(pdfStream.ToArray());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating HTML preview");
                return string.Empty;
            }
        });
    }

    private string GenerateHtmlFromPdf(byte[] pdfBytes)
    {
        var html = new StringBuilder();
        html.AppendLine("<!DOCTYPE html>");
        html.AppendLine("<html lang='en'>");
        html.AppendLine("<head>");
        html.AppendLine("    <meta charset='UTF-8'>");
        html.AppendLine("    <meta name='viewport' content='width=device-width, initial-scale=1.0'>");
        html.AppendLine("    <title>Report Preview</title>");
        html.AppendLine("    <style>");
        html.AppendLine("        body { font-family: Arial, sans-serif; margin: 20px; }");
        html.AppendLine("        .page { margin-bottom: 30px; page-break-after: always; }");
        html.AppendLine("        .warning { color: #666; font-style: italic; padding: 20px; border: 1px dashed #ccc; }");
        html.AppendLine("    </style>");
        html.AppendLine("</head>");
        html.AppendLine("<body>");
        html.AppendLine("    <div class='page'>");
        html.AppendLine("        <div class='warning'>");
        html.AppendLine("            <h3>PDF Preview Note</h3>");
        html.AppendLine("            <p>This is a preview of the PDF content. The actual PDF generation uses QuestPDF.</p>");
        html.AppendLine("            <p>To view the complete document with proper formatting, please export as PDF.</p>");
        html.AppendLine("        </div>");
        html.AppendLine("    </div>");
        html.AppendLine("</body>");
        html.AppendLine("</html>");

        return html.ToString();
    }

    private IDocument CreateDocument(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData)
    {
        return Document.Create(container =>
        {
            container.Page(page =>
            {
                // Set page size based on template
                var paperSize = GetPaperSize(template.Layout.PaperSize);
                page.Size(paperSize.width, paperSize.height, Unit.Inch);
                page.Margin((float)(template.Layout.MarginLeft / 100), Unit.Inch);

                page.DefaultTextStyle(TextStyle.Default
                    .FontFamily(template.Font.Name)
                    .FontSize((float)template.Font.Size));

                // Header
                page.Header().Element(comp => BuildHeader(comp, hospitalInfo, patientData));

                // Content
                page.Content().Element(comp => BuildContent(comp, template, patientData));

                // Footer
                page.Footer().Element(comp => BuildFooter(comp, hospitalInfo));
            });
        });
    }

    private (float width, float height) GetPaperSize(int paperSizeCode)
    {
        return paperSizeCode switch
        {
            1 => (8.5f, 11f),      // Letter
            5 => (8.5f, 14f),      // Legal
            8 => (11.7f, 16.5f),   // A3
            9 => (8.27f, 11.69f),  // A4
            11 => (5.83f, 8.27f),  // A5
            _ => (8.27f, 11.69f)   // Default A4
        };
    }

    private void BuildHeader(IContainer container, HospitalInfo hospitalInfo, PatientData patientData)
    {
        container.Column(column =>
        {
            // Hospital Name
            column.Item().AlignCenter().Text(hospitalInfo.HospitalName)
                .FontSize(16).Bold().FontColor(Colors.Blue.Darken3);

            // Hospital Address
            if (!string.IsNullOrEmpty(hospitalInfo.HospitalAddress))
            {
                column.Item().AlignCenter().Text(hospitalInfo.HospitalAddress)
                    .FontSize(10).FontColor(Colors.Grey.Darken2);
            }

            // Contact Info
            var contactInfo = $"{hospitalInfo.ContactNumber} | {hospitalInfo.EmailAddress} | {hospitalInfo.Website}";
            column.Item().AlignCenter().Text(contactInfo)
                .FontSize(8).FontColor(Colors.Grey.Darken2);

            column.Item().PaddingVertical(5).LineHorizontal(1).LineColor(Colors.Grey.Medium);

            // Patient Information
            column.Item().Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                    columns.ConstantColumn(80);
                    columns.RelativeColumn();
                });

                AddTableRow(table, "Patient ID:", patientData.PatientId);
                AddTableRow(table, "Patient Name:", patientData.PatientName);
                AddTableRow(table, "Age/Gender:", patientData.AgeGender);
                AddTableRow(table, "Visit Date:", patientData.VisitDate.ToString("dd/MM/yyyy"));
                AddTableRow(table, "Referred by:", patientData.ReferredBy);
                AddTableRow(table, "Consulted by:", patientData.ConsultedBy);
                AddTableRow(table, "Procedure:", patientData.ProcedureType);
            });

            column.Item().PaddingVertical(10).LineHorizontal(1).LineColor(Colors.Grey.Medium);
        });
    }

    private void BuildContent(IContainer container, ReportTemplate template, PatientData patientData)
    {
        container.Column(column =>
        {
            // Procedure Title
            column.Item().AlignCenter().Text(patientData.ProcedureType)
                .FontSize(14).Bold().Underline();

            column.Item().PaddingTop(20);

            // Findings
            if (patientData.Findings.Any())
            {
                column.Item().Text("Findings:").FontSize(12).Bold();
                column.Item().PaddingLeft(20);

                foreach (var finding in patientData.Findings)
                {
                    column.Item().PaddingVertical(2).Text($"{finding.Key}: {finding.Value}");
                }
            }

            // Premedication
            if (!string.IsNullOrEmpty(patientData.Premedication))
            {
                column.Item().PaddingTop(10).Text($"Premedication: {patientData.Premedication}");
            }

            // Instrument
            if (!string.IsNullOrEmpty(patientData.Instrument))
            {
                column.Item().PaddingTop(5).Text($"Instrument: {patientData.Instrument}");
            }

            // Biopsy
            if (patientData.BiopsyTaken)
            {
                column.Item().PaddingTop(10).Text($"Biopsy: {patientData.BiopsyResult}");
            }

            // Impression
            if (!string.IsNullOrEmpty(patientData.Impression))
            {
                column.Item().PaddingTop(20).Text("Impression:").FontSize(12).Bold();
                column.Item().PaddingLeft(20).Text(patientData.Impression);
            }

            // Images
            if (patientData.Images.Any())
            {
                column.Item().PaddingTop(30).Text("Images:").FontSize(12).Bold();

                // Simple image grid (2 per row)
                for (int i = 0; i < patientData.Images.Count; i += 2)
                {
                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Padding(5).Image(patientData.Images[i]);

                        if (i + 1 < patientData.Images.Count)
                        {
                            row.RelativeItem().Padding(5).Image(patientData.Images[i + 1]);
                        }
                    });
                }
            }
        });
    }

    private void BuildFooter(IContainer container, HospitalInfo hospitalInfo)
    {
        container.Column(column =>
        {
            column.Item().PaddingTop(10).LineHorizontal(1).LineColor(Colors.Grey.Lighten1);

            column.Item().PaddingTop(5).Row(row =>
            {
                row.RelativeItem().Text($"Generated: {DateTime.Now:dd/MM/yyyy HH:mm}")
                    .FontSize(8).FontColor(Colors.Grey.Darken1);

                row.RelativeItem().AlignRight().Text(hospitalInfo.HospitalName)
                    .FontSize(8).FontColor(Colors.Grey.Darken1);
            });
        });
    }

    private void AddTableRow(TableDescriptor table, string label, string value)
    {
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(2)
            .Text(label).Bold();
        table.Cell().BorderBottom(1).BorderColor(Colors.Grey.Lighten2).PaddingBottom(2)
            .Text(value);
    }

    public async Task<byte[]> GeneratePdfBytes(ReportTemplate template, HospitalInfo hospitalInfo, PatientData patientData)
    {
        return await Task.Run(() =>
        {
            try
            {
                var document = CreateDocument(template, hospitalInfo, patientData);
                using var stream = new MemoryStream();
                document.GeneratePdf(stream);
                return stream.ToArray();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating PDF bytes");
                throw;
            }
        });
    }
}