using MyHorizons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPalette : MonoBehaviour
{
	public GameObject PaletteColorPrefab;
	private List<ColorPaletteButton> Buttons = new List<ColorPaletteButton>();
	private int Selected = 0;
	private DesignPattern CurrentPattern;

    // Start is called before the first frame update
    void Start()
    {
		var colors = new Color[]
		{
			new Color(0.5f, 0.6f, 0.4f),
			new Color(0.1f, 0.2f, 0.3f),
			new Color(0.6f, 0.3f, 0.6f),
			new Color(0.8f, 0.5f, 0.8f),
			new Color(0.4f, 0.4f, 0.1f),
			new Color(0.2f, 0.8f, 0.2f),
			new Color(0.5f, 0.9f, 0.5f),
			new Color(0.6f, 0.0f, 0.6f),
			new Color(0.9f, 0.8f, 0.2f),
			new Color(0.6f, 0.8f, 0.2f),
			new Color(0.7f, 0.6f, 0.3f),
			new Color(0.8f, 0.5f, 0.4f),
			new Color(0.3f, 0.3f, 0.7f),
			new Color(0.2f, 0.3f, 0.9f),
			new Color(0.1f, 0.6f, 0.7f),
			new Color(0f, 0f, 0f, 0f),
		};
        for (int i = 0; i < 16; i++)
		{
			var button = GameObject.Instantiate(PaletteColorPrefab, this.transform);
			var colorPaletteButton = button.GetComponent<ColorPaletteButton>();
			colorPaletteButton.Palette = this;
			colorPaletteButton.Index = i;
			colorPaletteButton.SetColor(colors[i]);
			if (Selected == i)
				colorPaletteButton.Select();
			Buttons.Add(colorPaletteButton);
		}
    }

	public void SetPattern(DesignPattern pattern)
	{
		this.CurrentPattern = pattern;
		for (int i = 0; i < 15; i++)
		{
			Buttons[i].SetColor(new Color(((float) pattern.Palette[i].R) / 255f, ((float) pattern.Palette[i].G) / 255f, ((float) pattern.Palette[i].B) / 255f));
		}
	}

	public void ChangeColor(int index)
	{
		if (Selected != index)
		{
			Buttons[Selected].Deselect();
			Buttons[index].Select();
			Selected = index;
		}
	}

    // Update is called once per frame
    void Update()
    {
        
    }
}
