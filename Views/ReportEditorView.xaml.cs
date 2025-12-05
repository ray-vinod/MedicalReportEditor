using MedicalReportEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows.Controls;

namespace MedicalReportEditor.Views;

public partial class ReportEditorView : UserControl
{
    public ReportEditorView()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<ReportEditorViewModel>();
    }
}