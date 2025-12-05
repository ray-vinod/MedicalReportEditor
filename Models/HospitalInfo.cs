namespace MedicalReportEditor.Models;

public class HospitalInfo : INotifyPropertyChanged
{
    private string _hospitalName = "CIWEC HOSPITAL PVT. LTD.";
    private string _hospitalAddress = "Lainchaur, Kathmandu, Nepal";
    private string _contactNumber = "01-4535232, 4524111";
    private string _reportHeader = "Endoscopy";
    private string _departmentHeader = "";
    private string _patientIdPrefix = "KMT";
    private string _hospitalLogoPath = "";
    private string _emailAddress = "info@ciwec-clinic.com";
    private string _website = "www.ciwec-clinic.com";
    private string _faxNumber = "977-1-4412590";

    public string HospitalName
    {
        get => _hospitalName;
        set => SetField(ref _hospitalName, value);
    }

    public string HospitalAddress
    {
        get => _hospitalAddress;
        set => SetField(ref _hospitalAddress, value);
    }

    public string ContactNumber
    {
        get => _contactNumber;
        set => SetField(ref _contactNumber, value);
    }

    public string ReportHeader
    {
        get => _reportHeader;
        set => SetField(ref _reportHeader, value);
    }

    public string DepartmentHeader
    {
        get => _departmentHeader;
        set => SetField(ref _departmentHeader, value);
    }

    public string PatientIdPrefix
    {
        get => _patientIdPrefix;
        set => SetField(ref _patientIdPrefix, value);
    }

    public string HospitalLogoPath
    {
        get => _hospitalLogoPath;
        set => SetField(ref _hospitalLogoPath, value);
    }

    public string EmailAddress
    {
        get => _emailAddress;
        set => SetField(ref _emailAddress, value);
    }

    public string Website
    {
        get => _website;
        set => SetField(ref _website, value);
    }

    public string FaxNumber
    {
        get => _faxNumber;
        set => SetField(ref _faxNumber, value);
    }

    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged(string propertyName)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value)) return false;
        field = value;
        OnPropertyChanged(propertyName);
        return true;
    }
}