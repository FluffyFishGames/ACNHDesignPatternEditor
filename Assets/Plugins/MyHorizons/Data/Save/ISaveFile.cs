using System;

namespace MyHorizons.Data.Save
{
    public interface ISaveFile
    {
        bool AcceptsFile(in string headerPath, in string filePath);
        bool Load(in string headerPath, in string filePath, IProgress<float> progress);
        bool Load(in byte[] headerData, in byte[] fileData, IProgress<float> progress);
        bool Save(in string filePath, IProgress<float> progress);

        int GetRevision();

        // Read Helpers
        sbyte ReadS8(int offset);
        byte ReadU8(int offset);
        short ReadS16(int offset);
        ushort ReadU16(int offset);
        int ReadS32(int offset);
        uint ReadU32(int offset);
        long ReadS64(int offset);
        ulong ReadU64(int offset);
        float ReadF32(int offset);
        double ReadF64(int offset);
        string ReadString(int offset, int size);
        T[] ReadArray<T>(int offset, int count);
        T ReadStruct<T>(int offset) where T : struct;

        // Write Helpers
        void WriteS8(int offset, sbyte value);
        void WriteU8(int offset, byte value);
        void WriteS16(int offset, short value);
        void WriteU16(int offset, ushort value);
        void WriteS32(int offset, int value);
        void WriteU32(int offset, uint value);
        void WriteS64(int offset, long value);
        void WriteU64(int offset, ulong value);
        void WriteF32(int offset, float value);
        void WriteF64(int offset, double value);
        void WriteString(int offset, string value, int maxSize);
        void WriteArray<T>(int offset, T[] values);
        void WriteStruct<T>(int offset, in T structure) where T : struct;

    }
}