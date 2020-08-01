using UnityEngine;
using UnityEngine.Events;
using System;

namespace tinyfd
{
	[System.Serializable]
	public class MessageBoxEvent : UnityEvent<bool>
	{
	}

	public class MessageBox : MonoBehaviour
	{
		public string title;
		public string message;
		public DialogType dialogType = DialogType.ok;
		public IconType iconType = IconType.info;
		public bool okIsDefault = true;

		public MessageBoxEvent onInput;

		public void Show()
		{
			var input = TinyFileDialogs.MessageBox(title, message, dialogType, iconType, okIsDefault);
			if (onInput != null) {
				onInput.Invoke(input);
			}
		}
	}
}
