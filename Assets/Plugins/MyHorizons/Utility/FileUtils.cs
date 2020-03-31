using MyHorizons.Data;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

namespace MyHorizons.Utility
{
    public static class FileUtils
    {
        private static string _resourcesFolderPath;

        public static string GetResourcesPath()
            => _resourcesFolderPath ?? (_resourcesFolderPath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "Resources"));
    }

    public static class ItemDatabaseLoader
    {
        public static Dictionary<ushort, string> ItemNameDatabase;

        public static string GetNameForItem(in Item item)
        {
            if (ItemNameDatabase.ContainsKey(item.ItemId))
                return ItemNameDatabase[item.ItemId];
            return $"Unknown Item - [0x{item.ItemId}]";
        }

        public static Dictionary<ushort, string> LoadItemDatabase(uint revision)
        {
            var resourcesDir = FileUtils.GetResourcesPath();
            if (Directory.Exists(resourcesDir))
            {
                // TODO: other languages
                var itemDatabasePath = Path.Combine(resourcesDir, $"v{revision}", "Text", "Items", "ItemNames_en.txt");
                if (File.Exists(itemDatabasePath))
                {
                    var dict = new Dictionary<ushort, string>();
                    using (var reader = File.OpenText(itemDatabasePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Length > 6 && ushort.TryParse(line.Substring(0, 4), NumberStyles.HexNumber, null, out var itemId))
                            {
                                dict.Add(itemId, line.Substring(6));
                            }
                        }
                    }
                    ItemNameDatabase = dict;
                    return dict;
                }
            }
            return null;
        }
    }

    public static class VillagerDatabaseLoader
    {
        public const int NUM_ANIMAL_SPECIES = 35;
        public static Dictionary<byte, string>[] LoadVillagerDatabase(uint revision)
        {
            var resourcesDir = FileUtils.GetResourcesPath();
            if (Directory.Exists(resourcesDir))
            {
                // TODO: other languages
                var itemDatabasePath = Path.Combine(resourcesDir, $"v{revision}", "Text", "Villagers", "VillagerNames_en.txt");
                if (File.Exists(itemDatabasePath))
                {
                    var villagersBySpecies = new Dictionary<byte, string>[NUM_ANIMAL_SPECIES + 1];
                    for (var i = 0; i < NUM_ANIMAL_SPECIES + 1; i++)
                        villagersBySpecies[i] = new Dictionary<byte, string>();

                    using (var reader = File.OpenText(itemDatabasePath))
                    {
                        string line;
                        while ((line = reader.ReadLine()) != null)
                        {
                            if (line.Length > 8 && byte.TryParse(line.Substring(0, 2), NumberStyles.HexNumber, null, out var speciesIdx)
                                && speciesIdx <= NUM_ANIMAL_SPECIES && byte.TryParse(line.Substring(4, 2), NumberStyles.HexNumber, null, out var animalIdx))
                            {
                                villagersBySpecies[speciesIdx].Add(animalIdx, line.Substring(8));
                            }
                        }
                    }
                    return villagersBySpecies;
                }
            }
            return null;
        }
    }
}
