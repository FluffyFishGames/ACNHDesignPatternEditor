using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1, Size = 0x1C)]
public unsafe struct TownID
{
    public uint UniqueID;
    public string Name;
    public uint Unknown;

    public void Write(BinaryData data, int offset)
    {
        data.WriteU32(offset, this.UniqueID);
        data.WriteString(offset + 0x04, this.Name, 10);
        data.WriteU32(offset + 0x18, Unknown);
    }

    public static TownID Read(BinaryData data, int offset)
    {
        var ret = new TownID();
        ret.UniqueID = data.ReadU32(offset);
        ret.Name = data.ReadString(offset + 0x04, 10);
        ret.Unknown = data.ReadU32(offset + 0x18);
        return ret;
    }

    public override string ToString() => Name;
}