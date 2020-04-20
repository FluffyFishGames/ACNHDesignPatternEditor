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
		var headerFile = new FileInfo(Path.Combine(file.Directory.FullName, "mainHeader.dat"));
		var mainFile = new FileInfo(Path.Combine(file.Directory.FullName, "main.dat"));
		if (!mainFile.Exists)
			throw new Exception("main.dat is missing! Please export your whole savegame.");
		if (!headerFile.Exists)
			throw new Exception("mainHeader.dat is missing! Please export your whole savegame.");
		return new MainSaveFile(headerFile.FullName, mainFile.FullName, new MainSaveFile.Settings() { ParseDesigns = true, ParseProDesigns = true, ParsePlayers = false, ParseVillagers = false });
	}
}
