using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1, Size = 0x8A8)]
public unsafe class ProDesignPattern : DesignPattern
{
	public string _Name;
	public PersonalID _PersonalID;
	public DesignPattern.Color[] _Palette;
	public byte[] _Image;
	public byte _Type;

	public override int Width => 64;
	public override int Height => 64;
	public override string Name { get => _Name; set => _Name = value; }
	public override PersonalID PersonalID { get => _PersonalID; set => _PersonalID = value; }
	public override DesignPattern.TypeEnum Type { get => (DesignPattern.TypeEnum) _Type; set => _Type = (byte) value; }
	public override DesignPattern.Color[] Palette { get => _Palette; set => _Palette = value; }
	public override byte[] Image { get => _Image; set => _Image = value; }

	public void Write(BinaryData data, int offset)
	{
		data.WriteString(offset + 0x10, this._Name, 20);
		_PersonalID.Write(data, offset + 0x38);
		for (int i = 0; i < _Palette.Length; i++)
			_Palette[i].Write(data, offset + 0x78 + 0x03 * i);
		data.WriteBytes(offset + 0xA5, this._Image);
		data.WriteU8(offset + 0x8A5, this._Type);
	}

	public static ProDesignPattern Read(BinaryData data, int offset)
	{
		var ret = new ProDesignPattern();
		ret._Name = data.ReadString(offset + 0x10, 20);
		ret._PersonalID = PersonalID.Read(data, offset + 0x38);
		ret._Palette = new DesignPattern.Color[15];
		for (int i = 0; i < ret._Palette.Length; i++)
			ret._Palette[i] = DesignPattern.Color.Read(data, offset + 0x78 + 0x03 * i);
		ret._Image = data.ReadBytes(offset + 0xA5, 0x800);
		ret._Type = data.ReadU8(offset + 0x8A5);
		if (ret._Type == (byte) DesignPattern.TypeEnum.Standee)
        {
			var c = "";
			for (var y = 0; y < 64; y++)
			{ 
				for (var x = 0; x < 64; x++)
                {
					c += ret.GetPixel(x, y) + " ";
                }
				c += "\r\n";
            }
			UnityEngine.Debug.Log(c);
        }
		return ret;
	}

	override protected int GetIndex(int x, int y)
	{
		x = x / 2;
		var offset = (x >= this.Width / 4 ? 0x200 : 0x0) + (y >= this.Height / 2 ? 0x400 : 0x0);
		return offset + (x % (this.Width / 4)) + (y % (this.Height / 2)) * (this.Width / 4);
	}
}
