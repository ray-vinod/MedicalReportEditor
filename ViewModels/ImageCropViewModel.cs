namespace MedicalReportEditor.ViewModels;

public partial class ImageCropViewModel : ObservableObject
{
    private readonly IImageService _imageService;
    private string _currentImagePath;

    [ObservableProperty]
    private BitmapImage _imageSource;

    [ObservableProperty]
    private CropArea _cropArea = new();

    [ObservableProperty]
    private bool _isCropVisible;

    [ObservableProperty]
    private string _selectedAspectRatio = "Free";

    [ObservableProperty]
    private int _imageWidth;

    [ObservableProperty]
    private int _imageHeight;

    public ObservableCollection<string> AspectRatios { get; } = new()
        {
            "Free",
            "Square (1:1)",
            "4:3",
            "16:9",
            "A4 (1.414)",
            "Letter (1.294)"
        };

    public ObservableCollection<string> PaperSizes { get; } = new()
        {
            "A4 (210x297mm)",
            "Letter (216x279mm)",
            "Legal (216x356mm)",
            "A3 (297x420mm)",
            "A5 (148x210mm)"
        };

    public ImageCropViewModel(IImageService imageService)
    {
        _imageService = imageService;
    }

    [RelayCommand]
    private void OpenImage()
    {
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
            Title = "Select an image"
        };

        if (dialog.ShowDialog() == true)
        {
            LoadImage(dialog.FileName);
        }
    }

    [RelayCommand]
    private async Task AutoDetect()
    {
        if (ImageSource == null) return;

        try
        {
            var dimensions = await _imageService.GetImageDimensionsAsync(_currentImagePath);
            ImageWidth = dimensions.width;
            ImageHeight = dimensions.height;

            // Auto-detect crop area (center 80% of image)
            CropArea = new CropArea
            {
                X = ImageWidth * 0.1,
                Y = ImageHeight * 0.1,
                Width = ImageWidth * 0.8,
                Height = ImageHeight * 0.8
            };

            ApplyAspectRatio();
            IsCropVisible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error detecting image: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    [RelayCommand]
    private void ApplyAspectRatio()
    {
        if (!IsCropVisible || ImageSource == null) return;

        var aspectRatio = SelectedAspectRatio switch
        {
            "Square (1:1)" => 1.0,
            "4:3" => 4.0 / 3.0,
            "16:9" => 16.0 / 9.0,
            "A4 (1.414)" => 1.414,
            "Letter (1.294)" => 1.294,
            _ => 0.0 // Free
        };

        if (aspectRatio > 0)
        {
            CropArea.AspectRatio = aspectRatio;

            // Adjust width based on aspect ratio
            if (CropArea.Width / CropArea.Height > aspectRatio)
            {
                CropArea.Width = CropArea.Height * aspectRatio;
            }
            else
            {
                CropArea.Height = CropArea.Width / aspectRatio;
            }
        }
        else
        {
            CropArea.AspectRatio = 0;
        }
    }

    [RelayCommand]
    private void ResetCrop()
    {
        if (ImageSource == null) return;

        CropArea = new CropArea
        {
            X = 0,
            Y = 0,
            Width = ImageWidth,
            Height = ImageHeight,
            AspectRatio = 0
        };
    }

    [RelayCommand]
    private async Task ApplyCrop()
    {
        if (ImageSource == null || !IsCropVisible) return;

        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "JPEG Image|*.jpg|PNG Image|*.png",
            DefaultExt = ".jpg",
            FileName = Path.GetFileNameWithoutExtension(_currentImagePath) + "_cropped"
        };

        if (dialog.ShowDialog() == true)
        {
            try
            {
                await _imageService.CropAndSaveAsync(_currentImagePath, CropArea, dialog.FileName);

                MessageBox.Show("Image cropped and saved successfully!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                // Load the cropped image
                LoadImage(dialog.FileName);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cropping image: {ex.Message}", "Error",
                    MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }

    private void LoadImage(string filePath)
    {
        try
        {
            _currentImagePath = filePath;
            ImageSource = new BitmapImage(new Uri(filePath));
            ResetCrop();
            IsCropVisible = true;
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error loading image: {ex.Message}", "Error",
                MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }

    partial void OnSelectedAspectRatioChanged(string value)
    {
        ApplyAspectRatio();
    }
}