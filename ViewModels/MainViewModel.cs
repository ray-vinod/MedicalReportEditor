namespace MedicalReportEditor.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IReportService _reportService;
    private readonly IHospitalService _hospitalService;
    private readonly IPdfService _pdfService;
    private readonly ILogger<MainViewModel> _logger;

    [ObservableProperty]
    private ObservableCollection<ReportTemplate> _templates = new();

    [ObservableProperty]
    private ReportTemplate? _selectedTemplate;

    [ObservableProperty]
    private HospitalInfo _hospitalInfo = new();

    [ObservableProperty]
    private PatientData _patientData = new();

    [ObservableProperty]
    private string _statusMessage = "Ready";

    [ObservableProperty]
    private bool _isBusy;

    public ObservableCollection<string> ReportTypes { get; } = new()
        {
            "Upper GI Endoscopy",
            "Colonoscopy",
            "Sigmoidoscopy",
            "ERCP",
            "Bronchoscopy"
        };

    public MainViewModel(IReportService reportService,
                       IHospitalService hospitalService,
                       IPdfService pdfService,
                       ILogger<MainViewModel> logger)
    {
        _reportService = reportService;
        _hospitalService = hospitalService;
        _pdfService = pdfService;
        _logger = logger;

        InitializeAsync();
    }

    private async void InitializeAsync()
    {
        await LoadTemplatesAsync();
        await LoadHospitalInfoAsync();
        GenerateSamplePatientData();
    }

    [RelayCommand]
    private async Task LoadTemplatesAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Loading templates...";

            var collection = await _reportService.LoadTemplatesAsync();
            Templates = new ObservableCollection<ReportTemplate>(collection.Reports);

            if (Templates.Any())
            {
                SelectedTemplate = Templates.First();
            }

            StatusMessage = $"Loaded {Templates.Count} templates";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading templates");
            StatusMessage = "Error loading templates";
            MessageBox.Show($"Error loading templates: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task CreateNewTemplate()
    {
        try
        {
            var dialog = new InputDialog("New Template", "Enter template name:");
            if (dialog.ShowDialog() != true || string.IsNullOrWhiteSpace(dialog.InputText))
                return;

            var template = await _reportService.CreateTemplate(dialog.InputText, ReportType.Custom);
            Templates.Add(template);
            SelectedTemplate = template;

            StatusMessage = $"Created template: {template.Name}";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating template");
            MessageBox.Show($"Error creating template: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task DeleteTemplate()
    {
        if (SelectedTemplate == null) return;

        var result = MessageBox.Show(
            $"Are you sure you want to delete '{SelectedTemplate.Name}'?",
            "Confirm Delete",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (result != MessageBoxResult.Yes) return;

        try
        {
            var success = await _reportService.DeleteTemplate(SelectedTemplate.Name);
            if (success)
            {
                Templates.Remove(SelectedTemplate);
                SelectedTemplate = Templates.FirstOrDefault();
                StatusMessage = $"Deleted template";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting template");
            MessageBox.Show($"Error deleting template: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private async Task PreviewPdf()
    {
        if (SelectedTemplate == null)
        {
            MessageBox.Show("Please select a template first", "No Template",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            StatusMessage = "Generating PDF preview...";

            var pdfPath = await _pdfService.GenerateReportPreview(
                SelectedTemplate, HospitalInfo, PatientData);

            if (File.Exists(pdfPath))
            {
                // Open PDF with default viewer
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = pdfPath,
                    UseShellExecute = true
                });

                StatusMessage = "PDF preview opened";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error generating PDF preview");
            StatusMessage = "Error generating PDF";
            MessageBox.Show($"Error generating PDF: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportPdf()
    {
        if (SelectedTemplate == null) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "PDF Files (*.pdf)|*.pdf",
            DefaultExt = ".pdf",
            FileName = $"{PatientData.PatientId}_{DateTime.Now:yyyyMMdd}.pdf"
        };

        if (dialog.ShowDialog() != true) return;

        try
        {
            IsBusy = true;
            StatusMessage = "Exporting PDF...";

            await _pdfService.ExportReport(
                SelectedTemplate, HospitalInfo, PatientData, dialog.FileName);

            StatusMessage = $"PDF exported to: {dialog.FileName}";
            MessageBox.Show($"PDF exported successfully to:\n{dialog.FileName}",
                "Export Complete", MessageBoxButton.OK, MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting PDF");
            StatusMessage = "Error exporting PDF";
            MessageBox.Show($"Error exporting PDF: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private void OpenHospitalSettings()
    {
        var settingsWindow = new HospitalSettingsView();
        settingsWindow.Owner = Application.Current.MainWindow;
        settingsWindow.ShowDialog();
    }

    [RelayCommand]
    private void OpenImageCrop()
    {
        var cropWindow = new ImageCropView();
        cropWindow.Owner = Application.Current.MainWindow;
        cropWindow.ShowDialog();
    }

    private async Task LoadHospitalInfoAsync()
    {
        try
        {
            HospitalInfo = await _hospitalService.LoadHospitalInfoAsync();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading hospital info");
        }
    }

    private void GenerateSamplePatientData()
    {
        PatientData = new PatientData
        {
            PatientId = "KMT3675",
            PatientName = "SHER BAHADUR BHANDARI",
            AgeGender = "56Yrs, Male",
            VisitDate = DateTime.Parse("08/10/2023"),
            ReferredBy = "Self",
            ConsultedBy = "Dr. Rahul Pathak",
            ProcedureType = "Sigmoidoscopy",
            Premedication = "4% Xylocaine",
            Instrument = "Normal",
            Findings = new Dictionary<string, string>
                {
                    { "P/R", "Nil" },
                    { "Anal Canal", "Normal" },
                    { "Rectum", "SMALL HEMORRHIDS, EROSIVE MUCOSA" }
                },
            Impression = "PROCTITIS, E1 DISEASE, K/C/O OF ULCERATIVE COLITIS",
            BiopsyTaken = false
        };
    }
}

// Simple input dialog
public class InputDialog : Window
{
    public string InputText { get; private set; } = "";

    public InputDialog(string title, string prompt)
    {
        Title = title;
        Width = 300;
        Height = 150;
        WindowStartupLocation = WindowStartupLocation.CenterOwner;

        var textBox = new TextBox { Margin = new Thickness(10), VerticalAlignment = VerticalAlignment.Center };
        var okButton = new Button { Content = "OK", Width = 75, Margin = new Thickness(0, 0, 10, 0), IsDefault = true };
        var cancelButton = new Button { Content = "Cancel", Width = 75, IsCancel = true };

        okButton.Click += (s, e) => { InputText = textBox.Text; DialogResult = true; };

        var grid = new Grid();
        grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        grid.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        Grid.SetRow(textBox, 0);
        Grid.SetRow(okButton, 1);
        Grid.SetRow(cancelButton, 1);

        var buttonPanel = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(10)
        };
        buttonPanel.Children.Add(okButton);
        buttonPanel.Children.Add(cancelButton);

        grid.Children.Add(textBox);
        grid.Children.Add(buttonPanel);

        Content = grid;
    }
}