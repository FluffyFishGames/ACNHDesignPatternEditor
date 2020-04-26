public sealed class EncryptedInt32
{
    // Encryption constant used to encrypt the int.
    private const uint ENCRYPTION_CONSTANT = 0x80E32B11;
    // Base shift count used in the encryption.
    private const byte SHIFT_BASE = 3;

    private uint EncryptedValue;

    private ushort Adjust;
    private byte Shift;
    private byte Checksum;

    public EncryptedInt32()
    {
        EncryptedValue = 0;
        Adjust = 0;
        Shift = 0;
        Checksum = 0;
    }

    public EncryptedInt32(uint encryptedValue, ushort adjust, byte shift, byte checksum)
    {
        EncryptedValue = encryptedValue;
        Adjust = adjust;
        Shift = shift;
        Checksum = checksum;
    }

    public EncryptedInt32(BinaryData save, int offset)
        : this(save.ReadU32(offset), save.ReadU16(offset + 4),
                save.ReadU8(offset + 6), save.ReadU8(offset + 7)) { }

    public EncryptedInt32(uint value) => Set(value);

    // Quickhand method to determine if the encrypted int is still valid.
    public bool IsValid() => Checksum == CalculateChecksum();

    // Calculates a checksum for a given encrypted value
    // Checksum calculation is every byte of the encrypted in added together minus 0x2D.
    public byte CalculateChecksum()
    {
        return (byte)(((byte)EncryptedValue + (byte)(EncryptedValue >> 16) + (byte)(EncryptedValue >> 24) + (byte)(EncryptedValue >> 8)) - 0x2D);
    }

    // Decrypts an encrypted int with the given encrypt params
    // Encrypt params are made up of the following:
    //  ushort adjust -- changes the constant value, additive
    //  byte    shift -- shifts the encrypted int. 3 is the current base.
    //  byte checksum -- the embedded checksum for the encrypted int.
    public uint Decrypt()
    {
        // If both are 0, then it's just 0.
        if (EncryptedValue == 0 && Adjust == 0 && Shift == 0 && Checksum == 0) return 0;
        // Verify embedded checksum is correct
        if (IsValid())
        {
            // Decrypt the encrypted int using the given params.
            ulong val = ((ulong)EncryptedValue) << (((32 - SHIFT_BASE) - Shift) & 0x3F);
            int valConcat = (int)val + (int)(val >> 32);
            return (uint)((ENCRYPTION_CONSTANT - Adjust) + valConcat);
        }
        return 0;
    }

    // NOTE: The params must be written as a byte array or a struct.
    public (uint, uint) Get() => (EncryptedValue, (((uint)Adjust) << 16) | (((uint)Shift) << 8) | Checksum);

    // Encrypts a given uint. See above for encrypt params.
    public (uint, uint) Set(uint value)
    {
        // Create random generator
        var random = new SEADRandom();
        // Get our adjust value
        Adjust = (ushort)(random.GetU32() >> 16);
        // Get our shift value
        Shift = (byte)((((ulong)random.GetU32()) * 0x1B) >> 32);
        // Do the encryption
        ulong adjustedValue = (ulong)(value + (Adjust - ENCRYPTION_CONSTANT)) << (Shift + SHIFT_BASE);
        EncryptedValue = (uint)((adjustedValue >> 32) + adjustedValue);
        Checksum = CalculateChecksum();
        return Get();
    }

    // Writes the encrypted int directly to the loaded save at a given offset
    public void Write(BinaryData save, int offset)
    {
        save.WriteU32(offset, EncryptedValue);
        save.WriteU16(offset + 4, Adjust);
        save.WriteU8(offset + 6, Shift);
        save.WriteU8(offset + 7, Checksum);
    }

    public override string ToString() => Decrypt().ToString();
}