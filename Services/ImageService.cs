namespace MedicalReportEditor.Services;

public class ImageService : IImageService
{
    private readonly ILogger<ImageService> _logger;

    public ImageService(ILogger<ImageService> logger)
    {
        _logger = logger;
    }

    public async Task<byte[]> CropImageAsync(string sourcePath, CropArea area)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath);

            // Validate crop area
            var cropRect = ValidateCropArea(image, area);

            // Perform crop
            image.Mutate(ctx => ctx.Crop(cropRect));

            // Convert to bytes
            using var memoryStream = new MemoryStream();
            var encoder = GetEncoder(sourcePath);
            await image.SaveAsync(memoryStream, encoder);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cropping image: {Path}", sourcePath);
            throw;
        }
    }

    public async Task<string> CropAndSaveAsync(string sourcePath, CropArea area, string outputPath)
    {
        try
        {
            var imageBytes = await CropImageAsync(sourcePath, area);
            await File.WriteAllBytesAsync(outputPath, imageBytes);

            _logger.LogInformation("Image cropped and saved: {Path}", outputPath);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error saving cropped image");
            throw;
        }
    }

    public async Task<byte[]> ResizeImageAsync(string sourcePath, int maxWidth, int maxHeight)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath);

            // Calculate new dimensions maintaining aspect ratio
            var (newWidth, newHeight) = CalculateDimensions(image.Width, image.Height, maxWidth, maxHeight);

            // Resize
            image.Mutate(ctx => ctx.Resize(newWidth, newHeight));

            // Convert to bytes
            using var memoryStream = new MemoryStream();
            var encoder = GetEncoder(sourcePath);
            await image.SaveAsync(memoryStream, encoder);

            return memoryStream.ToArray();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error resizing image: {Path}", sourcePath);
            throw;
        }
    }

    public async Task<(int width, int height)> GetImageDimensionsAsync(string imagePath)
    {
        try
        {
            var info = await Image.IdentifyAsync(imagePath);
            return (info.Width, info.Height);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting image dimensions: {Path}", imagePath);
            throw;
        }
    }

    public async Task<string> ConvertImageFormatAsync(string sourcePath, string outputPath, string format)
    {
        try
        {
            using var image = await Image.LoadAsync(sourcePath);
            IImageEncoder encoder = format.ToLower() switch
            {
                "jpg" or "jpeg" => new JpegEncoder { Quality = 90 },
                "png" => new PngEncoder(),
                _ => throw new ArgumentException($"Unsupported format: {format}")
            };

            await image.SaveAsync(outputPath, encoder);
            return outputPath;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error converting image format");
            throw;
        }
    }

    private Rectangle ValidateCropArea(Image image, CropArea area)
    {
        var x = (int)Math.Max(0, Math.Min(area.X, image.Width - 1));
        var y = (int)Math.Max(0, Math.Min(area.Y, image.Height - 1));
        var width = (int)Math.Max(1, Math.Min(area.Width, image.Width - x));
        var height = (int)Math.Max(1, Math.Min(area.Height, image.Height - y));

        // Apply aspect ratio if specified
        if (area.AspectRatio > 0)
        {
            if (width / (double)height > area.AspectRatio)
            {
                width = (int)(height * area.AspectRatio);
            }
            else
            {
                height = (int)(width / area.AspectRatio);
            }

            // Revalidate bounds
            width = Math.Min(width, image.Width - x);
            height = Math.Min(height, image.Height - y);
        }

        return new Rectangle(x, y, width, height);
    }

    private (int width, int height) CalculateDimensions(int originalWidth, int originalHeight, int maxWidth, int maxHeight)
    {
        var ratioX = (double)maxWidth / originalWidth;
        var ratioY = (double)maxHeight / originalHeight;
        var ratio = Math.Min(ratioX, ratioY);

        var newWidth = (int)(originalWidth * ratio);
        var newHeight = (int)(originalHeight * ratio);

        return (newWidth, newHeight);
    }

    private IImageEncoder GetEncoder(string filePath)
    {
        var extension = Path.GetExtension(filePath).ToLowerInvariant();

        return extension switch
        {
            ".jpg" or ".jpeg" => new JpegEncoder { Quality = 90 },
            ".png" => new PngEncoder(),
            ".bmp" => new SixLabors.ImageSharp.Formats.Bmp.BmpEncoder(),
            ".gif" => new SixLabors.ImageSharp.Formats.Gif.GifEncoder(),
            _ => new JpegEncoder { Quality = 90 }
        };
    }
}