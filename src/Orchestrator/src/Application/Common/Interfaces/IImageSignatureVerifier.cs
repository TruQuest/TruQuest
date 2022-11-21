namespace Application.Common.Interfaces;

public interface IImageSignatureVerifier
{
    int GetSignatureSize(string fileExt);
    bool Verify(byte[] content, string fileExt);
}