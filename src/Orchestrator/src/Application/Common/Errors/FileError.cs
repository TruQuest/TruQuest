using Domain.Errors;

namespace Application.Common.Errors;

public class FileError : HandleError
{
    public FileError(string message) : base("File")
    {
        Errors = new Dictionary<string, string[]>
        {
            [string.Empty] = new[] { message }
        };
    }
}