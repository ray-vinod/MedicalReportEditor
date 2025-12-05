namespace MedicalReportEditor.ViewModels;

public partial class HospitalSettingsViewModel : ObservableObject
{
    private readonly IHospitalService _hospitalService;
    private HospitalInfo _originalInfo;

    [ObservableProperty]
    private string _hospitalName = "";

    [ObservableProperty]
    private string _hospitalAddress = "";

    [ObservableProperty]
    private string _contactNumber = "";

    [ObservableProperty]
    private string _reportHeader = "";

    [ObservableProperty]
    private string _departmentHeader = "";

    [ObservableProperty]
    private string _patientIdPrefix = "";

    [ObservableProperty]
    private string _hospitalLogoPath = "";

    [ObservableProperty]
    private BitmapImage _logoPreview;

    [ObservableProperty]
    private string _emailAddress = "";

    [ObservableProperty]
    private string _website = "";

    [ObservableProperty]
    private string _faxNumber = "";

    public HospitalSettingsViewModel(IHospitalService hospitalService)
    {
        _hospitalService = hospitalService;
        LoadHospitalInfo();
    }

    private async void LoadHospitalInfo()
    {
        try
        {
            _originalInfo = await _hospitalService.LoadHospitalInfoAsync();

            HospitalName = _originalInfo.HospitalName;
            HospitalAddress = _originalInfo.HospitalAddress;
            ContactNumber = _originalInfo.ContactNumber;
            ReportHeader = _originalInfo.ReportHeader;
            DepartmentHeader = _originalInfo.DepartmentHeader;
            PatientIdPrefix = _originalInfo.PatientIdPrefix;
            HospitalLogoPath = _originalInfo.HospitalLogoPath;
            EmailAddress = _originalInfo.EmailAddress;
            Website = _originalInfo.Website;
            FaxNumber = _originalInfo.FaxNumber;

            if (File.Exists(HospitalLogoPath))
            {
                LoadLogoPreview(HospitalLogoPath);
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading hospital info: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void BrowseLogo()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image Files|*.jpg;*.jpeg;*.png;*.bmp|All Files|*.*",
            Title = "Select Hospital Logo"
        };

        if (dialog.ShowDialog() == true)
        {
            HospitalLogoPath = dialog.FileName;
            LoadLogoPreview(HospitalLogoPath);
        }
    }

    private void LoadLogoPreview(string path)
    {
        try
        {
            LogoPreview = new BitmapImage(new Uri(path));
        }
        catch
        {
            // Ignore errors for preview
        }
    }

    [RelayCommand]
    private async Task Save()
    {
        try
        {
            var updatedInfo = new HospitalInfo
            {
                HospitalName = HospitalName,
                HospitalAddress = HospitalAddress,
                ContactNumber = ContactNumber,
                ReportHeader = ReportHeader,
                DepartmentHeader = DepartmentHeader,
                PatientIdPrefix = PatientIdPrefix,
                HospitalLogoPath = HospitalLogoPath,
                EmailAddress = EmailAddress,
                Website = Website,
                FaxNumber = FaxNumber
            };

            await _hospitalService.SaveHospitalInfoAsync(updatedInfo);

            MessageBox.Show("Hospital settings saved successfully", "Success",
                MessageBoxButton.OK, MessageBoxImage.Information);

            CloseWindow();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error saving hospital info: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void Cancel()
    {
        CloseWindow();
    }

    private void CloseWindow()
    {
        Application.Current.Windows
            .OfType<Window>()
            .FirstOrDefault(w => w.DataContext == this)?
            .Close();
    }
}