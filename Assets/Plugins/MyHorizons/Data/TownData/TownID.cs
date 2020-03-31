using MyHorizons.Data.Save;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.TownData
{
    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode, Pack = 1, Size = 0x1C)]
    public unsafe struct TownID
    {
        public uint UniqueID;
        public fixed char Name[10];
        public uint Unknown;

        public TownID(ISaveFile save, int offset) => this = save.ReadStruct<TownID>(offset);

        public string GetName()
        {
            fixed (char* name = Name)
                return new string(name);
        }

        public void SetName(in string newName)
        {
            fixed (char* name = Name)
            {
                for (var i = 0; i < 10; i++)
                    name[i] = i >= newName.Length ? '\0' : newName[i];
            }
        }

        public override string ToString() => GetName();
    }
}
