using MyHorizons.Data.PlayerData;
using System;
using System.Collections.Generic;
using System.IO;

namespace MyHorizons.Data.Save
{
    public sealed class MainSaveFile : SaveBase, IDesignPatternContainer
    {
        private static SaveBase _saveFile;
        private readonly List<PlayerSave> _playerSaves;
        
        public static SaveBase Singleton() => _saveFile;

        public int NumPlayers => _playerSaves.Count;
        public readonly Villager[] Villagers = new Villager[10];
		private DesignPattern[] _DesignPatterns = new DesignPattern[50];
		private DesignPattern[] _ProDesignPatterns = new DesignPattern[50];
		private PlayerData.PersonalID _PersonalID;
		public DesignPattern[] DesignPatterns { get { return _DesignPatterns; } }
		public DesignPattern[] ProDesignPatterns { get { return _ProDesignPatterns; } }
		public PlayerData.PersonalID PersonalID { get { return _PersonalID; } }

		private Settings _settings;
		private bool IsDesignsFile = false;

		public class Settings
		{
			public static readonly Settings Default = new Settings()
			{
				ParsePlayers = true,
				ParseVillagers = true,
				ParseDesigns = true,
				ParseProDesigns = true
			};

			public bool ParsePlayers;
			public bool ParseVillagers;
			public bool ParseDesigns;
			public bool ParseProDesigns;
		}

		public override int GetRevision()
		{
			if (IsDesignsFile)
				return 50;
			else
				return base.GetRevision();
		}

		public MainSaveFile(in string headerPath, in string filePath, Settings settings = null)
        {
			if (headerPath == null)
			{
				_saveFile = this;
				IsDesignsFile = true;

				if (AcceptsFile(headerPath, filePath) && Load(headerPath, filePath, null))
				{
					_PersonalID = new PersonalID();
					_PersonalID.SetName("Designer");
					_PersonalID.UniqueId = 0xFFFFFFFF;
					_PersonalID.TownId = new TownData.TownID();
					_PersonalID.TownId.SetName("Designer");
					_PersonalID.TownId.UniqueID = 0xFFFFFFFF;

					for (var i = 0; i < 50; i++)
						DesignPatterns[i] = new DesignPattern(i, false);
					for (var i = 0; i < 50; i++)
						ProDesignPatterns[i] = new DesignPattern(i, true);
				}
			}
			else
			{
				this._settings = settings == null ? Settings.Default : settings;
				IsDesignsFile = false;

				// TODO: IProgress<float> needs to be passed to load
				if (AcceptsFile(headerPath, filePath) && Load(headerPath, filePath, null))
				{
					//System.IO.File.WriteAllBytes(filePath + ".decrpyted", _rawData);
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

					_PersonalID = this.ReadStruct<PlayerData.PersonalID>(0x124);

					if (this._settings.ParseVillagers)
					{
						// Load villagers
						for (var i = 0; i < 10; i++)
							Villagers[i] = new Villager(i);
					}

					if (this._settings.ParseDesigns)
					{
						for (var i = 0; i < 50; i++)
							DesignPatterns[i] = new DesignPattern(i, false);
					}

					if (this._settings.ParseProDesigns)
					{
						for (var i = 0; i < 50; i++)
							ProDesignPatterns[i] = new DesignPattern(i, true);
					}
				}
			}
        }

        public override bool AcceptsFile(in string headerPath, in string filePath)
        {
			if (headerPath == null) return true;
            return base.AcceptsFile(headerPath, filePath) && new FileInfo(filePath).Length == RevisionManager.GetSaveFileSizes(_revision)?.Size_main;
        }

        public override bool Save(in string filePath, IProgress<float> progress)
        {
			if (IsDesignsFile)
			{
				foreach (var pattern in DesignPatterns)
					pattern.Save();
				
				foreach (var pattern in ProDesignPatterns)
					pattern.Save();

				return base.Save(filePath, progress);
			}
			else
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

				if (this._settings.ParseProDesigns)
				{
					foreach (var pattern in ProDesignPatterns)
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
        }

        public Player GetPlayer(int index) => _playerSaves[index].Player;

        public List<PlayerSave> GetPlayerSaves() => _playerSaves;
    }
}
