using Application.Common.Interfaces;

namespace Infrastructure.Files;

internal class ImageSignatureVerifier : IImageSignatureVerifier
{
    public int GetSignatureSize(string fileExt) => 4;

    public bool Verify(byte[] content, string fileExt)
    {
        return true;
    }
}