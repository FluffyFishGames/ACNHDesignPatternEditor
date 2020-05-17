using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public unsafe class Savegame : BinaryData, IDesignPatternContainer
{
    public SimpleDesignPattern[] SimpleDesignPatterns { get => _SimpleDesignPatterns; set => _SimpleDesignPatterns = value; }
    public ProDesignPattern[] ProDesignPatterns { get => _ProDesignPatterns; set => _ProDesignPatterns = value; }
    public PersonalID PersonalID => _PersonalID;

    private FileInfo HeaderFile;
    private FileInfo MainFile;
    private IntPtr RawData;
    private SavegameInfo.Info Info;

    private SimpleDesignPattern[] _SimpleDesignPatterns = new SimpleDesignPattern[50];
    private ProDesignPattern[] _ProDesignPatterns = new ProDesignPattern[50];
    private PersonalID _PersonalID;

    public Savegame(FileInfo file)
	{
        MainFile = new FileInfo(Path.Combine(file.Directory.FullName, "main.dat"));
        HeaderFile = new FileInfo(Path.Combine(file.Directory.FullName, "mainHeader.dat"));
        if (!MainFile.Exists || !HeaderFile.Exists)
            throw new Exception("Savegame is incomplete.");

        Load();
    }

    public void Load()
    {
        byte[] headerBytes = File.ReadAllBytes(HeaderFile.FullName);
        byte[] mainBytes = File.ReadAllBytes(MainFile.FullName);
        Info = SavegameInfo.GetInfo(headerBytes);
        if (Info == null)
            throw new Exception("Game version is not supported.");
        try
        {
            var bytes = SaveEncryption.Decrypt(headerBytes, mainBytes);
            System.IO.File.WriteAllBytes("test.dat", bytes);
            Size = bytes.Length;
            RawData = Marshal.AllocHGlobal(Size);
            Data = (byte*) RawData.ToPointer();
            fixed (byte* bytesPtr = &bytes[0])
                System.Buffer.MemoryCopy(bytesPtr, Data, Size, Size);
            ParseData();
        }
        catch (Exception e)
        {
            UnityEngine.Debug.LogException(e);
            throw new Exception("Error while parsing savegame.");
        }
    }

    private void ParseData()
    {
        _PersonalID = PersonalID.Read(this, 0x124);
        for (int i = 0; i < _SimpleDesignPatterns.Length; i++)
        {
            _SimpleDesignPatterns[i] = SimpleDesignPattern.Read(this, Info.SimpleDesignPatternsOffset + i * 0x2A8);
            _SimpleDesignPatterns[i].Index = i;
        }
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
        {
            _ProDesignPatterns[i] = ProDesignPattern.Read(this, Info.ProDesignPatternsOffset + i * 0x8A8);
            _ProDesignPatterns[i].Index = i;
        }
    }

    public void Dispose()
    {
        Data = null;
        Marshal.FreeHGlobal(this.RawData);
    }

    public void Save()
    {
        if (Info == null)
            throw new Exception("Savegame wasn't loaded successfully.");

        for (int i = 0; i < _SimpleDesignPatterns.Length; i++)
            _SimpleDesignPatterns[i].Write(this, Info.SimpleDesignPatternsOffset + i * 0x2A8);
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
            _ProDesignPatterns[i].Write(this, Info.ProDesignPatternsOffset + i * 0x8A8);

        byte[] bytes = new byte[this.Size];
        fixed (byte* bytesPtr = &bytes[0])
            Buffer.MemoryCopy(this.Data, bytesPtr, Size, Size);

        var thisFileSet = Info.HashRegions;
        if (thisFileSet != null)
        {
            foreach (var hashRegion in thisFileSet)
                Murmur3.UpdateMurmur32(bytes, hashRegion.HashOffset, hashRegion.BeginOffset, hashRegion.Size);
        }

        var (fileData, headerData) = SaveEncryption.Encrypt(bytes, (uint) DateTime.Now.Ticks);
        File.WriteAllBytes(HeaderFile.FullName, headerData);
        File.WriteAllBytes(MainFile.FullName, fileData);
    }
}
