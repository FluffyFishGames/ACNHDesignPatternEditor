using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Pixel
{
	public PixelGrid Grid;
	private Color Color;
	private Texture2D Texture;
	private int X;
	private int Y;
	private int PixelSize;

	public Pixel(PixelGrid grid, Texture2D texture, int x, int y, int pixelSize)
	{
		this.Grid = grid;
		this.Texture = texture;
		this.X = x;
		this.Y = y;
		this.PixelSize = pixelSize;
	}

	public void SetColor(Color c)
	{
		this.Color = c;
		/*var colors = new Color[PixelSize * PixelSize];
		for (int i = 0; i < colors.Length; i++)
			colors[i] = this.Color;*/
		this.Texture.SetPixel(X, this.Texture.height - (Y + 1), this.Color);
		//this.Texture.SetPixels(X * PixelSize, this.Texture.height - (Y + 1) * PixelSize, PixelSize, PixelSize, colors);
		this.Grid.PixelsUpdated = true;
	}

	public Color GetColor()
	{
		return this.Color;
	}
}
