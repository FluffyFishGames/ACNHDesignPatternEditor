using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using UnityEngine;
using MyHorizons.Encryption;
using MyHorizons.Hash;
using MyHorizons.Data.Save;

public class Savegame
{
	public static MainSaveFile Decrypt(FileInfo file)
	{
		var headerFile = new FileInfo(Path.Combine(file.Directory.FullName, Path.GetFileNameWithoutExtension(file.Name) + "Header.dat"));
		return new MainSaveFile(headerFile.FullName, file.FullName, new MainSaveFile.Settings() { ParseDesigns = true, ParsePlayers = false, ParseVillagers = false });
	}
}
