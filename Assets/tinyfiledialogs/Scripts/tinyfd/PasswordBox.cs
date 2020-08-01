using UnityEngine;
using UnityEngine.Events;
using System;

namespace tinyfd
{
	public class PasswordBox : MonoBehaviour
	{
		public string title;
		public string message;

		public InputBoxEvent onInput;

		public void Show()
		{
			var input = TinyFileDialogs.PasswordBox(title, message);
			if (onInput != null) {
				onInput.Invoke(input);
			}
		}
	}

}
