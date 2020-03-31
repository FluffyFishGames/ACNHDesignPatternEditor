using MyHorizons.Data.PlayerData;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace MyHorizons.Data.Save
{
    public sealed class PlayerSave
    {
        public readonly int Index;
        public readonly bool Valid;

        public readonly Player Player;

        private PersonalSaveFile _personalSave;
        public readonly byte[] _photo_studio_islandData;
        public readonly byte[] _postboxData;
        public readonly byte[] _profileData;

        private readonly SaveRevision _revision;

        public PlayerSave(in string folder, in SaveRevision revision)
        {
            if (Directory.Exists(folder))
            {
                var folderName = new DirectoryInfo(folder).Name;
                if (folderName.StartsWith("Villager") && int.TryParse(folderName.Substring(8, 1), out var idx) && idx > -1 && idx < 8)
                {
                    Index = idx;
                    _revision = revision;
                    ProcessFolder(folder);
                    Player = new Player(idx, _personalSave);
                    // TODO: Valid should only be set to true when all player save files are found and loaded correctly.
                    Valid = true;
                }
            }
        }

        public bool Save(in string folderPath)
        {
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);
            Player.Save();
            return _personalSave.Save(null);
        }

        private void ProcessFolder(in string folder)
        {
            var saveSizesNullable = RevisionManager.GetSaveFileSizes(_revision);
            if (saveSizesNullable != null)
            {
                var saveSizes = saveSizesNullable.Value;
                foreach (var file in Directory.GetFiles(folder, "*.dat"))
                {
                    if (!file.EndsWith("Header.dat"))
                    {
                        var headerFile = Path.Combine(folder, $"{Path.GetFileNameWithoutExtension(file)}Header.dat");
                        var fileSize = new FileInfo(file).Length;
                        if (fileSize == saveSizes.Size_personal && File.Exists(headerFile))
                        {
                            if (_personalSave == null)
                                _personalSave = new PersonalSaveFile(headerFile, file);
                        }
                        // TODO: other files
                    }
                }
            }
        }
    }
}
