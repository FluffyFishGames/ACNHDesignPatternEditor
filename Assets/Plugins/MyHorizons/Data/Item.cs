using MyHorizons.Data.Save;

namespace MyHorizons.Data
{
    public sealed class Item
    {
        public static readonly Item NO_ITEM = new Item(0xFFFE, 0, 0, 0, 0, 0);

        private static readonly ushort[] resolvedItemIdArray =
        {
            0x1E13, 0x1E14, 0x1E15, 0x1E16, 0x1E17, 0x1E18, 0x1E19, 0x1E1A,
            0x1E1B, 0x1E1C, 0x1E1D, 0x1E1E, 0x1E1F, 0x1E20, 0x1E21, 0x1E22
        };

        public enum Type
        {
            Nothing = 0,
            Reserved = 1,
            Present = 2,
            Delivery = 3
        };

        public ushort ItemId;
        public byte Flags0;
        public byte Flags1;
        public byte Flags2;
        public byte Flags3;
        public ushort UseCount;

        public Item(ushort itemId, byte flags0, byte flags1, byte flags2, byte flags3, ushort useCount)
        {
            ItemId = itemId;
            Flags0 = flags0;
            Flags1 = flags1;
            Flags2 = flags2;
            Flags3 = flags3;
            UseCount = useCount;
        }

        public Item(ISaveFile save, int offset)
            : this(save.ReadU16(offset + 0), save.ReadU8(offset + 2), save.ReadU8(offset + 3),
                   save.ReadU8(offset + 4), save.ReadU8(offset + 5), save.ReadU16(offset + 6)) { }

        public void Save(ISaveFile save, int offset)
        {
            save.WriteU16(offset, ItemId);
            save.WriteU8(offset + 2, Flags0);
            save.WriteU8(offset + 3, Flags1);
            save.WriteU8(offset + 4, Flags3);
            save.WriteU8(offset + 5, Flags2);
            save.WriteU16(offset + 6, UseCount);
        }

        public ushort GetInventoryNameFromFlags()
        {
            if ((Flags1 & 3) != 0 && ItemId != 0x16A1 && ItemId != 0x3100)
            {
                switch ((Type)(Flags1 & 3))
                {
                    case Type.Reserved:
                        return resolvedItemIdArray[(Flags1 >> 2) & 0xF];
                    case Type.Present:
                        return 0x1180;
                    case Type.Delivery:
                        return 0x1225;
                }
            }
            return ItemId;
        }

        public Item Clone() => new Item(ItemId, Flags0, Flags1, Flags2, Flags3, UseCount);

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (ReferenceEquals(obj, null))
            {
                return false;
            }

            if (obj is Item other)
                return ItemId == other.ItemId && Flags0 == other.Flags0 && Flags1 == other.Flags1
                    && Flags3 == other.Flags3 && Flags2 == other.Flags2 && UseCount == other.UseCount;
            return false;
        }

        public static bool operator ==(Item a, Item b) => a?.Equals(b) ?? false;

        public static bool operator !=(Item a, Item b) => !(a == b);

        public override int GetHashCode() => ((base.GetHashCode() << 2) ^ ItemId) << ((Flags0 + Flags1 + Flags3) & 7) ^ (Flags2 << UseCount & 0x1F);
    }
}
