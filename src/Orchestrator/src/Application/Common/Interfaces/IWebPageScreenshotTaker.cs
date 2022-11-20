namespace Application.Common.Interfaces;

public interface IWebPageScreenshotTaker
{
    Task Take(string url, string filePath);
}