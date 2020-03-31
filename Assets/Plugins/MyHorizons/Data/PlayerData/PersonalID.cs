using MyHorizons.Data.Save;
using MyHorizons.Data.TownData;
using System.Runtime.InteropServices;

namespace MyHorizons.Data.PlayerData
{
    [StructLayout(LayoutKind.Sequential, CharSet=CharSet.Unicode, Pack = 1, Size = 0x34)]
    public unsafe struct PersonalID
    {
        public TownID TownId;
        public uint UniqueId;
        public fixed char Name[10];

        public PersonalID(ISaveFile save, int offset) => this = save.ReadStruct<PersonalID>(offset);

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
