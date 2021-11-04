using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

//[StructLayout(LayoutKind.Explicit, CharSet = CharSet.Unicode, Pack = 1, Size = 0x2A8)]
public unsafe class SimpleDesignPattern : DesignPattern
{
	private string _Name;
	private DesignPattern.Color[] _Palette;
	private byte[] _Image;
	private byte _Type;

	public override int Width => 32;
	public override int Height => 32;
	public override string Name { get => _Name; set => _Name = value; }
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
		data.WriteU8(offset + 0x2A5, this._Type);
	}

	public static SimpleDesignPattern Read(BinaryData data, int offset)
	{
		var ret = new SimpleDesignPattern();
		ret._Name = data.ReadString(offset + 0x10, 20);
		ret._PersonalID = PersonalID.Read(data, offset + 0x38);
		ret._Palette = new DesignPattern.Color[15];
		for (int i = 0; i < ret._Palette.Length; i++)
			ret._Palette[i] = DesignPattern.Color.Read(data, offset + 0x78 + 0x03 * i);
		ret._Image = data.ReadBytes(offset + 0xA5, 0x200);
		ret._Type = data.ReadU8(offset + 0x2A5);
		return ret;
	}

	override protected int GetIndex(int x, int y)
	{
		x = x / 2;
		return x + y * (this.Width / 2);
	}
}
