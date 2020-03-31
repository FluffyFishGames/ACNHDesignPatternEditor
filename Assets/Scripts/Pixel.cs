using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pixel : MonoBehaviour
{
	private Image Image;

	void OnEnable()
	{
		Image = GetComponent<Image>();
	}

	public void SetColor(Color c)
	{
		Image.color = c;
	}
}
