using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Cover : ICrop
{
	private int Width = 0;
	private int Height = 0;
	public void SetImage(System.Drawing.Bitmap bitmap)
	{
		int width = bitmap.Width;
		int height = bitmap.Height;
		if (width == height)
		{
			Width = 32;
			Height = 32;
		}
		else
		{
			if (width > height)
			{
				Height = 32;
				Width = (int) ((((float) Height) / ((float) height)) * width);
			}
			else
			{
				Width = 32;
				Height = (int) ((((float) Width) / ((float) width)) * height);
			}
		}
	}

	public int GetWidth()
	{
		return Width;
	}

	public int GetHeight()
	{
		return Height;
	}
}
