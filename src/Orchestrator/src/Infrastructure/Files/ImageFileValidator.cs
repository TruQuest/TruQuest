using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace Infrastructure.Files;

internal class ImageFileValidator
{
    private static readonly Dictionary<string, List<byte[]>> _fileExtensionToSignatures = new()
    {
        {
            ".jpg", new List<byte[]> {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE1 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE8 }
            }
        },
        {
            ".jpeg", new List<byte[]> {
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE0 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE2 },
                new byte[] { 0xFF, 0xD8, 0xFF, 0xE3 }
            }
        },
        {
            ".png", new List<byte[]> {
                new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }
            }
        }
    };

    public bool IsFileDeclaredAsImage(ContentDispositionHeaderValue contentDisposition)
    {
        var fileName = contentDisposition.FileName.Value;
        var ext = Path.GetExtension(fileName)?.ToLower();
        return !string.IsNullOrEmpty(ext) && _fileExtensionToSignatures.ContainsKey(ext);
    }

    public async Task<(bool Valid, string? Ext, byte[]? Header)> ValidateSignature(
        MultipartSection section, ContentDispositionHeaderValue contentDisposition
    )
    {
        var fileName = contentDisposition.FileName.Value;
        var ext = Path.GetExtension(fileName)!.ToLower();

        var signatures = _fileExtensionToSignatures[ext];
        var header = new byte[signatures.Max(signature => signature.Length)];

        int n = await section.Body.ReadAtLeastAsync(header, header.Length, throwOnEndOfStream: false); // @@TODO: Cancel
        if (n < header.Length)
        {
            return (Valid: false, Ext: null, Header: null);
        }

        bool valid = signatures.Any(
            signature => header.Take(signature.Length).SequenceEqual(signature)
        );

        return (Valid: valid, Ext: ext, Header: header);
    }
}