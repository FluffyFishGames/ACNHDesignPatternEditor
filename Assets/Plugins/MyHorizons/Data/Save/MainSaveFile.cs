using MyHorizons.Data.PlayerData;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase
    {
        private static SaveBase _saveFile;
        private readonly List<PlayerSave> _playerSaves;
        
        public static SaveBase Singleton() => _saveFile;

        public int NumPlayers => _playerSaves.Count;
        public readonly Villager[] Villagers = new Villager[10];
        public readonly DesignPattern[] DesignPatterns = new DesignPattern[50];

		private Settings _settings;

		public class Settings
		{
			public static readonly Settings Default = new Settings()
			{
				ParsePlayers = true,
				ParseVillagers = true,
				ParseDesigns = true
			};

			public bool ParsePlayers;
			public bool ParseVillagers;
			public bool ParseDesigns;
		}

        public MainSaveFile(in string headerPath, in string filePath, Settings settings = null)
        {
			this._settings = settings == null ? Settings.Default : settings;

            // TODO: IProgress<float> needs to be passed to load
            if (AcceptsFile(headerPath, filePath) && Load(headerPath, filePath, null))
            {
				System.IO.File.WriteAllBytes(filePath + ".decrpyted", _rawData);
                _saveFile = this;

				// Load player save files
				if (this._settings.ParsePlayers)
				{
					_playerSaves = new List<PlayerSave>();
					foreach (var dir in Directory.GetDirectories(Path.GetDirectoryName(filePath)))
					{
						var playerSave = new PlayerSave(dir, _revision.Value);
						if (playerSave.Valid)
							_playerSaves.Add(playerSave);
					}
				}

				if (this._settings.ParseVillagers)
				{
					// Load villagers
					for (var i = 0; i < 10; i++)
						Villagers[i] = new Villager(i);
				}

				if (this._settings.ParseDesigns)
				{
					for (var i = 0; i < 50; i++)
						DesignPatterns[i] = new DesignPattern(i);
				}
            }
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }

        public override bool Save(in string filePath, IProgress<float> progress)
        {
			// Save Villagers
			if (this._settings.ParseVillagers)
			{
				foreach (var villager in Villagers)
					villager.Save();
			}

			if (this._settings.ParseDesigns)
			{
				foreach (var pattern in DesignPatterns)
					pattern.Save();
			}

            if (base.Save(filePath, progress))
            {
				// Save Players
				if (this._settings.ParsePlayers)
				{
					var dir = Path.GetDirectoryName(filePath);
					foreach (var playerSave in _playerSaves)
					{
						if (!playerSave.Save(Path.Combine(dir, $"Villager{playerSave.Index}")))
							return false;
					}
				}
                return true;
            }
            return false;
        }

        public Player GetPlayer(int index) => _playerSaves[index].Player;

        public List<PlayerSave> GetPlayerSaves() => _playerSaves;
    }
}
