using MyHorizons.Data.Save;
using MyHorizons.Encryption;

namespace MyHorizons.Data.PlayerData
{
    public sealed class Player
    {
        public readonly int Index;

        public PersonalID PersonalId;
        public ItemCollection Pockets; // TODO: Detect pockets size
        public ItemCollection Storage; // TODO: Same as pockets
        public EncryptedInt32 Wallet;
        public EncryptedInt32 Bank;
        public EncryptedInt32 NookMiles;

        private readonly PersonalSaveFile _personalFile;

        private readonly struct Offsets
        {
            public readonly int PersonalId;
            public readonly int Pockets;
            public readonly int Wallet;
            public readonly int Bank;
            public readonly int NookMiles;
            public readonly int Photo;
            public readonly int Storage;

            public Offsets(int pid, int pockets, int wallet, int bank, int nookMiles, int photo, int storage)
            {
                PersonalId = pid;
                Pockets = pockets;
                Wallet = wallet;
                Bank = bank;
                NookMiles = nookMiles;
                Photo = photo;
                Storage = storage;
            }
        }

        private static readonly Offsets[] PlayerOffsetsByRevision =
        {
            new Offsets(0xB0A0, 0x35BD4, 0x11578, 0x68BE4, 0x11570, 0x11598, 0x35D50),
            new Offsets(0xB0B8, 0x35C20, 0x11590, 0x68C34, 0x11588, 0x115C4, 0x35D9C)
        };

        private static Offsets GetOffsetsFromRevision() => PlayerOffsetsByRevision[MainSaveFile.Singleton().GetRevision()];

        public Player(int idx, PersonalSaveFile personalSave)
        {
            _personalFile = personalSave;
            var offsets = GetOffsetsFromRevision();
            Index = idx;

            PersonalId = new PersonalID(personalSave, offsets.PersonalId);
            Wallet = new EncryptedInt32(personalSave, offsets.Wallet);
            Bank = new EncryptedInt32(personalSave, offsets.Bank);
            NookMiles = new EncryptedInt32(personalSave, offsets.NookMiles);

            // TODO: This should be refactored to detect the "expanded pockets" state
            var pockets = new Item[40];
            for (var i = 0; i < 20; i++)
            {
                pockets[i] = new Item(personalSave, offsets.Pockets + 0xB8 + i * 8);
                pockets[i + 20] = new Item(personalSave, offsets.Pockets + i * 8);
            }

            Pockets = new ItemCollection(pockets);

            var storage = new Item[5000];
            for (var i = 0; i < 5000; i++)
                storage[i] = new Item(personalSave, offsets.Storage + i * 8);
            Storage = new ItemCollection(storage);
        }

        public void Save()
        {
            var offsets = GetOffsetsFromRevision();
            _personalFile.WriteStruct(offsets.PersonalId, PersonalId);
            Wallet.Write(_personalFile, offsets.Wallet);
            Bank.Write(_personalFile, offsets.Bank);
            NookMiles.Write(_personalFile, offsets.NookMiles);

            for (var i = 0; i < 20; i++)
            {
                Pockets[i].Save(_personalFile, offsets.Pockets + 0xB8 + i * 8);
                Pockets[i + 20].Save(_personalFile, offsets.Pockets + i * 8);
            }

            for (var i = 0; i < 5000; i++)
                Storage[i].Save(_personalFile, offsets.Storage + i * 8);
        }

        public string GetName() => PersonalId.GetName();
        public void SetName(in string newName) => PersonalId.SetName(newName);

        public byte[] GetPhotoData()
        {
            var offset = GetOffsetsFromRevision().Photo;
            if (_personalFile.ReadU16(offset) != 0xD8FF)
                return null;
            // TODO: Determine actual size buffer instead of using this.
            var size = 2;
            while (_personalFile.ReadU16(offset + size) != 0xD9FF)
                size++;
            return _personalFile.ReadArray<byte>(offset, size + 2);
        }

        public override string ToString() => GetName();
    }
}
