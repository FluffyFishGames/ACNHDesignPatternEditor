using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class Fit : ICrop
{
	private int Width = 0;
	private int Height = 0;
	public void SetImage(System.Drawing.Bitmap bitmap, int desiredWidth, int desiredHeight)
	{
		int width = bitmap.Width;
		int height = bitmap.Height;
		float factor1 = ((float) desiredWidth / (float) desiredHeight);
		float factor2 = ((float) width / (float) height);
		if (factor1 > factor2)
		{
			Height = desiredHeight;
			Width = (int) ((((float) Height) / ((float) height)) * width);
		}
		else
		{
			Width = desiredWidth;
			Height = (int) ((((float) Width) / ((float) width)) * height);
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
