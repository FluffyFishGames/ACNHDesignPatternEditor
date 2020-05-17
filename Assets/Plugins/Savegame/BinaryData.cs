using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Unity.Collections.LowLevel.Unsafe;

public unsafe class BinaryData
{
	public byte* Data;
    public int Size;

    public sbyte ReadS8(int offset)
    {
        if (offset < 0 || offset + 1 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((sbyte*) (Data + offset)));
    }

    public byte ReadU8(int offset)
    {
        if (offset < 0 || offset + 1 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((byte*) (Data + offset)));
    }

    public ushort ReadU16(int offset)
    {
        if (offset < 0 || offset + 2 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((ushort*) (Data + offset)));
    }

    public short ReadS16(int offset)
    {
        if (offset < 0 || offset + 2 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((short*) (Data + offset)));
    }

    public uint ReadU32(int offset)
    {
        if (offset < 0 || offset + 4 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((uint*) (Data + offset)));
    }

    public int ReadS32(int offset)
    {
        if (offset < 0 || offset + 4 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((int*) (Data + offset)));
    }

    public ulong ReadU64(int offset)
    {
        if (offset < 0 || offset + 8 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((ulong*) (Data + offset)));
    }

    public long ReadS64(int offset)
    {
        if (offset < 0 || offset + 8 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return *(((long*) (Data + offset)));
    }

    public string ReadString(int offset, int size)
    {
        if (offset < 0 || offset + size * 2 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        return System.Text.Encoding.Unicode.GetString(ReadBytes(offset, size * 2)).Trim('\0');
        //return Marshal.PtrToStringUni(new IntPtr(Data + offset), size * 2).Trim('\0');
        /*byte[] byteData = new byte[size * 2];
        fixed (byte* bytePtr = &byteData[0])
            Buffer.MemoryCopy(Data + offset, bytePtr, size * 10, size * 10);

        return Encoding.Unicode.GetString(byteData).Trim('\0');*/
    }

    public unsafe byte[] ReadBytes(int offset, int length)
    {
        if (offset < 0 || offset + length > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        if (length == 0) return new byte[] { };
        byte[] ret = new byte[length];
        fixed (byte* retPtr = &ret[0])
            System.Buffer.MemoryCopy(this.Data + offset, retPtr, length, length);
        return ret;
    }

    public void WriteS8(int offset, sbyte value)
    {
        if (offset < 0 || offset + 1 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((sbyte*) (Data + offset))) = value;
    }

    public void WriteU8(int offset, byte value)
    {
        if (offset < 0 || offset + 1 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((byte*) (Data + offset))) = value;
    }

    public void WriteU16(int offset, ushort value)
    {
        if (offset < 0 || offset + 2 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((ushort*) (Data + offset))) = value;
    }

    public void WriteS16(int offset, short value)
    {
        if (offset < 0 || offset + 2 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((short*) (Data + offset))) = value;
    }

    public void WriteU32(int offset, uint value)
    {
        if (offset < 0 || offset + 4 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((uint*) (Data + offset))) = value;
    }

    public void WriteS32(int offset, int value)
    {
        if (offset < 0 || offset + 4 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((int*) (Data + offset))) = value;
    }

    public void WriteU64(int offset, ulong value)
    {
        if (offset < 0 || offset + 8 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((ulong*) (Data + offset))) = value;
    }

    public void WriteS64(int offset, long value)
    {
        if (offset < 0 || offset + 8 > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        *(((long*) (Data + offset))) = value;
    }

    public void WriteBytes(int offset, byte[] bytes)
    {
        if (offset < 0 || offset + bytes.Length > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
        fixed (byte* bytesPtr = &bytes[0])
            System.Buffer.MemoryCopy(bytesPtr, Data + offset, bytes.Length, bytes.Length);
    }

    public void WriteString(int offset, string value, int fixedLength = 0)
    {
        if (value == null)
            value = "";
        var bytes = System.Text.Encoding.Unicode.GetBytes(value);
        if (bytes.Length > 0)
        {
            if (offset < 0 || offset + bytes.Length > Size) throw new IndexOutOfRangeException(offset + " is outside of binary data.");
            fixed (byte* bytesPtr = &bytes[0])
                System.Buffer.MemoryCopy(bytesPtr, Data + offset, bytes.Length, bytes.Length);
        }
        if (fixedLength > 0)
            for (int i = bytes.Length; i < fixedLength * 2; i++)
                *(Data + offset + i) = 0x00;
    }
}
