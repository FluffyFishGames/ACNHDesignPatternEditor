using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public unsafe class Designs : BinaryData, IDesignPatternContainer
{
    public SimpleDesignPattern[] SimpleDesignPatterns { get => _SimpleDesignPatterns; set => _SimpleDesignPatterns = value; }
    public ProDesignPattern[] ProDesignPatterns { get => _ProDesignPatterns; set => _ProDesignPatterns = value; }

    public PersonalID PersonalID { get => _PersonalID; }
    private PersonalID _PersonalID = new PersonalID() { Name = "Someone", TownId = new TownID() { Name = "Sometown", UniqueID = 0xFFFFFFFF }, UniqueId = 0xFFFFFFFF };
    private FileInfo MainFile;
    private IntPtr RawData;

    private SimpleDesignPattern[] _SimpleDesignPatterns = new SimpleDesignPattern[50];
    private ProDesignPattern[] _ProDesignPatterns = new ProDesignPattern[50];

    public Designs(FileInfo file)
    {
        MainFile = file;
        if (file.Exists)
            Load();
        else
        {
            RawData = Marshal.AllocHGlobal(0x235A0);
            Size = 0x235A0;
            Data = (byte*) RawData.ToPointer();
        }
    }

    public void Load()
    {
        try 
        { 
            var bytes = File.ReadAllBytes(MainFile.FullName);
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
        _PersonalID = PersonalID.Read(this, 0x120);
        for (int i = 0; i < _SimpleDesignPatterns.Length; i++)
        {
            _SimpleDesignPatterns[i] = SimpleDesignPattern.Read(this, 0x00 + i * 0x2A8);
            _SimpleDesignPatterns[i].Index = i;
        }
        int proDesignOffset = 0x84D0;
        if (Size == 0x38900) // still support broken versions
            proDesignOffset = 0xD480;
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
        {
            _ProDesignPatterns[i] = ProDesignPattern.Read(this, proDesignOffset + i * 0x8A8);
            _ProDesignPatterns[i].Index = i;
        }

        if (Size == 0x38900) // still support broken versions
        {
            Dispose();
            RawData = Marshal.AllocHGlobal(0x235A0);
            Size = 0x235A0;
            Data = (byte*) RawData.ToPointer();
        }
    }

    public void Dispose()
    {
        Data = null;
        Marshal.FreeHGlobal(this.RawData);
    }

    public void Save()
    {
        for (int i = 0; i < _SimpleDesignPatterns.Length; i++)
            _SimpleDesignPatterns[i].Write(this, 0x00 + i * 0x2A8);
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
            _ProDesignPatterns[i].Write(this, 0x84D0 + i * 0x8A8);

        byte[] bytes = new byte[Size];
        fixed (byte* bytesPtr = &bytes[0])
            System.Buffer.MemoryCopy(Data, bytesPtr, Size, Size);

        File.WriteAllBytes(MainFile.FullName, bytes);
    }
}
