using MedicalReportEditor.Services;
using MedicalReportEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;

namespace MedicalReportEditor;

public partial class App : Application
{
    private readonly IHost _host;

    public static IServiceProvider Services =>
        ((App)Current)._host?.Services
        ?? throw new InvalidOperationException("Service provider not initialized.");

    public App()
    {
        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((context, services) =>
            {
                // Register Services
                services.AddSingleton<IReportService, ReportService>();
                services.AddSingleton<IHospitalService, HospitalService>();
                services.AddSingleton<IPdfService, PdfService>();
                services.AddSingleton<IImageService, ImageService>();

                // Register ViewModels
                services.AddSingleton<MainViewModel>();
                services.AddTransient<ReportEditorViewModel>();
                services.AddTransient<HospitalSettingsViewModel>();
                services.AddTransient<ImageCropViewModel>();

                // Register Views
                services.AddTransient<MainWindow>();
                services.AddTransient<Views.ReportEditorView>();
                services.AddTransient<Views.HospitalSettingsView>();
                services.AddTransient<Views.ImageCropView>();

                // Add Logging
                services.AddLogging();
            })
            .Build();
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetRequiredService<MainWindow>();
        mainWindow.DataContext = _host.Services.GetRequiredService<MainViewModel>();
        mainWindow.Show();

        base.OnStartup(e);
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }

        base.OnExit(e);
    }
}
