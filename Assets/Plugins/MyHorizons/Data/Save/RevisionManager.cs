using System;

namespace MyHorizons.Data.Save
{
    public readonly struct SaveRevision
    {
        public readonly uint Major;
        public readonly uint Minor;
        public readonly ushort HeaderFileRevision; // ?
        public readonly ushort Unk1;
        public readonly ushort SaveFileRevision; // ?
        public readonly ushort Unk2;

        public readonly string GameVersion; // Game version string
        public readonly int HashVersion; // What HashInfo values to use when updating hashes.
        public readonly int Revision; // MyHorizons Revision Id

        public SaveRevision(uint maj, uint min, ushort headerR, ushort u1, ushort saveR, ushort u2, string gameVersion, int hashVer, int rev)
        {
            Major = maj;
            Minor = min;
            HeaderFileRevision = headerR;
            Unk1 = u1;
            SaveFileRevision = saveR;
            Unk2 = u2;

            GameVersion = gameVersion;
            HashVersion = hashVer;
            Revision = rev;
        }
    }

    public readonly struct SaveFileSizes
    {
        public readonly uint Size_main;
        public readonly uint Size_personal;
        public readonly uint Size_photo_studio_island;
        public readonly uint Size_postbox;
        public readonly uint Size_profile;

        public SaveFileSizes(uint main, uint personal, uint photo, uint postbox, uint profile)
        {
            Size_main = main;
            Size_personal = personal;
            Size_photo_studio_island = photo;
            Size_postbox = postbox;
            Size_profile = profile;
        }
    }

    public static class RevisionManager
    {
        // Table of known revision data for each game version
        private static readonly SaveRevision[] KnownRevisions =
        {
            new SaveRevision(0x67, 0x6F, 0, 2, 0, 2, "1.0.0", 0, 0), // 1.0.0
            new SaveRevision(0x6D, 0x78, 0, 2, 1, 2, "1.1.0", 1, 1), // 1.1.0
            new SaveRevision(0x6D, 0x78, 0, 2, 2, 2, "1.1.1", 1, 1), // 1.1.1
            new SaveRevision(0x6D, 0x78, 0, 2, 3, 2, "1.1.2", 1, 1)  // 1.1.2
        };

        // Table of save file sizes by revision
        private static readonly SaveFileSizes[] SizesByRevision =
        {
            new SaveFileSizes(0xAC0938, 0x6BC50, 0x263B4, 0xB44580, 0x69508),
            new SaveFileSizes(0xAC2AA0, 0x6BED0, 0x263C0, 0xB44590, 0x69560)
        };

        // Gets the revision info for a given file data.
        public static SaveRevision? GetFileRevision(in byte[] data)
        {
            // Revision data seems to be 0x40 in length. First one is current revision, second one is either "prev revision" or "creation revision"
            if (data.Length < 0x80) return null;
            var maj = BitConverter.ToUInt32(data, 0);
            var min = BitConverter.ToUInt32(data, 4);
            var unk1 = BitConverter.ToUInt16(data, 8);
            var headerRev = BitConverter.ToUInt16(data, 10);
            var unk2 = BitConverter.ToUInt16(data, 12);
            var saveRev = BitConverter.ToUInt16(data, 14);

            foreach (var revision in KnownRevisions)
                if (revision.Major == maj && revision.Minor == min
                 && revision.HeaderFileRevision == headerRev && revision.Unk1 == unk1
                 && revision.SaveFileRevision == saveRev && revision.Unk2 == unk2)
                    return revision;
            return null;
        }

        // Checks whether the file data has a known revision.
        public static bool IsKnownRevision(in byte[] data) => GetFileRevision(data) != null;

        // Gets the version string for for the supplied data.
        public static string GetGameVersionFromRevision(in byte[] data) => GetFileRevision(data)?.GameVersion ?? "Unknown Version";

        // Gets the save file sizes set for the given data.
        public static SaveFileSizes? GetSaveFileSizes(in byte[] data) => GetSaveFileSizes(GetFileRevision(data));

        // Gets the save file sizes for a given revision
        public static SaveFileSizes? GetSaveFileSizes(SaveRevision? revision)
        {
            if (revision?.Revision < SizesByRevision.Length)
                return SizesByRevision[revision.Value.Revision];
            return null;
        }

        // Determines whether the given file data matches any known save file for a revision.
        public static bool IsSaveSizeValid(in byte[] data)
        {
            var sizes = GetSaveFileSizes(data);
            return data.Length == sizes?.Size_main || data.Length == sizes?.Size_personal || data.Length == sizes?.Size_photo_studio_island
                || data.Length == sizes?.Size_postbox || data.Length == sizes?.Size_profile;
        }
    }
}
