using System.Collections;

namespace ContractStorageExplorer;

public static class BitArrayExtension
{
    public static BitArray TwosComplement(this BitArray bitArray)
    {
        bitArray = bitArray.Not();
        var carry = true;
        for (int i = bitArray.Count - 1; carry && i >= 0; --i)
        {
            bool bit = bitArray[i];
            if (!bit)
            {
                bitArray[i] = true;
                carry = false;
            }
            else
            {
                bitArray[i] = false;
            }
        }

        return bitArray;
    }

    public static byte[] ToByteArray(this BitArray bitArray)
    {
        var byteArray = new byte[(bitArray.Length - 1) / 8 + 1];
        bitArray.CopyTo(byteArray, 0);

        return byteArray;
    }
}