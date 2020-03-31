using MyHorizons.Data;
using SFB;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ExportOperation : IOperation, IPatternOperation
{
	private DesignPattern Pattern;
	private bool _IsFinished = false;

	public ExportOperation(DesignPattern pattern)
	{
		this.Pattern = pattern;
	}

	public void Abort()
	{
		_IsFinished = true;
	}

	public DesignPattern GetPattern()
	{
		return this.Pattern;
	}

	public bool IsFinished()
	{
		return _IsFinished;
	}

	public void Start()
	{
		var colors = Pattern.GetPixels();
		var bitmap = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		for (var y = 0; y < 32; y++)
		{
			for (var x = 0; x < 32; x++)
			{
				bitmap.SetPixel(x, y, System.Drawing.Color.FromArgb((byte) (colors[x + y * 32].a * 255f), (byte) (colors[x + y * 32].r * 255f), (byte) (colors[x + y * 32].g * 255f), (byte) (colors[x + y * 32].b * 255f)));
			}
		}
		StandaloneFileBrowser.SaveFilePanelAsync("Export image", "", "image.png", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif" }) }, (path) =>
		{
			if (path != null && path.Length > 0)
			{
				var format = System.Drawing.Imaging.ImageFormat.Png;
				if (path.EndsWith(".gif"))
					format = System.Drawing.Imaging.ImageFormat.Gif;
				else if (path.EndsWith(".jpg") || path.EndsWith(".jpeg"))
					format = System.Drawing.Imaging.ImageFormat.Jpeg;
				else if (path.EndsWith(".bmp"))
					format = System.Drawing.Imaging.ImageFormat.Bmp;
				bitmap.Save(path, format);
				_IsFinished = true;
			}
		});
	}
}