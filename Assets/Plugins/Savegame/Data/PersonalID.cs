using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack = 1, Size = 0x34)]
public unsafe struct PersonalID
{
    public TownID TownId;
    public uint UniqueId;
    //[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 10)]
    public string Name;

    //public PersonalID(ISaveFile save, int offset) => this = save.ReadStruct<PersonalID>(offset);

    public void Write(BinaryData data, int offset)
    {
        TownId.Write(data, offset);
        data.WriteU32(offset + 0x1C, this.UniqueId);
        data.WriteString(offset + 0x20, this.Name, 10);
    }

    public static PersonalID Read(BinaryData data, int offset)
    {
        var ret = new PersonalID();
        ret.TownId = TownID.Read(data, offset);
        ret.UniqueId = data.ReadU32(offset + 0x1C);
        ret.Name = data.ReadString(offset + 0x20, 10);
        return ret;
    }

    public override string ToString() => Name;
}
