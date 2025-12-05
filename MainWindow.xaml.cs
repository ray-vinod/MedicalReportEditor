using MedicalReportEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace MedicalReportEditor;


public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<MainViewModel>();
    }
}