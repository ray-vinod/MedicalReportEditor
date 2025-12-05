using MedicalReportEditor.Models;
using Microsoft.Extensions.Logging;
using System.Configuration;

namespace MedicalReportEditor.Services;

public class HospitalService : IHospitalService
{
    private readonly ILogger<HospitalService> _logger;
    private const string ConfigPath = "user.config";

    public HospitalService(ILogger<HospitalService> logger)
    {
        _logger = logger;
    }

    public async Task<HospitalInfo> LoadHospitalInfoAsync()
    {
        return await Task.Run(() =>
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = config.AppSettings.Settings;

                return new HospitalInfo
                {
                    HospitalName = GetSetting(settings, "HospitalName", "CIWEC HOSPITAL PVT. LTD."),
                    HospitalAddress = GetSetting(settings, "HospitalAddress", "Lainchaur, Kathmandu, Nepal"),
                    ContactNumber = GetSetting(settings, "ContactNumber", "01-4535232, 4524111"),
                    ReportHeader = GetSetting(settings, "ReportHeader", "Endoscopy"),
                    DepartmentHeader = GetSetting(settings, "DepartmentHeader", ""),
                    PatientIdPrefix = GetSetting(settings, "PatientIdPrefix", "KMT"),
                    HospitalLogoPath = GetSetting(settings, "HospitalLogo", ""),
                    EmailAddress = GetSetting(settings, "EmailAddress", "info@ciwec-clinic.com"),
                    Website = GetSetting(settings, "Website", "www.ciwec-clinic.com"),
                    FaxNumber = GetSetting(settings, "FaxNumber", "977-1-4412590")
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading hospital info");
                return new HospitalInfo();
            }
        });
    }

    public async Task SaveHospitalInfoAsync(HospitalInfo info)
    {
        await Task.Run(() =>
        {
            try
            {
                var config = ConfigurationManager.OpenExeConfiguration(ConfigurationUserLevel.None);
                var settings = config.AppSettings.Settings;

                UpdateSetting(settings, "HospitalName", info.HospitalName);
                UpdateSetting(settings, "HospitalAddress", info.HospitalAddress);
                UpdateSetting(settings, "ContactNumber", info.ContactNumber);
                UpdateSetting(settings, "ReportHeader", info.ReportHeader);
                UpdateSetting(settings, "DepartmentHeader", info.DepartmentHeader);
                UpdateSetting(settings, "PatientIdPrefix", info.PatientIdPrefix);
                UpdateSetting(settings, "HospitalLogo", info.HospitalLogoPath);
                UpdateSetting(settings, "EmailAddress", info.EmailAddress);
                UpdateSetting(settings, "Website", info.Website);
                UpdateSetting(settings, "FaxNumber", info.FaxNumber);

                config.Save(ConfigurationSaveMode.Modified);
                ConfigurationManager.RefreshSection("appSettings");

                _logger.LogInformation("Hospital info saved successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving hospital info");
                throw;
            }
        });
    }

    private string GetSetting(KeyValueConfigurationCollection settings, string key, string defaultValue)
    {
        return settings[key]?.Value ?? defaultValue;
    }

    private void UpdateSetting(KeyValueConfigurationCollection settings, string key, string value)
    {
        if (settings[key] != null)
        {
            settings[key].Value = value;
        }
        else
        {
            settings.Add(key, value);
        }
    }
}