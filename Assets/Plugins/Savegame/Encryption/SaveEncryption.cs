using System;

public static class SaveEncryption
{
    private static byte[] GetParam(uint[] data, in int index)
    {
        var sead = new SEADRandom(data[data[index] & 0x7F]);
        var prms = data[data[index + 1] & 0x7F] & 0x7F;

        var rndRollCount = (prms & 0xF) + 1;
        for (var i = 0; i < rndRollCount; i++)
            sead.GetU64();

        var result = new byte[0x10];
        for (var i = 0; i < result.Length; i++)
            result[i] = (byte)(sead.GetU32() >> 24);

        return result;
    }

    public static byte[] Decrypt(byte[] headerData, byte[] encData)
    {
        // First 256 bytes go unused
        var importantData = new uint[0x80];
        Buffer.BlockCopy(headerData, 0x100, importantData, 0, 0x200);

        // Set up Key
        var key = GetParam(importantData, 0);

        // Set up counter
        var counter = GetParam(importantData, 2);

        // Do the AES
        using (var aesCtr = new Aes128CounterMode(counter))
        {
            var transform = aesCtr.CreateDecryptor(key, counter);
            var decData = new byte[encData.Length];

            transform.TransformBlock(encData, 0, encData.Length, decData, 0);
            return decData;
        }
    }

    private static (byte[] headerData, byte[] key, byte[] ctr) GenerateHeaderFile(uint seed, in byte[] versionData)
    {
        // Generate 128 Random uints which will be used for params
        var random = new SEADRandom(seed);
        var encryptData = new uint[128];
        for (var i = 0; i < encryptData.Length; i++)
            encryptData[i] = random.GetU32();

        var headerData = new byte[0x300];
        Buffer.BlockCopy(versionData, 0, headerData, 0, 0x100);
        Buffer.BlockCopy(encryptData, 0, headerData, 0x100, 0x200);
        return (headerData, GetParam(encryptData, 0), GetParam(encryptData, 2));
    }

    public static (byte[] encData, byte[] headerData) Encrypt(byte[] data, uint seed)
    {
        // Generate header file and get key and counter
        var (headerData, key, ctr) = GenerateHeaderFile(seed, data);

        // Encrypt file
        using (var aesCtr = new Aes128CounterMode(ctr))
        {
            var transform = aesCtr.CreateEncryptor(key, ctr);
            var encData = new byte[data.Length];
            transform.TransformBlock(data, 0, data.Length, encData, 0);

            return (encData, headerData);
        }
    }
}