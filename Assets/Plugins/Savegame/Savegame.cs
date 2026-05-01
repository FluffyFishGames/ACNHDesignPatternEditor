using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    private SimpleDesignPattern[] _SimpleDesignPatterns;
    private ProDesignPattern[] _ProDesignPatterns;
    private PersonalID _PersonalID;
    private bool SaveFlags = true;

    public Savegame(FileInfo file)
	{
        MainFile = new FileInfo(Path.Combine(file.Directory.FullName, "main.dat"));
        HeaderFile = new FileInfo(Path.Combine(file.Directory.FullName, "mainHeader.dat"));
        if (!MainFile.Exists || !HeaderFile.Exists)
            throw new Exception("Savegame is incomplete.");

        Load();
    }

    private int FindFlagOffset(byte[] bytes, int simplePatterns = 50, int proPatterns = 50, int firstHashRegion = 0x110)
    {
        var islandID = BitConverter.ToUInt32(bytes, firstHashRegion + 0x14);
        UnityEngine.Debug.Log("Found island ID " + islandID.ToString("X4"));
        var currentOffset = 0;
        bool found = false;
        for (var i = bytes.Length - 4; i >= 0; i--)
        {
            var otherIslandID = BitConverter.ToUInt32(bytes, i);
            if (islandID == otherIslandID)
            {
                currentOffset = i;
                found = true;
                break;
            }
        }
        if (!found)
            return -1;
        UnityEngine.Debug.Log("Last occurence at " + currentOffset.ToString("X8"));
        found = false;
        for (var i = currentOffset; i < bytes.Length; i++)
        {
            if (bytes[i] == 0x01 && bytes[i + 1] == 0x00 && bytes[i + 2] == 0x34 && bytes[i + 3] == 0x00 && bytes[i + 4] == 0x00 && bytes[i + 5] == 0x00)
            {
                currentOffset = i;
                found = true;
                break;
            }
        }
        if (!found)
            return -1;
        UnityEngine.Debug.Log("01 00 34 00 00 00 found at " + currentOffset.ToString("X8"));
        found = false;
        for (var i = currentOffset; i >= 0; i--)
        {
            if (bytes[i] == 0x07 && bytes[i + 3] == 0x05)
            {
                currentOffset = i;
                found = true;
                break;
            }
        }
        if (!found)
            return -1;
        UnityEngine.Debug.Log("07 ?? ?? 05 found at " + currentOffset.ToString("X8"));
        currentOffset -= 5;
        currentOffset -= simplePatterns;
        currentOffset -= proPatterns;
        return currentOffset;
    }

    public void Load()
    {
        byte[] headerBytes = File.ReadAllBytes(HeaderFile.FullName);
        byte[] mainBytes = File.ReadAllBytes(MainFile.FullName);
        Info = SavegameInfo.GetInfo(headerBytes);
        var bytes = SaveEncryption.Decrypt(headerBytes, mainBytes);
        System.IO.File.WriteAllBytes(HeaderFile.FullName + ".decrypted", bytes);
        Info = null;
        if (Info == null)
        {
            UnityEngine.Debug.LogWarning("Save file is not supported out of the box. We need to find the hash regions.");
            var latest = SavegameInfo.GetLatest();
            // check if latest is just working first

            bool needToFindRegions = false;
            foreach (var region in latest.HashRegions)
            {
                var originalHash = BitConverter.ToUInt64(bytes, region.HashOffset);
                var calculatedHash = Murmur3.GetMurmur3Hash(bytes, region.BeginOffset, region.Size);
                if (originalHash != calculatedHash)
                {
                    needToFindRegions = true;
                    break;
                }
            }

            if (needToFindRegions)
            {
                SavegameInfo.HashRegion[] newHashRegions = new SavegameInfo.HashRegion[latest.HashRegions.Length];
                int offset = 0;
                const int maxSearchSize = 0x100000;
                for (var i = 0; i < latest.HashRegions.Length; i++)
                {
                    var region = latest.HashRegions[i];
                    if (i == 0)
                        offset = region.HashOffset;

                    var size = Murmur3.FindBlock(bytes, offset, 128, region.Size + maxSearchSize, 0);
                    if (size == 0)
                        throw new Exception("Couldn't find hash regions for save file automatically.");

                    UnityEngine.Debug.Log("Found hash region: " + offset.ToString("X8") + " with size " + size.ToString("X8"));
                    newHashRegions[i] = new SavegameInfo.HashRegion(offset, size);
                    offset += (int) size + 4;

                    // find non null bytes, non 0x0000FFFF bytes and non 0xFEFF0000 bytes (there are possibilities for hash collisions)
                    while (offset < bytes.Length - 4 &&
                        (
                            (bytes[offset] == 0x00 && bytes[offset + 1] == 0x00 && bytes[offset + 2] == 0x00 && bytes[offset + 3] == 0x00) ||
                            (bytes[offset] == 0x00 && bytes[offset + 1] == 0x00 && bytes[offset + 2] == 0xFF && bytes[offset + 3] == 0xFF) ||
                            (bytes[offset] == 0xFE && bytes[offset + 1] == 0xFF && bytes[offset + 2] == 0x00 && bytes[offset + 3] == 0x00)
                        ))
                        offset += 4;
                }

                // search for offsets
                var simpleFound = false;
                var simpleOffset = newHashRegions[1].HashOffset + 0x4A0; // educated guess
                UnityEngine.Debug.Log("Start search at " + simpleOffset.ToString("X8"));
                for (var i = 0; i < 128; i += 4)
                {
                    var checkOffset = simpleOffset + i + 0x2a8 - 0x03; // look at end of pattern for bytes
                    UnityEngine.Debug.Log("Check offset " + checkOffset.ToString("X8"));
                    if (bytes[checkOffset + 0] == 0x00 &&
                        bytes[checkOffset + 1] == 0x00 &&
                        bytes[checkOffset + 2] == 0x00)
                    {
                        bool found = true;
                        for (var j = 1; j < 50; j++)
                        {
                            var otherCheck = checkOffset + 0x2a8 * j;
                            UnityEngine.Debug.Log("Other check " + otherCheck.ToString("X8"));
                            if (bytes[otherCheck + 0] != 0x00 ||
                                bytes[otherCheck + 1] != 0x00 ||
                                bytes[otherCheck + 2] != 0x00)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            simpleOffset += i;
                            simpleFound = true;
                            UnityEngine.Debug.Log("Found simple design offset at 0x" + simpleOffset.ToString("X8"));
                            break;
                        }
                    }
                }
                if (!simpleFound)
                {
                    throw new Exception("Wasn't able to automatically find simple design offset.");
                }
                // check how many patterns there are
                int patterns = 50;
                var searchOffset = simpleOffset + 0x2a8 * 51;
                var mainIslandID = BitConverter.ToUInt32(bytes, simpleOffset + 0x38); // first id appears at 0x38 after our offset

                while (patterns < 200)
                {
                    var byteOffset = searchOffset + 0x2a8 - 0x03;
                    if (bytes[byteOffset + 0] != 0x00 ||
                        bytes[byteOffset + 1] != 0x00 ||
                        bytes[byteOffset + 2] != 0x00)
                        break;
                    searchOffset += 0x2a8;
                    patterns++;
                }
                patterns = patterns - patterns % 50;

                UnityEngine.Debug.Log("The amount of patterns is " + patterns);

                var proFound = false;
                var proOffset = simpleOffset + 0x2a8 * patterns; // start searching after 50 patterns and then increase by simple pattern length
                UnityEngine.Debug.Log("Start search at " + proOffset.ToString("X8"));
                for (var i = 0; i < 0xF0000; i += 4)
                {
                    var checkOffset = proOffset + i + 0x8a8 - 0x03;
                    UnityEngine.Debug.Log("Check offset " + checkOffset.ToString("X8"));
                    if (bytes[checkOffset + 1] <= 0x1D &&
                        bytes[checkOffset + 1] == 0x00 &&
                        bytes[checkOffset + 2] == 0x00)
                    {
                        bool found = true;
                        for (var j = 1; j < 50; j++)
                        {
                            var otherCheck = checkOffset + 0x8a8 * j;
                            UnityEngine.Debug.Log("Other check " + otherCheck.ToString("X8"));
                            // check for island id being the same across at least 50 patterns
                            if (bytes[otherCheck + 1] > 0x1D ||
                                bytes[otherCheck + 1] != 0x00 ||
                                bytes[otherCheck + 2] != 0x00)
                            {
                                found = false;
                                break;
                            }
                        }
                        if (found)
                        {
                            proOffset += i;
                            proFound = true;
                            UnityEngine.Debug.Log("Found pro design offset at 0x" + proOffset.ToString("X8"));
                            break;
                        }
                    }
                }
                if (!proFound)
                {
                    throw new Exception("Wasn't able to automatically find pro design offset.");
                }

                var flagOffset = FindFlagOffset(bytes, patterns, patterns, newHashRegions[0].HashOffset);

                Info = new SavegameInfo.Info(bytes.Length, simpleOffset, proOffset, flagOffset, flagOffset + 0x64, patterns, patterns, newHashRegions);
            }
            else 
            {
                Info = latest;
            }
        }
        try
        {
            foreach (var region in Info.HashRegions)
            {
                var originalHash = BitConverter.ToUInt64(bytes, region.HashOffset);
                var calculatedHash = Murmur3.GetMurmur3Hash(bytes, region.BeginOffset, region.Size);
                if (originalHash != calculatedHash)
                    throw new Exception("Checksum mismatch. Savegame is corrupted.");
            }

            // sanity check for set flags
            SaveFlags = true;
            if (bytes[Info.SimpleDesignSetFlagOffset + Info.SimpleDesignCount + Info.ProDesignCount + 5] != 0x07 ||
                bytes[Info.SimpleDesignSetFlagOffset + Info.SimpleDesignCount + Info.ProDesignCount + 8] != 0x05)
            {
                UnityEngine.Debug.LogWarning("Flag offset is wrong. Trying to find flags...");
                var flagOffset = FindFlagOffset(bytes, Info.SimpleDesignCount, Info.ProDesignCount, Info.HashRegions[0].HashOffset);
                if (flagOffset == -1)
                {
                    SaveFlags = false;
                    UnityEngine.Debug.LogWarning("Can't save flags for patterns.");
                }
                else
                {
                    Info = new SavegameInfo.Info(Info.Size, Info.SimpleDesignPatternsOffset, Info.ProDesignPatternsOffset, flagOffset, flagOffset + 0x64, Info.SimpleDesignCount, Info.ProDesignCount, Info.HashRegions);
                }
            }
            _SimpleDesignPatterns = new SimpleDesignPattern[Info.SimpleDesignCount];
            _ProDesignPatterns = new ProDesignPattern[Info.ProDesignCount];

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
            if (SaveFlags)
                _SimpleDesignPatterns[i].IsSet = this.ReadU8(Info.SimpleDesignSetFlagOffset + i) != 0xFF;
        }
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
        {
            _ProDesignPatterns[i] = ProDesignPattern.Read(this, Info.ProDesignPatternsOffset + i * 0x8A8);
            _ProDesignPatterns[i].Index = i;
            if (SaveFlags)
                _ProDesignPatterns[i].IsSet = this.ReadU8(Info.ProDesignSetFlagOffset + i) != 0xFF;
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
        {
            _SimpleDesignPatterns[i].Write(this, Info.SimpleDesignPatternsOffset + i * 0x2A8);
            if (SaveFlags)
                WriteU8(Info.SimpleDesignSetFlagOffset + i, (byte) (_SimpleDesignPatterns[i].IsSet ? 0x00 : 0xFF));
        }
        for (int i = 0; i < _ProDesignPatterns.Length; i++)
        {
            _ProDesignPatterns[i].Write(this, Info.ProDesignPatternsOffset + i * 0x8A8);
            if (SaveFlags)
                WriteU8(Info.ProDesignSetFlagOffset + i, (byte) (_ProDesignPatterns[i].IsSet ? 0x00 : 0xFF));
        }

        byte[] bytes = new byte[this.Size];
        fixed (byte* bytesPtr = &bytes[0])
            Buffer.MemoryCopy(this.Data, bytesPtr, Size, Size);

        var thisFileSet = Info.HashRegions;
        if (thisFileSet != null)
        {
            foreach (var hashRegion in thisFileSet)
                Murmur3.UpdateMurmur32(bytes, hashRegion.HashOffset, hashRegion.BeginOffset, hashRegion.Size);
        }

        //System.IO.File.WriteAllBytes(HeaderFile.DirectoryName + "/beforeEncryption", bytes);
        var (fileData, headerData) = SaveEncryption.Encrypt(bytes, (uint) DateTime.Now.Ticks);
        File.WriteAllBytes(HeaderFile.FullName, headerData);
        File.WriteAllBytes(MainFile.FullName, fileData);
    }
}
