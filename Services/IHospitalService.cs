namespace MedicalReportEditor.Services;

public interface IHospitalService
{
    Task<HospitalInfo> LoadHospitalInfoAsync();
    Task SaveHospitalInfoAsync(HospitalInfo info);
}