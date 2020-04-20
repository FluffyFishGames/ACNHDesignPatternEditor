using MyHorizons.Data;
using SFB;
using System;
using System.Collections.Generic;
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
		var bitmap = new TextureBitmap(Pattern.Width, Pattern.Height);
		for (var y = 0; y < Pattern.Width; y++)
		{
			for (var x = 0; x < Pattern.Height; x++)
			{
				bitmap.SetPixel(x, y, new TextureBitmap.Color((byte) (colors[x + y * Pattern.Width].a * 255f), (byte) (colors[x + y * Pattern.Width].r * 255f), (byte) (colors[x + y * Pattern.Width].g * 255f), (byte) (colors[x + y * Pattern.Width].b * 255f)));
			}
		}
		StandaloneFileBrowser.SaveFilePanelAsync("Export image", "", "image.png", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif" }) }, (path) =>
		{
			if (path != null && path.Length > 0)
			{
				bitmap.Save(path);
				_IsFinished = true;
			}
		});
	}
}