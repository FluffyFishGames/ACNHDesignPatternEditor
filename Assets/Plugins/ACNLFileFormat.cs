using System;
using System.Collections.Generic;

public class ACNLFileFormat
{
	public int Width;
	public int Height;
	public string Name;
	public string Username;
	public string TownName;
	public byte[] Palette = new byte[15];
	public byte[] Pixels;
	public byte PatternType;
	public DesignPattern.TypeEnum Type;
	public bool IsPro;

	public static DesignPattern.Color[] Colors = new DesignPattern.Color[] 
	{
		new DesignPattern.Color() { R = 0xFF, G = 0xEE, B = 0xFF }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x99, B = 0xAA }, 
		new DesignPattern.Color() { R = 0xEE, G = 0x55, B = 0x99 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x66, B = 0xAA }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x00, B = 0x66 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x44, B = 0x77 }, 
		new DesignPattern.Color() { R = 0xCC, G = 0x00, B = 0x55 }, 
		new DesignPattern.Color() { R = 0x99, G = 0x00, B = 0x33 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x22, B = 0x33 },
		null,null,null,null,null,null,//0x09-0x0E unused / unknown
		new DesignPattern.Color() { R = 0xFF, G = 0xFF, B = 0xFF }, //0x0F: Grey 1
		//Reds (0x10 - 0x18)
		new DesignPattern.Color() { R = 0xFF, G = 0xBB, B = 0xCC }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x77, B = 0x77 }, 
		new DesignPattern.Color() { R = 0xDD, G = 0x32, B = 0x10 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x55, B = 0x44 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x00, B = 0x00 }, 
		new DesignPattern.Color() { R = 0xCC, G = 0x66, B = 0x66 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x44, B = 0x44 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x00, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x88, G = 0x22, B = 0x22 },
		null,null,null,null,null,null,//0x19-0x1E unused / unknown
		new DesignPattern.Color() { R = 0xEE, G = 0xEE, B = 0xEE }, //0x1F: Grey 2
		//Oranges (0x20 - 0x28)
		new DesignPattern.Color() { R = 0xDD, G = 0xCD, B = 0xBB }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xCD, B = 0x66 }, 
		new DesignPattern.Color() { R = 0xDD, G = 0x66, B = 0x22 },
		new DesignPattern.Color() { R = 0xFF, G = 0xAA, B = 0x22 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x66, B = 0x00 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x88, B = 0x55 },
		new DesignPattern.Color() { R = 0xDD, G = 0x44, B = 0x00 },
		new DesignPattern.Color() { R = 0xBB, G = 0x44, B = 0x00 },
		new DesignPattern.Color() { R = 0x66, G = 0x32, B = 0x10 },
		null,null,null,null,null,null,//0x29-0x2E unused / unknown
		new DesignPattern.Color() { R = 0xDD, G = 0xDD, B = 0xDD }, //0x2F: Grey 3
		//Pastels or something, I guess? (0x30 - 0x38)
		new DesignPattern.Color() { R = 0xFF, G = 0xEE, B = 0xDD }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xDD, B = 0xCC }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xCD, B = 0xAA }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xBB, B = 0x88 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xAA, B = 0x88 }, 
		new DesignPattern.Color() { R = 0xDD, G = 0x88, B = 0x66 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x66, B = 0x44 }, 
		new DesignPattern.Color() { R = 0x99, G = 0x55, B = 0x33 }, 
		new DesignPattern.Color() { R = 0x88, G = 0x44, B = 0x22 },
		null,null,null,null,null,null,//0x39-0x3E unused / unknown
		new DesignPattern.Color() { R = 0xCC, G = 0xCD, B = 0xCC }, //0x3F: Grey 4
		//Purple (0x40 - 0x48)
		new DesignPattern.Color() { R = 0xFF, G = 0xCD, B = 0xFF }, 
		new DesignPattern.Color() { R = 0xEE, G = 0x88, B = 0xFF }, 
		new DesignPattern.Color() { R = 0xCC, G = 0x66, B = 0xDD },
		new DesignPattern.Color() { R = 0xBB, G = 0x88, B = 0xCC }, 
		new DesignPattern.Color() { R = 0xCC, G = 0x00, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x99, G = 0x66, B = 0x99 },
		new DesignPattern.Color() { R = 0x88, G = 0x00, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x55, G = 0x00, B = 0x77 }, 
		new DesignPattern.Color() { R = 0x33, G = 0x00, B = 0x44 },
		null,null,null,null,null,null,//0x49-0x4E unused / unknown
		new DesignPattern.Color() { R = 0xBB, G = 0xBB, B = 0xBB }, //0x4F: Grey 5
		//Pink (0x50 - 0x58)
		new DesignPattern.Color() { R = 0xFF, G = 0xBB, B = 0xFF }, 
		new DesignPattern.Color() { R = 0xFF, G = 0x99, B = 0xFF },
		new DesignPattern.Color() { R = 0xDD, G = 0x22, B = 0xBB },
		new DesignPattern.Color() { R = 0xFF, G = 0x55, B = 0xEE },
		new DesignPattern.Color() { R = 0xFF, G = 0x00, B = 0xCC }, 
		new DesignPattern.Color() { R = 0x88, G = 0x55, B = 0x77 }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x00, B = 0x99 }, 
		new DesignPattern.Color() { R = 0x88, G = 0x00, B = 0x66 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x00, B = 0x44 },
		null,null,null,null,null,null,//0x59-0x5E unused / unknown
		new DesignPattern.Color() { R = 0xAA, G = 0xAA, B = 0xAA }, //0x5F: Grey 6
		//Brown (0x60 - 0x68)
		new DesignPattern.Color() { R = 0xDD, G = 0xBB, B = 0x99 }, 
		new DesignPattern.Color() { R = 0xCC, G = 0xAA, B = 0x77 }, 
		new DesignPattern.Color() { R = 0x77, G = 0x44, B = 0x33 }, 
		new DesignPattern.Color() { R = 0xAA, G = 0x77, B = 0x44 }, 
		new DesignPattern.Color() { R = 0x99, G = 0x32, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x77, G = 0x32, B = 0x22 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x22, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x33, G = 0x10, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x10, B = 0x00 },
		null,null,null,null,null,null,//0x69-0x6E unused / unknown
		new DesignPattern.Color() { R = 0x99, G = 0x99, B = 0x99 }, //0x6F: Grey 7
		//Yellow (0x70 - 0x78)
		new DesignPattern.Color() { R = 0xFF, G = 0xFF, B = 0xCC }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xFF, B = 0x77 }, 
		new DesignPattern.Color() { R = 0xDD, G = 0xDD, B = 0x22 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xFF, B = 0x00 }, 
		new DesignPattern.Color() { R = 0xFF, G = 0xDD, B = 0x00 }, 
		new DesignPattern.Color() { R = 0xCC, G = 0xAA, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x99, G = 0x99, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x88, G = 0x77, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x55, B = 0x00 },
		null,null,null,null,null,null,//0x79-0x7E unused / unknown
		new DesignPattern.Color() { R = 0x88, G = 0x88, B = 0x88 }, //0x7F: Grey 8
		//Blue (0x80 - 0x88)
		new DesignPattern.Color() { R = 0xDD, G = 0xBB, B = 0xFF }, 
		new DesignPattern.Color() { R = 0xBB, G = 0x99, B = 0xEE }, 
		new DesignPattern.Color() { R = 0x66, G = 0x32, B = 0xCC }, 
		new DesignPattern.Color() { R = 0x99, G = 0x55, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x66, G = 0x00, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x55, G = 0x44, B = 0x88 }, 
		new DesignPattern.Color() { R = 0x44, G = 0x00, B = 0x99 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x00, B = 0x66 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x10, B = 0x33 },
		null,null,null,null,null,null,//0x89-0x8E unused / unknown
		new DesignPattern.Color() { R = 0x77, G = 0x77, B = 0x77 }, //0x8F: Grey 9
		//Ehm... also blue? (0x90 - 0x98)
		new DesignPattern.Color() { R = 0xBB, G = 0xBB, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x88, G = 0x99, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x33, G = 0x32, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x33, G = 0x55, B = 0xEE }, 
		new DesignPattern.Color() { R = 0x00, G = 0x00, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x33, G = 0x32, B = 0x88 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x00, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x10, G = 0x10, B = 0x66 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x00, B = 0x22 },
		null,null,null,null,null,null,//0x99-0x9E unused / unknown
		new DesignPattern.Color() { R = 0x66, G = 0x66, B = 0x66 }, //0x9F: Grey 10
		//Green (0xA0 - 0xA8)
		new DesignPattern.Color() { R = 0x99, G = 0xEE, B = 0xBB }, 
		new DesignPattern.Color() { R = 0x66, G = 0xCD, B = 0x77 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x66, B = 0x10 }, 
		new DesignPattern.Color() { R = 0x44, G = 0xAA, B = 0x33 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x88, B = 0x33 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x77, B = 0x55 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x55, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x10, G = 0x32, B = 0x22 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x22, B = 0x10 },
		null,null,null,null,null,null,//0xA9-0xAE unused / unknown
		new DesignPattern.Color() { R = 0x55, G = 0x55, B = 0x55 }, //0xAF: Grey 11
		//Icky greenish yellow (0xB0 - 0xB8)
		new DesignPattern.Color() { R = 0xDD, G = 0xFF, B = 0xBB }, 
		new DesignPattern.Color() { R = 0xCC, G = 0xFF, B = 0x88 }, 
		new DesignPattern.Color() { R = 0x88, G = 0xAA, B = 0x55 }, 
		new DesignPattern.Color() { R = 0xAA, G = 0xDD, B = 0x88 }, 
		new DesignPattern.Color() { R = 0x88, G = 0xFF, B = 0x00 }, 
		new DesignPattern.Color() { R = 0xAA, G = 0xBB, B = 0x99 }, 
		new DesignPattern.Color() { R = 0x66, G = 0xBB, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x55, G = 0x99, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x33, G = 0x66, B = 0x00 },
		null,null,null,null,null,null,//0xB9-0xBE unused / unknown
		new DesignPattern.Color() { R = 0x44, G = 0x44, B = 0x44 }, //0xBF: Grey 12
		//Wtf? More blue? (0xC0 - 0xC8)
		new DesignPattern.Color() { R = 0xBB, G = 0xDD, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x77, G = 0xCD, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x33, G = 0x55, B = 0x99 }, 
		new DesignPattern.Color() { R = 0x66, G = 0x99, B = 0xFF },
		new DesignPattern.Color() { R = 0x10, G = 0x77, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x44, G = 0x77, B = 0xAA },
		new DesignPattern.Color() { R = 0x22, G = 0x44, B = 0x77 },
		new DesignPattern.Color() { R = 0x00, G = 0x22, B = 0x77 },
		new DesignPattern.Color() { R = 0x00, G = 0x10, B = 0x44 },
		null,null,null,null,null,null,//0xC9-0xCE unused / unknown
		new DesignPattern.Color() { R = 0x33, G = 0x32, B = 0x33 }, //0xCF: Grey 13
		//Gonna call this cyan (0xD0 - 0xD8)
		new DesignPattern.Color() { R = 0xAA, G = 0xFF, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x55, G = 0xFF, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x00, G = 0x88, B = 0xBB }, 
		new DesignPattern.Color() { R = 0x55, G = 0xBB, B = 0xCC }, 
		new DesignPattern.Color() { R = 0x00, G = 0xCD, B = 0xFF }, 
		new DesignPattern.Color() { R = 0x44, G = 0x99, B = 0xAA },
		new DesignPattern.Color() { R = 0x00, G = 0x66, B = 0x88 },
		new DesignPattern.Color() { R = 0x00, G = 0x44, B = 0x55 },
		new DesignPattern.Color() { R = 0x00, G = 0x22, B = 0x33 },
		null,null,null,null,null,null,//0xD9-0xDE unused / unknown
		new DesignPattern.Color() { R = 0x22, G = 0x22, B = 0x22 }, //0xDF: Grey 14
		//More cyan, because we didn't have enough blue-like colors yet (0xE0 - 0xE8)
		new DesignPattern.Color() { R = 0xCC, G = 0xFF, B = 0xEE }, 
		new DesignPattern.Color() { R = 0xAA, G = 0xEE, B = 0xDD }, 
		new DesignPattern.Color() { R = 0x33, G = 0xCD, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x55, G = 0xEE, B = 0xBB }, 
		new DesignPattern.Color() { R = 0x00, G = 0xFF, B = 0xCC }, 
		new DesignPattern.Color() { R = 0x77, G = 0xAA, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x00, G = 0xAA, B = 0x99 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x88, B = 0x77 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x44, B = 0x33 },
		null,null,null,null,null,null,//0xE9-0xEE unused / unknown
		new DesignPattern.Color() { R = 0x00, G = 0x00, B = 0x00 }, //0xEF: Grey 15
		//Also green. Fuck it, whatever. (0xF0 - 0xF8)
		new DesignPattern.Color() { R = 0xAA, G = 0xFF, B = 0xAA }, 
		new DesignPattern.Color() { R = 0x77, G = 0xFF, B = 0x77 }, 
		new DesignPattern.Color() { R = 0x66, G = 0xDD, B = 0x44 }, 
		new DesignPattern.Color() { R = 0x00, G = 0xFF, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x22, G = 0xDD, B = 0x22 }, 
		new DesignPattern.Color() { R = 0x55, G = 0xBB, B = 0x55 }, 
		new DesignPattern.Color() { R = 0x00, G = 0xBB, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x00, G = 0x88, B = 0x00 }, 
		new DesignPattern.Color() { R = 0x22, G = 0x44, B = 0x22 },
		null,null,null,null,null,null,//0xF9-0xFE unused / unknown
		null, //0xFF unused (white in-game, editing freezes the game)
	};

	private static Dictionary<(int, int), int> Offsets = new Dictionary<(int, int), int>()
	{
		// front
		{ ( 0x000, 0x200), 0x200 }, 
		// back
		{ ( 0x200, 0x400), 0x000 },
		// front bottom
		{ ( 0x600, 0x700), 0x600 },
		// back bottom
		{ ( 0x700, 0x800), 0x400 },
		// left sleeve
		{ ( 0x400, 0x500), 0x500 },
		// right sleeve
		{ ( 0x500, 0x600), 0x700 },
	};

	private static int TransformIndex(int index)
	{
		foreach (var kv in Offsets)
		{
			if (index >= kv.Key.Item1 && index < kv.Key.Item2)
			{
				return (index - kv.Key.Item1) + kv.Value;
			}
		}
		return index;
	}

	private static int InverseTransformIndex(int index)
	{
		foreach (var kv in Offsets)
		{
			if (index >= kv.Value && index < kv.Value + (kv.Key.Item2 - kv.Key.Item1))
			{
				return (index - kv.Value) + kv.Key.Item1;
			}
		}
		return index;
	}

	public static byte GetNearestColor(byte r, byte g, byte b)
	{
		var shorted = -1;
		int shortedIndex = -1;
		for (int i = 0; i < Colors.Length; i++)
		{
			if (Colors[i] != null)
			{
				var distance = UnityEngine.Mathf.Abs(Colors[i].R - r) + UnityEngine.Mathf.Abs(Colors[i].G - g) + UnityEngine.Mathf.Abs(Colors[i].B - b);
				if (shorted == -1 || shorted > distance)
				{
					shorted = distance;
					shortedIndex = i;
				}
			}
		}
		return (byte) shortedIndex;
	}

	public static ACNLFileFormat FromPattern(DesignPattern pattern)
	{

		var result = new ACNLFileFormat();
		result.Name = pattern.Name;
		result.PatternType = 0xFF;
		result.Type = pattern.Type;
		if (pattern.Type == DesignPattern.TypeEnum.LongSleeveDress3DS)
			result.PatternType = 0x00;
		else if (pattern.Type == DesignPattern.TypeEnum.ShortSleeveDress3DS)
			result.PatternType = 0x01;
		else if (pattern.Type == DesignPattern.TypeEnum.NoSleeveDress3DS)
			result.PatternType = 0x02;
		else if (pattern.Type == DesignPattern.TypeEnum.LongSleeveShirt3DS)
			result.PatternType = 0x03;
		else if (pattern.Type == DesignPattern.TypeEnum.ShortSleeveShirt3DS)
			result.PatternType = 0x04;
		else if (pattern.Type == DesignPattern.TypeEnum.NoSleeveShirt3DS)
			result.PatternType = 0x05;
		else if (pattern.Type == DesignPattern.TypeEnum.HornHat3DS)
			result.PatternType = 0x06;
		else if (pattern.Type == DesignPattern.TypeEnum.Hat3DS)
			result.PatternType = 0x07;
		else if (pattern.Type == DesignPattern.TypeEnum.SimplePattern)
			result.PatternType = 0x09;

		result.Palette = new byte[15];
		for (int i = 0; i < pattern.Palette.Length; i++)
			result.Palette[i] = GetNearestColor(pattern.Palette[i].R, pattern.Palette[i].G, pattern.Palette[i].B);

		result.IsPro = result.PatternType != 0x09;
		if (result.IsPro)
		{
			result.Width = 64;
			result.Height = 64;
		}
		else
		{
			result.Width = 32;
			result.Height = 32;
		}
		result.Pixels = new byte[(result.Width / 2) * result.Height];
		Array.Copy(pattern.Image, result.Pixels, result.Pixels.Length);
		return result;
	}

	public byte[] ToBytes()
	{
		int length = 0x870;
		bool isShort = false;
		if (this.PatternType == 0x06 || this.PatternType == 0x07 || this.PatternType == 0x09)
		{
			length = 0x26C;
			isShort = true;
		}
		byte[] ret = new byte[length];

		if (Name != null)
		{
			var nameBytes = System.Text.Encoding.Unicode.GetBytes(Name);
			Array.Copy(nameBytes, 0, ret, 0, nameBytes.Length);
		}
		else
		{
			var nameBytes = System.Text.Encoding.Unicode.GetBytes("Unknown");
			Array.Copy(nameBytes, 0, ret, 0, nameBytes.Length);
		}

		ret[0x2A] = 0x4C;
		ret[0x2B] = 0xD0;
		if (Username != null)
		{
			var usernameBytes = System.Text.Encoding.Unicode.GetBytes(Username);
			Array.Copy(usernameBytes, 0, ret, 0x2c, usernameBytes.Length);
		}
		else
		{
			var usernameBytes = System.Text.Encoding.Unicode.GetBytes("Unknown");
			Array.Copy(usernameBytes, 0, ret, 0x2c, usernameBytes.Length);
		}

		ret[0x40] = 0xC5;
		ret[0x41] = 0xF0;
		if (TownName != null)
		{
			var townBytes = System.Text.Encoding.Unicode.GetBytes(TownName);
			Array.Copy(townBytes, 0, ret, 0x42, townBytes.Length);
		}
		else
		{
			var townBytes = System.Text.Encoding.Unicode.GetBytes("Unknown");
			Array.Copy(townBytes, 0, ret, 0x42, townBytes.Length);

		}

		ret[0x56] = 0x31;
		ret[0x57] = 0x0B;

		for (int i = 0; i < this.Palette.Length; i++)
		{
			ret[0x58 + i] = this.Palette[i];
		}
		ret[0x67] = 0x67;
		ret[0x68] = 0x00;

		ret[0x69] = this.PatternType;

		int width = Width;
		int height = Height;
		if (Type == DesignPattern.TypeEnum.HornHat3DS || Type == DesignPattern.TypeEnum.Hat3DS)
		{
			width = 32;
			height = 32;
		}

		if (IsPro && Type != DesignPattern.TypeEnum.HornHat3DS && Type != DesignPattern.TypeEnum.Hat3DS)
		{
			//this.Pixels = pixels;

			for (var y = 0; y < height; y++)
			{
				for (var x = 0; x < width / 2; x++)
				{
					if (Type != DesignPattern.TypeEnum.HornHat3DS && Type != DesignPattern.TypeEnum.Hat3DS)
					{
						var offset = (x >= width / 4 ? 0x200 : 0x0) + (y >= height / 2 ? 0x400 : 0x0);
						int index = offset + x % (width / 4) + (y % (height / 2)) * (width / 4);
						ret[0x6C + InverseTransformIndex(index)] = Pixels[index];
					}
//					else
//						ret[0x6C + x + y * width / 2] = Pixels[x + y * width / 2];
				}
			}
		}
		else
		{
			Array.Copy(Pixels, 0, ret, 0x6C, 0x200);
		}
		return ret;
	}

	public ACNLFileFormat()
	{

	}
	public ACNLFileFormat(byte[] bytes)
	{
		Name = System.Text.Encoding.Unicode.GetString(bytes, 0, 0x2A).Replace(" ", "").Trim();
		Username = System.Text.Encoding.Unicode.GetString(bytes, 0x2c, 20).Replace(" ", "").Trim();
		TownName = System.Text.Encoding.Unicode.GetString(bytes, 0x42, 20).Replace(" ", "").Trim();
		Palette = new byte[15];
		string kk = "";
		for (int i = 0; i < bytes.Length; i++)
		{
			kk += bytes[i].ToString("X2");
		}
		UnityEngine.Debug.Log(kk);
		for (int i = 0; i < 15; i++)
			Palette[i] = bytes[0x58 + i];

		PatternType = bytes[0x69];
		if (PatternType == 0x00)
			Type = DesignPattern.TypeEnum.LongSleeveDress3DS;
		else if (PatternType == 0x01)
			Type = DesignPattern.TypeEnum.ShortSleeveDress3DS;
		else if (PatternType == 0x02)
			Type = DesignPattern.TypeEnum.NoSleeveDress3DS;
		else if (PatternType == 0x03)
			Type = DesignPattern.TypeEnum.LongSleeveShirt3DS;
		else if (PatternType == 0x04)
			Type = DesignPattern.TypeEnum.ShortSleeveShirt3DS;
		else if (PatternType == 0x05)
			Type = DesignPattern.TypeEnum.NoSleeveShirt3DS;
		else if (PatternType == 0x06)
			Type = DesignPattern.TypeEnum.HornHat3DS;
		else if (PatternType == 0x07)
			Type = DesignPattern.TypeEnum.Hat3DS;
		else if (PatternType == 0x09)
			Type = DesignPattern.TypeEnum.SimplePattern;
		else
		{
			Type = DesignPattern.TypeEnum.Unsupported;
		}
		IsPro = Type != DesignPattern.TypeEnum.SimplePattern;

		if (IsPro)
		{
			Width = 64;
			Height = 64;
		}
		else
		{
			Width = 32;
			Height = 32;
		}
		this.Pixels = new byte[(Width / 2) * Height];
		if (IsPro && Type != DesignPattern.TypeEnum.HornHat3DS && Type != DesignPattern.TypeEnum.Hat3DS)
		{
			byte[] pixels = new byte[(Width / 2) * Height];
			
			Array.Copy(bytes, 0x6C, pixels, 0, UnityEngine.Mathf.Min(bytes.Length - 0x6C, pixels.Length));
			//this.Pixels = pixels;

			for (var y = 0; y < Height; y++)
			{
				for (var x = 0; x < Width / 2; x++)
				{
					var offset = (x >= this.Width / 4 ? 0x200 : 0x0) + (y >= this.Height / 2 ? 0x400 : 0x0);
					//if (Type != DesignPattern.TypeEnum.HornHat3DS && Type != DesignPattern.TypeEnum.Hat3DS)
					//{
					var index = offset + x % (this.Width / 4) + (y % (this.Height / 2)) * (this.Width / 4);
					this.Pixels[TransformIndex(index)] = pixels[index];
					//}
					//else 
					//	this.Pixels[x + y * this.Width / 2] = pixels[offset + x % (this.Width / 4) + (y % (this.Height / 2)) * (this.Width / 4)];
				}
			}
		}
		else
		{
			if (Type == DesignPattern.TypeEnum.Hat3DS || Type == DesignPattern.TypeEnum.HornHat3DS)
				Array.Copy(bytes, 0x6C, Pixels, 0, bytes.Length - 0x6C);
			else 
				Array.Copy(bytes, 0x6C, Pixels, 0, Pixels.Length);
		}
	}

	public TextureBitmap GetImage()
	{
		var bmp = new TextureBitmap(Width, Height);
		for (var y = 0; y < Height; y++)
		{
			for (var x = 0; x < Width; x++)
			{
				byte index = 0;
				if (x % 2 == 0)
					index = (byte) (Pixels[(x / 2) + y * (Width / 2)] & 0x0F);
				else
					index = (byte) ((Pixels[(x / 2) + y * (Width / 2)] & 0xF0) >> 4);

				if (index == 15)
					bmp.SetPixel(x, y, new TextureBitmap.Color(0, 0, 0, 0));
				else 
					bmp.SetPixel(x, y, new TextureBitmap.Color(Colors[Palette[index]].R, Colors[Palette[index]].G, Colors[Palette[index]].B, 255));
			}
		}
		return bmp;
	}
}