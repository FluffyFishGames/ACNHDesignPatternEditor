using UnityEngine;
using System.Collections;

public class EventLogger : MonoBehaviour
{
	public void LogMessageBoxEvent(bool ok) {
		Debug.Log("Clicked " + (ok ? "OK/Yes" : "Cancel/No") + " button.");
	}

	public void LogInputBoxEvent(string input) {
		if (input != null) {
			Debug.Log("Entered " + input + ".");
		} else {
			Debug.Log("Canceled the dialog");
		}
	}

	public void LogInputBoxEvent(bool choosed, Color32 color) {
		if (choosed) {
			Debug.Log("Choosed color " + color);
		} else {
			Debug.Log("Canceled the dialog");
		}
	}
}

