using UnityEngine;
using UnityEngine.Events;
using System;

namespace tinyfd
{
	public enum FileDialogType {
		Save,
		Open,
		Folder
	}

	public class FileDialog : MonoBehaviour
	{
		public FileDialogType fileDialogType = FileDialogType.Open;

		public string title;
		public string defaultPathAndFile;

		public string[] filterPatterns;
		public string filterDescription = null;

		public bool allowMultipleSelects = false;

		public InputBoxEvent onInput;

		public void Show()
		{
			string input = "";
			switch(fileDialogType) {
				case FileDialogType.Open:
					input = TinyFileDialogs.OpenFileDialog(title, defaultPathAndFile, filterPatterns, filterDescription, allowMultipleSelects);
					break;
				case FileDialogType.Folder:
					input = TinyFileDialogs.SelectFolderDialog(title, defaultPathAndFile);
					break;
				case FileDialogType.Save:
					input = TinyFileDialogs.SaveFileDialog(title, defaultPathAndFile, filterPatterns, filterDescription);
					break;
			}
			if (onInput != null) {
				onInput.Invoke(input);
			}
		}
	}
}
