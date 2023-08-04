namespace Services;

internal interface IImageCropper
{
    Task<string> Crop(string filePath);
}
