using MedicalReportEditor.ViewModels;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;
using System.Windows.Input;

namespace MedicalReportEditor.Views;

public partial class ImageCropView : Window
{
    public ImageCropView()
    {
        InitializeComponent();

        DataContext = App.Services.GetRequiredService<ImageCropViewModel>();
    }

    private void CancelButton_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is IDisposable disposable)
        {
            disposable.Dispose();
        }

        this.Close();
    }
}