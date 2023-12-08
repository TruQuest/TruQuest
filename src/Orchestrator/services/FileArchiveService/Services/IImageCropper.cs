namespace Services;

internal interface IImageCropper
{
    Task<string> Crop(string requestId, string filePath);
}
