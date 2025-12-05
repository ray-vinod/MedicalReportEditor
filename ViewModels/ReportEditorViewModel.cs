namespace MedicalReportEditor.ViewModels;

public partial class ReportEditorViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private ReportTemplate _currentTemplate;
    private readonly Stack<string> _undoStack = new();
    private readonly Stack<string> _redoStack = new();

    [ObservableProperty]
    private string _xmlContent = "";

    [ObservableProperty]
    private ObservableCollection<ReportTemplate> _templates = new();

    [ObservableProperty]
    private string _selectedTemplateName = "";

    [ObservableProperty]
    private bool _isDirty;

    [ObservableProperty]
    private bool _isValidXml = true;

    [ObservableProperty]
    private string _validationMessage = "";

    [ObservableProperty]
    private ObservableCollection<PaperSize> _paperSizes = new(PaperSize.GetStandardSizes());

    [ObservableProperty]
    private PaperSize _selectedPaperSize;

    public ObservableCollection<string> FontFamilies { get; } = new()
        {
            "Arial", "Calibri", "Cambria", "Times New Roman",
            "Segoe UI", "Bahnschrift Light"
        };

    public ReportEditorViewModel(IReportService reportService)
    {
        _reportService = reportService;
        _selectedPaperSize = _paperSizes.FirstOrDefault(p => p.Value == 9) ?? _paperSizes.First();
        LoadTemplates();
    }

    [RelayCommand]
    private async Task LoadTemplates()
    {
        try
        {
            var collection = await _reportService.LoadTemplatesAsync();
            Templates = new ObservableCollection<ReportTemplate>(collection.Reports);

            if (Templates.Any())
            {
                SelectedTemplateName = Templates.First().Name;
                LoadTemplate(Templates.First());
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading templates: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private void LoadTemplate(ReportTemplate template)
    {
        _currentTemplate = template;
        XmlContent = FormatXml(SerializeToXml(template));
        _undoStack.Clear();
        _redoStack.Clear();
        SaveUndoState();
        IsDirty = false;

        // Update selected paper size
        SelectedPaperSize = _paperSizes.FirstOrDefault(p => p.Value == template.Layout.PaperSize)
            ?? _paperSizes.First();
    }

    partial void OnSelectedTemplateNameChanged(string value)
    {
        if (string.IsNullOrEmpty(value)) return;

        var template = Templates.FirstOrDefault(t => t.Name == value);
        if (template != null)
        {
            LoadTemplate(template);
        }
    }

    partial void OnXmlContentChanged(string value)
    {
        if (_currentTemplate == null) return;

        IsDirty = true;
        ValidateXml();

        if (!string.IsNullOrEmpty(value))
        {
            SaveUndoState();
        }
    }

    partial void OnSelectedPaperSizeChanged(PaperSize value)
    {
        if (_currentTemplate == null || value == null) return;

        _currentTemplate.Layout.PaperSize = value.Value;
        IsDirty = true;
    }

    private void SaveUndoState()
    {
        _undoStack.Push(XmlContent);
        _redoStack.Clear();
    }

    [RelayCommand]
    private void Undo()
    {
        if (_undoStack.Count > 1)
        {
            _redoStack.Push(_undoStack.Pop());
            XmlContent = _undoStack.Peek();
            IsDirty = true;
        }
    }

    [RelayCommand]
    private void Redo()
    {
        if (_redoStack.Count > 0)
        {
            var state = _redoStack.Pop();
            _undoStack.Push(state);
            XmlContent = state;
            IsDirty = true;
        }
    }

    [RelayCommand]
    private async Task SaveTemplate()
    {
        if (_currentTemplate == null) return;

        try
        {
            if (!IsValidXml)
            {
                var result = MessageBox.Show(
                    "XML contains errors. Save anyway?",
                    "Validation Warning",
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Warning);

                if (result != MessageBoxResult.Yes) return;
            }

            var updatedTemplate = DeserializeFromXml(XmlContent);
            if (updatedTemplate != null)
            {
                var collection = await _reportService.LoadTemplatesAsync();
                var existingTemplate = collection.Reports.FirstOrDefault(t => t.Name == updatedTemplate.Name);

                if (existingTemplate != null)
                {
                    var index = collection.Reports.IndexOf(existingTemplate);
                    collection.Reports[index] = updatedTemplate;
                }

                await _reportService.SaveTemplatesAsync(collection);
                LoadTemplate(updatedTemplate);

                MessageBox.Show("Template saved successfully", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving template: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ValidateXml()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(XmlContent))
            {
                IsValidXml = true;
                ValidationMessage = "Empty document";
                return;
            }

            var doc = new XmlDocument();
            doc.LoadXml(XmlContent);

            IsValidXml = true;
            ValidationMessage = $"Valid XML - {doc.DocumentElement?.Name}";
        }
        catch (XmlException ex)
        {
            IsValidXml = false;
            ValidationMessage = $"Invalid XML: {ex.Message}";
        }
    }

    [RelayCommand]
    private void FormatXml()
    {
        try
        {
            XmlContent = FormatXml(XmlContent);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error formatting XML: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    private string FormatXml(string xml)
    {
        try
        {
            var doc = new XmlDocument();
            doc.LoadXml(xml);

            using var writer = new StringWriter();
            using var xmlWriter = XmlWriter.Create(writer, new XmlWriterSettings
            {
                Indent = true,
                IndentChars = "  ",
                NewLineChars = "\n",
                NewLineHandling = NewLineHandling.Replace,
                OmitXmlDeclaration = true
            });

            doc.Save(xmlWriter);
            return writer.ToString();
        }
        catch
        {
            return xml;
        }
    }

    private string SerializeToXml(ReportTemplate template)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ReportTemplate));
            using var writer = new StringWriter();
            var namespaces = new XmlSerializerNamespaces();
            namespaces.Add("", "");

            serializer.Serialize(writer, template, namespaces);
            return writer.ToString();
        }
        catch (Exception)
        {
            return "";
        }
    }

    private ReportTemplate DeserializeFromXml(string xml)
    {
        try
        {
            var serializer = new XmlSerializer(typeof(ReportTemplate));
            using var reader = new StringReader(xml);
            return (ReportTemplate)serializer.Deserialize(reader);
        }
        catch (Exception)
        {
            return null;
        }
    }
}