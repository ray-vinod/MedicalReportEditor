using MedicalReportEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;


namespace MedicalReportEditor.Views;

public partial class HospitalSettingsView : Window
{
    public HospitalSettingsView()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<HospitalSettingsViewModel>();
    }
}