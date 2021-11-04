using System;
using System.IO;
using System.Runtime.InteropServices;

public abstract class DesignPattern
{
	public abstract string Name { get; set; }
	public PersonalID PersonalID { get => _PersonalID; set => _PersonalID = value; }
	protected PersonalID _PersonalID;
	public abstract DesignPattern.TypeEnum Type { get; set; }
	public abstract DesignPattern.Color[] Palette { get; set; }
	public abstract byte[] Image { get; set; }
	public abstract int Width { get; }
	public abstract int Height { get; }
	public int Index { get; set; }

	public void ChangeOwnership(PersonalID personalID)
	{
		var newPersonalID = new PersonalID();
		newPersonalID.Name = personalID.Name;
		newPersonalID.UniqueId = 0xFFFFFFFF;
		var townID = new TownID();
		townID.Name = personalID.TownId.Name;
		townID.UniqueID = personalID.TownId.UniqueID;
		townID.Unknown = personalID.TownId.Unknown;
		newPersonalID.TownId = townID;
		PersonalID = newPersonalID;
	}

	public virtual byte this[int index]
	{
		get
		{
			int x = index % Width;
			int y = index / Width;
			return GetPixel(x, y);
		}
		set
		{
			int x = index % Width;
			int y = index / Width;
			SetPixel(x, y, value);
		}
	}

	[StructLayout(LayoutKind.Sequential, Pack = 1, Size = 0x03)]
	public class Color
	{
		public Color()
		{

		}

		public void Write(BinaryData data, int offset)
		{
			data.WriteU8(offset, R);
			data.WriteU8(offset + 1, G);
			data.WriteU8(offset + 2, B);
		}

		public static Color Read(BinaryData data, int offset)
		{
			var ret = new Color();
			ret.R = data.ReadU8(offset);
			ret.G = data.ReadU8(offset + 1);
			ret.B = data.ReadU8(offset + 2);
			return ret;
		}

		public byte R;
		public byte G;
		public byte B;
	}

	public enum TypeEnum : byte
	{
		SimplePattern       = 0x00,
		EmptyProPattern     = 0x01,
		SimpleShirt         = 0x02,
		LongSleeveShirt     = 0x03,
		TShirt              = 0x04,
		Tanktop             = 0x05,
		Pullover            = 0x06,
		Hoodie              = 0x07,
		Coat                = 0x08,
		ShortSleeveDress    = 0x09,
		SleevelessDress     = 0x0A,
		LongSleeveDress     = 0x0B,
		BalloonDress        = 0x0C,
		RoundDress          = 0x0D,
		Robe                = 0x0E,
		BrimmedCap          = 0x0F,
		KnitCap             = 0x10,
		BrimmedHat          = 0x11,
		ShortSleeveDress3DS = 0x12,
		LongSleeveDress3DS  = 0x13,
		NoSleeveDress3DS    = 0x14,
		ShortSleeveShirt3DS = 0x15,
		LongSleeveShirt3DS  = 0x16,
		NoSleeveShirt3DS    = 0x17,
		Hat3DS              = 0x18,
		HornHat3DS          = 0x19,
		Standee3DS          = 0x1E,
		Standee             = 0x1A,
		Umbrella            = 0x1B,
		Flag                = 0x1C,
		Fan                 = 0x1D,
		Unsupported         = 0xFF
	}

	public DesignPattern()
	{
		Image = new byte[(Width / 2) * Height];
		Palette = new Color[15];
		for (int i = 0; i < 15; i++)
			Palette[i] = new DesignPattern.Color();
	}

	protected abstract int GetIndex(int x, int y);

	public virtual byte GetPixel(int x, int y)
	{
		if (x < 0 || x >= Width)
			throw new ArgumentException("Argument out of range (0-" + Width + ")", "x");
		if (y < 0 || y >= Height)
			throw new ArgumentException("Argument out of range (0-" + Height + ")", "y");

		if (x % 2 == 0)
			return (byte) (Image[GetIndex(x, y)] & 0x0F);
		else
			return (byte) ((Image[GetIndex(x, y)] & 0xF0) >> 4);
	}

	public virtual void SetPixel(int x, int y, byte colorIndex)
    {
        if (x < 0 || x >= Width)
            throw new ArgumentException("Argument out of range (0-"+Width+")", "x");
        if (y < 0 || y >= Height)
            throw new ArgumentException("Argument out of range (0-"+Height+")", "y");
        if (colorIndex > 15)
            throw new ArgumentException("Argument out of range (0-15)", "paletteColorIndex");

		var index = GetIndex(x, y);
        if (x % 2 == 0)
            Image[index] = (byte) ((colorIndex & 0x0F) | Image[index] & 0xF0);
        else
			Image[index] = (byte) (((colorIndex * 0x10) & 0xF0) | Image[index] & 0x0F);
    }
}
