using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;

public class BicubicSampling : ISampling
{
	public System.Drawing.Bitmap Resample(System.Drawing.Bitmap image, int width, int height)
	{
		//a holder for the result
		Bitmap result = new Bitmap(width, height);
		//set the resolutions the same to avoid cropping due to resolution differences
		result.SetResolution(image.HorizontalResolution, image.VerticalResolution);

		//use a graphics object to draw the resized image into the bitmap
		using (Graphics graphics = Graphics.FromImage(result))
		{
			//set the resize quality modes to high quality
			graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBilinear;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			//draw the image into the target bitmap
			graphics.DrawImage(image, 0, 0, result.Width, result.Height);
		}

		//return the resulting bitmap
		return result;
	}
}