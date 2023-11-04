namespace Services;

internal class ImageCropper : IImageCropper
{
    private readonly ILogger<ImageCropper> _logger;

    public ImageCropper(ILogger<ImageCropper> logger)
    {
        _logger = logger;
    }

    public async Task<string> Crop(string filePath)
    {
        using var originFs = new FileStream(filePath, FileMode.Open, FileAccess.Read);
        using var image = await Image.LoadAsync(originFs);

        // @@TODO: Handle when cropping is not necessary.
        var targetWidth = image.Width;
        if (targetWidth > 1920) targetWidth = 1920;
        var targetHeight = (int)(targetWidth * 0.5625);
        if (targetHeight > image.Height) targetHeight = image.Height;

        image.Mutate(ctx => ctx.Crop(new Rectangle(0, 0, targetWidth, targetHeight)));

        var croppedImagePath = $"{Path.GetDirectoryName(filePath)}/{Path.GetFileNameWithoutExtension(filePath)}-cropped.png";
        using var targetFs = new FileStream(croppedImagePath, FileMode.CreateNew, FileAccess.Write);
        await image.SaveAsPngAsync(targetFs);

        return croppedImagePath;
    }
}
