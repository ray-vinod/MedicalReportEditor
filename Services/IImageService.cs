using MedicalReportEditor.Models;

namespace MedicalReportEditor.Services;

public interface IImageService
{
    Task<byte[]> CropImageAsync(string sourcePath, CropArea area);
    Task<string> CropAndSaveAsync(string sourcePath, CropArea area, string outputPath);
    Task<byte[]> ResizeImageAsync(string sourcePath, int maxWidth, int maxHeight);
    Task<(int width, int height)> GetImageDimensionsAsync(string imagePath);
    Task<string> ConvertImageFormatAsync(string sourcePath, string outputPath, string format);
}