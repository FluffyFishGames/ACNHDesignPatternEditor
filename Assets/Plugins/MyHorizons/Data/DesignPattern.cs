using System;
using MyHorizons.Data.PlayerData;
using MyHorizons.Data.Save;

namespace MyHorizons.Data
{
    public class DesignPattern
    {
        public string Name;
        public PersonalID PersonalID;
        public readonly int Index;
        public readonly DesignColor[] Palette = new DesignColor[15];
        public readonly byte[] Pixels = new byte[32 * 16];

        private readonly int Offset;

        public class DesignColor
        {
			public DesignColor()
			{

			}

            public DesignColor(int offset)
            {
                var save = MainSaveFile.Singleton();
                R = save.ReadU8(offset + 0);
                G = save.ReadU8(offset + 1);
                B = save.ReadU8(offset + 2);
            }

            public byte R;
            public byte G;
            public byte B;
        }

        private readonly struct Offsets
        {
            public readonly int BaseOffset;
            public readonly int Size;

            public readonly int Name;
            public readonly int PersonalID;
            public readonly int Palette;
            public readonly int Image;

            public Offsets(int baseOffset, int size, int name, int personalID, int palette, int image)
            {
                BaseOffset = baseOffset;
                Size = size;
                Name = name;
                PersonalID = personalID;
                Palette = palette;
                Image = image;
            }
        }

        private static readonly Offsets[] DesignPatternOffsetsByRevision =
        {
            new Offsets(0x1D7310, 0x2A8, 0x10, 0x38, 0x78, 0xA5), // not entirely sure
            new Offsets(0x1D7310, 0x2A8, 0x10, 0x38, 0x78, 0xA5)
        };

        private static Offsets GetOffsetsFromRevision() => DesignPatternOffsetsByRevision[MainSaveFile.Singleton().GetRevision()];

		public DesignPattern()
		{
			for (int i = 0; i < 15; i++)
				Palette[i] = new DesignColor();
		}

        public DesignPattern(int idx)
        {
            Index = idx;
            var save = MainSaveFile.Singleton();
            var offsets = GetOffsetsFromRevision();
            Offset = offsets.BaseOffset + idx * offsets.Size;

            Name = save.ReadString(Offset + offsets.Name, 20);
            PersonalID = save.ReadStruct<PersonalID>(Offset + offsets.PersonalID);

            for (int i = 0; i < 15; i++)
                Palette[i] = new DesignColor(Offset + offsets.Palette + i * 3);

            this.Pixels = save.ReadArray<byte>(Offset + offsets.Image, this.Pixels.Length);
        }

        public byte GetPixel(int x, int y)
        {
            if (x < 0 || x > 31)
                throw new ArgumentException("Argument out of range (0-32)", "x");
            if (y < 0 || y > 31)
                throw new ArgumentException("Argument out of range (0-32)", "y");

            if (x % 2 == 0)
                return (byte) (Pixels[(x / 2) + y * 16] & 0x0F);
            else
                return (byte) ((Pixels[(x / 2) + y * 16] & 0xF0) >> 4);
        }

        public void SetPixel(int x, int y, byte paletteColorIndex)
        {
            if (x < 0 || x > 31)
                throw new ArgumentException("Argument out of range (0-32)", "x");
            if (y < 0 || y > 31)
                throw new ArgumentException("Argument out of range (0-32)", "y");
            if (paletteColorIndex > 15)
                throw new ArgumentException("Argument out of range (0-15)", "paletteColorIndex");

            var index = (x / 2) + y * 16;
            if (x % 2 == 0)
                Pixels[index] = (byte) ((paletteColorIndex & 0x0F) | Pixels[index] & 0xF0);
            else
                Pixels[index] = (byte) (((paletteColorIndex * 0x10) & 0xF0) | Pixels[index] & 0x0F);
        }

        public void Save()
        {
            var save = MainSaveFile.Singleton();
            var offsets = GetOffsetsFromRevision();

            save.WriteString(Offset + offsets.Name, this.Name, 20);
            save.WriteStruct<PersonalID>(Offset + offsets.PersonalID, this.PersonalID);

            for (int i = 0; i < 15; i++)
            {
                save.WriteU8(Offset + offsets.Palette + i * 3 + 0, this.Palette[i].R);
                save.WriteU8(Offset + offsets.Palette + i * 3 + 1, this.Palette[i].G);
                save.WriteU8(Offset + offsets.Palette + i * 3 + 2, this.Palette[i].B);
            }

            save.WriteArray<byte>(Offset + offsets.Image, this.Pixels);
        }
    }
}
