namespace Services;

internal interface IImageSignatureVerifier
{
    int GetSignatureSize(string fileExt);
    bool Verify(byte[] content, string fileExt);
}