using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class GridCreator
{

	public static Texture2D CreateGrid(Color color, int width, int height, int pixelSize)
	{
		var over = 10;
		Texture2D tex = new Texture2D(width * pixelSize + over * 2, height * pixelSize + over * 2, TextureFormat.ARGB32, false);
		Color[] colors = new Color[tex.width * tex.height];
		for (int gridX = 1; gridX < width; gridX++)
		{
			bool isCenter = (width / 2) == gridX;
			int startY = isCenter ? 0 : over;
			int endY = over + height * pixelSize + (isCenter ? over : 0);

			int startX = over + gridX * pixelSize - (isCenter ? 1 : 0);
			int endX = over + gridX * pixelSize + 1 + (isCenter ? 1 : 0);
			for (int x = startX; x < endX; x++)
				for (int y = startY; y < endY; y++)
					colors[x + y * tex.width] = color;
		}

		for (int gridY = 1; gridY < height; gridY++)
		{
			bool isCenter = (height / 2) == gridY;
			int startX = isCenter ? 0 : over;
			int endX = over + width * pixelSize + (isCenter ? over : 0);

			int startY = over + gridY * pixelSize - (isCenter ? 1 : 0);
			int endY = over + gridY * pixelSize + 1 + (isCenter ? 1 : 0);
			for (int x = startX; x < endX; x++)
				for (int y = startY; y < endY; y++)
					colors[x + y * tex.width] = color;
		}

		tex.SetPixels(colors);
		tex.Apply();
		return tex;
	}
}
