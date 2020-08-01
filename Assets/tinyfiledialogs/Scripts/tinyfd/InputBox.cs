using UnityEngine;
using UnityEngine.Events;
using System;

namespace tinyfd
{
	[System.Serializable]
	public class InputBoxEvent : UnityEvent<string>
	{
	}

	public class InputBox : MonoBehaviour
	{
		public string title;
		public string message;
		public string defaultInput = "";

		public InputBoxEvent onInput;

		public void Show()
		{
			var input = TinyFileDialogs.InputBox(title, message, defaultInput);
			if (onInput != null) {
				onInput.Invoke(input);
			}
		}
	}
}
