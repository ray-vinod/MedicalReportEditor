using MedicalReportEditor.Enums;
using MedicalReportEditor.Models;
using Microsoft.Extensions.Logging;
using System.Collections.ObjectModel;
using System.IO;
using System.Xml;
using System.Xml.Serialization;

namespace MedicalReportEditor.Services;

public class ReportService : IReportService
{
    private readonly ILogger<ReportService> _logger;
    private const string TemplatePath = "DefaultReports.xml";

    public ReportService(ILogger<ReportService> logger)
    {
        _logger = logger;
    }

    public async Task<ReportCollection> LoadTemplatesAsync()
    {
        try
        {
            if (!File.Exists(TemplatePath))
            {
                _logger.LogInformation("Creating default template file");
                return await CreateDefaultTemplatesAsync();
            }

            var serializer = new XmlSerializer(typeof(ReportCollection));
            await using var stream = File.OpenRead(TemplatePath);
            using var reader = XmlReader.Create(stream);

            if (serializer.CanDeserialize(reader))
            {
                return (ReportCollection)serializer.Deserialize(reader)!;
            }

            return new ReportCollection();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates");
            throw;
        }
    }

    public async Task SaveTemplatesAsync(ReportCollection templates)
    {
        try
        {
            // Backup existing file
            if (File.Exists(TemplatePath))
            {
                var backupPath = $"{TemplatePath}.backup_{DateTime.Now:yyyyMMdd_HHmmss}";
                File.Copy(TemplatePath, backupPath, true);
            }

            var serializer = new XmlSerializer(typeof(ReportCollection));
            var settings = new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                Encoding = System.Text.Encoding.UTF8
            };

            await using var stream = new FileStream(TemplatePath, FileMode.Create, FileAccess.Write, FileShare.None);
            using var writer = XmlWriter.Create(stream, settings);

            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            serializer.Serialize(writer, templates, namespaces);

            _logger.LogInformation("Templates saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving templates");
            throw;
        }
    }

    public async Task<ReportTemplate> CreateTemplate(string name, ReportType type)
    {
        var templates = await LoadTemplatesAsync();

        if (templates.Reports.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Template '{name}' already exists");
        }

        var newTemplate = CreateDefaultTemplate(name, type);
        templates.Reports.Add(newTemplate);

        await SaveTemplatesAsync(templates);
        _logger.LogInformation("Created new template: {Name}", name);

        return newTemplate;
    }

    public async Task<bool> DeleteTemplate(string name)
    {
        var templates = await LoadTemplatesAsync();
        var template = templates.Reports.FirstOrDefault(t => t.Name == name);

        if (template == null)
        {
            return false;
        }

        templates.Reports.Remove(template);
        await SaveTemplatesAsync(templates);

        _logger.LogInformation("Deleted template: {Name}", name);
        return true;
    }

    public async Task<ReportTemplate> DuplicateTemplate(string sourceName, string newName)
    {
        var templates = await LoadTemplatesAsync();
        var sourceTemplate = templates.Reports.FirstOrDefault(t => t.Name == sourceName);

        if (sourceTemplate == null)
        {
            throw new ArgumentException($"Template '{sourceName}' not found");
        }

        if (templates.Reports.Any(t => t.Name.Equals(newName, StringComparison.OrdinalIgnoreCase)))
        {
            throw new InvalidOperationException($"Template '{newName}' already exists");
        }

        // Deep clone using XML serialization
        var serializer = new XmlSerializer(typeof(ReportTemplate));
        using var stream = new MemoryStream();
        serializer.Serialize(stream, sourceTemplate);
        stream.Position = 0;

        var clonedTemplate = (ReportTemplate)serializer.Deserialize(stream)!;
        clonedTemplate.Name = newName;
        clonedTemplate.ReportInfo.Author = "Duplicated from " + sourceName;

        // Update field names to avoid conflicts
        foreach (var field in clonedTemplate.Fields)
        {
            if (!string.IsNullOrEmpty(field.Name))
            {
                field.Name = field.Name + "_copy";
            }
        }

        templates.Reports.Add(clonedTemplate);
        await SaveTemplatesAsync(templates);

        return clonedTemplate;
    }

    public async Task<bool> TemplateExists(string name)
    {
        var templates = await LoadTemplatesAsync();
        return templates.Reports.Any(t => t.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<List<ReportTemplate>> GetTemplatesByType(ReportType type)
    {
        var templates = await LoadTemplatesAsync();
        return templates.Reports
            .Where(t => t.Type == type)
            .ToList();
    }

    private async Task<ReportCollection> CreateDefaultTemplatesAsync()
    {
        var collection = new ReportCollection
        {
            Reports = new ObservableCollection<ReportTemplate>
                {
                    CreateDefaultTemplate("UpperGIEndoscopy6Images2pages-new", ReportType.UpperGIEndoscopy),
                    CreateDefaultTemplate("Colonoscopy6Images2pages-new", ReportType.Colonoscopy),
                    CreateDefaultTemplate("ERCP5Images", ReportType.ERCP),
                    CreateDefaultTemplate("Sigmoidoscopy", ReportType.Sigmoidoscopy),
                    CreateDefaultTemplate("Bronchoscopy", ReportType.Bronchoscopy)
                }
        };

        await SaveTemplatesAsync(collection);
        return collection;
    }

    private ReportTemplate CreateDefaultTemplate(string name, ReportType type)
    {
        return new ReportTemplate
        {
            Name = name,
            Type = type,
            ReportInfo = new ReportInfo { Author = "System" },
            Layout = new Layout
            {
                Width = 12390,
                MarginLeft = 504,
                MarginTop = 403.2,
                MarginRight = 230.4,
                MarginBottom = 144,
                Orientation = 1,
                PaperSize = 9 // A4
            },
            Font = new ReportFont { Name = "Arial", Size = 9 },
            Sections = new ObservableCollection<Section>
                {
                    new() { Name = "Detail", Type = 0, Height = 25065 },
                    new() { Name = "Header", Type = 1 },
                    new() { Name = "Footer", Type = 2, Visible = 0 },
                    new() { Name = "PageHeader", Type = 3, Height = 2325 },
                    new() { Name = "PageFooter", Type = 4, Height = 2625 }
                },
            Fields = CreateDefaultFields(type)
        };
    }

    private ObservableCollection<ReportField> CreateDefaultFields(ReportType type)
    {
        var fields = new ObservableCollection<ReportField>
            {
                // Hospital Header
                new()
                {
                    Name = "textHospitalName",
                    Section = 3,
                    Text = "CIWEC HOSPITAL PVT. LTD.",
                    Left = 2985,
                    Top = 30,
                    Width = 6300,
                    Height = 540,
                    Align = 1, // Center
                    Font = new FieldFont
                    {
                        Name = "Calibri",
                        Size = 22,
                        Bold = -1
                    },
                    ForeColor = -16776961 // Blue
                }
            };

        // Add type-specific fields
        switch (type)
        {
            case ReportType.UpperGIEndoscopy:
                fields.Add(CreateField("textProcedure", 0, "Upper GI Endoscopy", 1920, 0, 6330, 450, 7));
                fields.Add(CreateField("LabelField1", 0, "Esophagus", 840, 1087.5, 1605, 450, 0));
                fields.Add(CreateField("LabelField7", 0, "Stomach", 840, 4365, 1560, 450, 0));
                fields.Add(CreateField("LabelField12", 0, "Duodenum", 840, 7260, 1560, 450, 0));
                break;

            case ReportType.Colonoscopy:
                fields.Add(CreateField("textProcedure", 0, "Colonoscopy", 1770, 0, 6330, 450, 7));
                fields.Add(CreateField("LabelField1", 0, "P/R", 690, 1119, 1920, 450, 0));
                fields.Add(CreateField("LabelField4", 0, "Rectum", 690, 2856, 1920, 450, 0));
                fields.Add(CreateField("LabelField5", 0, "Sigmoid Colon", 690, 3436, 1920, 450, 0));
                break;

            case ReportType.ERCP:
                fields.Add(CreateField("textProcedure", 0, "ERCP", 1140, 0, 6330, 450, 7));
                fields.Add(CreateField("LabelField1", 0, "Scope used", 15, 1107, 2025, 450, 0));
                fields.Add(CreateField("LabelField2", 0, "Papilla", 15, 1674, 2025, 450, 0));
                fields.Add(CreateField("LabelField3", 0, "Pancreatic Duct", 15, 2241, 2025, 450, 0));
                break;
        }

        return fields;
    }

    private ReportField CreateField(string name, int section, string text, double left, double top, double width, double height, int align)
    {
        return new ReportField
        {
            Name = name,
            Section = section,
            Text = text,
            Left = left,
            Top = top,
            Width = width,
            Height = height,
            Align = align,
            Font = new FieldFont { Name = "Cambria", Size = 11 }
        };
    }
}