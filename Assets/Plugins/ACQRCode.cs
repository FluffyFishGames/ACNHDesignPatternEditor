using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MyHorizons.Data;

public class ACQRCode
{
	public string Name;
	public string Username;
	public string TownName;
	public byte[] Palette = new byte[15];
	public byte[] Pixels;
	public byte PatternType;

	public static DesignPattern.DesignColor[] Colors = new DesignPattern.DesignColor[] 
	{
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xEE, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x99, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0xEE, G = 0x55, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x66, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x00, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x44, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0x00, B = 0x55 }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x00, B = 0x33 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x22, B = 0x33 },
		null,null,null,null,null,null,//0x09-0x0E unused / unknown
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xFF, B = 0xFF }, //0x0F: Grey 1
		//Reds (0x10 - 0x18)
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xBB, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x77, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0xDD, G = 0x32, B = 0x10 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x55, B = 0x44 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x00, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0x66, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x44, B = 0x44 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x00, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x22, B = 0x22 },
		null,null,null,null,null,null,//0x19-0x1E unused / unknown
		new DesignPattern.DesignColor() { R = 0xEE, G = 0xEE, B = 0xEE }, //0x1F: Grey 2
		//Oranges (0x20 - 0x28)
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xCD, B = 0xBB }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xCD, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0xDD, G = 0x66, B = 0x22 },
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xAA, B = 0x22 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x66, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x88, B = 0x55 },
		new DesignPattern.DesignColor() { R = 0xDD, G = 0x44, B = 0x00 },
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x44, B = 0x00 },
		new DesignPattern.DesignColor() { R = 0x66, G = 0x32, B = 0x10 },
		null,null,null,null,null,null,//0x29-0x2E unused / unknown
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xDD, B = 0xDD }, //0x2F: Grey 3
		//Pastels or something, I guess? (0x30 - 0x38)
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xEE, B = 0xDD }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xDD, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xCD, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xBB, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xAA, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0xDD, G = 0x88, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x66, B = 0x44 }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x55, B = 0x33 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x44, B = 0x22 },
		null,null,null,null,null,null,//0x39-0x3E unused / unknown
		new DesignPattern.DesignColor() { R = 0xCC, G = 0xCD, B = 0xCC }, //0x3F: Grey 4
		//Purple (0x40 - 0x48)
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xCD, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0xEE, G = 0x88, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0x66, B = 0xDD },
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x88, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0x00, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x66, B = 0x99 },
		new DesignPattern.DesignColor() { R = 0x88, G = 0x00, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x00, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x00, B = 0x44 },
		null,null,null,null,null,null,//0x49-0x4E unused / unknown
		new DesignPattern.DesignColor() { R = 0xBB, G = 0xBB, B = 0xBB }, //0x4F: Grey 5
		//Pink (0x50 - 0x58)
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xBB, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x99, B = 0xFF },
		new DesignPattern.DesignColor() { R = 0xDD, G = 0x22, B = 0xBB },
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x55, B = 0xEE },
		new DesignPattern.DesignColor() { R = 0xFF, G = 0x00, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x55, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x00, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x00, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x00, B = 0x44 },
		null,null,null,null,null,null,//0x59-0x5E unused / unknown
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xAA, B = 0xAA }, //0x5F: Grey 6
		//Brown (0x60 - 0x68)
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xBB, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0xAA, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0x77, G = 0x44, B = 0x33 }, 
		new DesignPattern.DesignColor() { R = 0xAA, G = 0x77, B = 0x44 }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x32, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x77, G = 0x32, B = 0x22 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x22, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x10, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x10, B = 0x00 },
		null,null,null,null,null,null,//0x69-0x6E unused / unknown
		new DesignPattern.DesignColor() { R = 0x99, G = 0x99, B = 0x99 }, //0x6F: Grey 7
		//Yellow (0x70 - 0x78)
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xFF, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xFF, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xDD, B = 0x22 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xFF, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0xFF, G = 0xDD, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0xAA, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x99, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x77, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x55, B = 0x00 },
		null,null,null,null,null,null,//0x79-0x7E unused / unknown
		new DesignPattern.DesignColor() { R = 0x88, G = 0x88, B = 0x88 }, //0x7F: Grey 8
		//Blue (0x80 - 0x88)
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xBB, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0xBB, G = 0x99, B = 0xEE }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0x32, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0x99, G = 0x55, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0x00, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x44, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0x44, G = 0x00, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x00, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x10, B = 0x33 },
		null,null,null,null,null,null,//0x89-0x8E unused / unknown
		new DesignPattern.DesignColor() { R = 0x77, G = 0x77, B = 0x77 }, //0x8F: Grey 9
		//Ehm... also blue? (0x90 - 0x98)
		new DesignPattern.DesignColor() { R = 0xBB, G = 0xBB, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0x99, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x32, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x55, B = 0xEE }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x00, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x32, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x00, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x10, G = 0x10, B = 0x66 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x00, B = 0x22 },
		null,null,null,null,null,null,//0x99-0x9E unused / unknown
		new DesignPattern.DesignColor() { R = 0x66, G = 0x66, B = 0x66 }, //0x9F: Grey 10
		//Green (0xA0 - 0xA8)
		new DesignPattern.DesignColor() { R = 0x99, G = 0xEE, B = 0xBB }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0xCD, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x66, B = 0x10 }, 
		new DesignPattern.DesignColor() { R = 0x44, G = 0xAA, B = 0x33 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x88, B = 0x33 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x77, B = 0x55 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x55, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x10, G = 0x32, B = 0x22 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x22, B = 0x10 },
		null,null,null,null,null,null,//0xA9-0xAE unused / unknown
		new DesignPattern.DesignColor() { R = 0x55, G = 0x55, B = 0x55 }, //0xAF: Grey 11
		//Icky greenish yellow (0xB0 - 0xB8)
		new DesignPattern.DesignColor() { R = 0xDD, G = 0xFF, B = 0xBB }, 
		new DesignPattern.DesignColor() { R = 0xCC, G = 0xFF, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0xAA, B = 0x55 }, 
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xDD, B = 0x88 }, 
		new DesignPattern.DesignColor() { R = 0x88, G = 0xFF, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xBB, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0xBB, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0x99, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x66, B = 0x00 },
		null,null,null,null,null,null,//0xB9-0xBE unused / unknown
		new DesignPattern.DesignColor() { R = 0x44, G = 0x44, B = 0x44 }, //0xBF: Grey 12
		//Wtf? More blue? (0xC0 - 0xC8)
		new DesignPattern.DesignColor() { R = 0xBB, G = 0xDD, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x77, G = 0xCD, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0x55, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0x99, B = 0xFF },
		new DesignPattern.DesignColor() { R = 0x10, G = 0x77, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x44, G = 0x77, B = 0xAA },
		new DesignPattern.DesignColor() { R = 0x22, G = 0x44, B = 0x77 },
		new DesignPattern.DesignColor() { R = 0x00, G = 0x22, B = 0x77 },
		new DesignPattern.DesignColor() { R = 0x00, G = 0x10, B = 0x44 },
		null,null,null,null,null,null,//0xC9-0xCE unused / unknown
		new DesignPattern.DesignColor() { R = 0x33, G = 0x32, B = 0x33 }, //0xCF: Grey 13
		//Gonna call this cyan (0xD0 - 0xD8)
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xFF, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0xFF, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x88, B = 0xBB }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0xBB, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0xCD, B = 0xFF }, 
		new DesignPattern.DesignColor() { R = 0x44, G = 0x99, B = 0xAA },
		new DesignPattern.DesignColor() { R = 0x00, G = 0x66, B = 0x88 },
		new DesignPattern.DesignColor() { R = 0x00, G = 0x44, B = 0x55 },
		new DesignPattern.DesignColor() { R = 0x00, G = 0x22, B = 0x33 },
		null,null,null,null,null,null,//0xD9-0xDE unused / unknown
		new DesignPattern.DesignColor() { R = 0x22, G = 0x22, B = 0x22 }, //0xDF: Grey 14
		//More cyan, because we didn't have enough blue-like colors yet (0xE0 - 0xE8)
		new DesignPattern.DesignColor() { R = 0xCC, G = 0xFF, B = 0xEE }, 
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xEE, B = 0xDD }, 
		new DesignPattern.DesignColor() { R = 0x33, G = 0xCD, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0xEE, B = 0xBB }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0xFF, B = 0xCC }, 
		new DesignPattern.DesignColor() { R = 0x77, G = 0xAA, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0xAA, B = 0x99 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x88, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x44, B = 0x33 },
		null,null,null,null,null,null,//0xE9-0xEE unused / unknown
		new DesignPattern.DesignColor() { R = 0x00, G = 0x00, B = 0x00 }, //0xEF: Grey 15
		//Also green. Fuck it, whatever. (0xF0 - 0xF8)
		new DesignPattern.DesignColor() { R = 0xAA, G = 0xFF, B = 0xAA }, 
		new DesignPattern.DesignColor() { R = 0x77, G = 0xFF, B = 0x77 }, 
		new DesignPattern.DesignColor() { R = 0x66, G = 0xDD, B = 0x44 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0xFF, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0xDD, B = 0x22 }, 
		new DesignPattern.DesignColor() { R = 0x55, G = 0xBB, B = 0x55 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0xBB, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x00, G = 0x88, B = 0x00 }, 
		new DesignPattern.DesignColor() { R = 0x22, G = 0x44, B = 0x22 },
		null,null,null,null,null,null,//0xF9-0xFE unused / unknown
		null, //0xFF unused (white in-game, editing freezes the game)
	};

	public ACQRCode(byte[] bytes)
	{
		Name = System.Text.Encoding.Unicode.GetString(bytes, 0, 0x29).Replace(" ", "").Trim();
		Username = System.Text.Encoding.Unicode.GetString(bytes, 0x2c, 20).Replace(" ", "").Trim();
		TownName = System.Text.Encoding.Unicode.GetString(bytes, 0x42, 20).Replace(" ", "").Trim();
		Palette = new byte[15];
		for (int i = 0; i < 15; i++)
			Palette[i] = bytes[0x58 + i];

		PatternType = bytes[0x69];
		Pixels = new byte[32 * 16];
		Array.Copy(bytes, 0x6D, Pixels, 0, 32 * 16);
	}

	public Bitmap GetImage()
	{
		var bmp = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		for (var y = 0; y < 32; y++)
		{
			for (var x = 0; x < 32; x++)
			{
				byte index = 0;
				if (x % 2 == 0)
					index = (byte) (Pixels[(x / 2) + y * 16] & 0x0F);
				else
					index = (byte) ((Pixels[(x / 2) + y * 16] & 0xF0) >> 4);

				if (index == 15)
					bmp.SetPixel(x, y, Color.FromArgb(0, 0, 0, 0));
				else 
					bmp.SetPixel(x, y, Color.FromArgb(Colors[Palette[index]].R, Colors[Palette[index]].G, Colors[Palette[index]].B));
			}
		}
		return bmp;
	}
}