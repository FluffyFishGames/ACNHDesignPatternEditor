using MyHorizons.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Color = UnityEngine.Color;

public class EditPattern
{
	
	private Sprite _PreviewSprite;
	private DesignPattern _Pattern;
	private Bitmap _Image;
	private Texture2D _UpscaledPreviewTexture;
	private Color[] _Pixels;

	public Sprite PreviewSprite
	{
		get
		{
			if (_PreviewSprite == null)
				CreatePreviewSprite();
			return _PreviewSprite;
		}
	}

	public Texture2D UpscaledPreviewTexture
	{
		get
		{
			if (_UpscaledPreviewTexture == null)
				CreateUpscaledPreviewTexture();
			return _UpscaledPreviewTexture;
		}
	}

	public Color[] Pixels
	{
		get
		{
			if (_Pixels == null)
				CreatePixels();
			return _Pixels;
		}
	}

	public Bitmap Image
	{
		get
		{
			if (_Image == null)
				CreateImage();
			return _Image;
		}
	}

	public DesignPattern Pattern
	{
		get
		{
			return _Pattern;
		}
	}

	public void Invalidate()
	{
		GameObject.DestroyImmediate(_UpscaledPreviewTexture);
		GameObject.DestroyImmediate(_PreviewSprite);
	}

	public void Dispose()
	{
		_Image.Dispose();
	}

	private void CreatePreviewSprite()
	{
		_Pattern.GetPreview();
	}

	private void CreateUpscaledPreviewTexture()
	{
		var scaler = new xBRZNet.xBRZScaler();
		var whitedImage = new Bitmap(Image.Width, Image.Height);
		for (var y = 0; y < Image.Height; y++)
		{
			for (var x = 0; x < Image.Width; x++)
			{
				var col = Image.GetPixel(x, y);
				if (col.A == 0)
					whitedImage.SetPixel(x, y, System.Drawing.Color.FromArgb(255, 255, 255));
				else
					whitedImage.SetPixel(x, y, col);
			}
		}
		var scaledImage = scaler.ScaleImage(whitedImage, 4, null);
		whitedImage.Dispose();
		var texture = new Texture2D(scaledImage.Width, scaledImage.Height, TextureFormat.RGB24, false);
		for (int y = 0; y < texture.width; y++)
		{
			for (int x = 0; x < texture.height; x++)
			{
				var col = scaledImage.GetPixel(x, y);
				if (col.A == 0)
					texture.SetPixel(x, texture.height - y, new Color(1f, 1f, 1f, 1f));
				else 
					texture.SetPixel(x, texture.height - y, new Color(((float) col.R) / 255f, ((float) col.G) / 255f, ((float) col.B) / 255f, ((float) col.A) / 255f));
			}
		}
		texture.Apply();
		_UpscaledPreviewTexture = texture;
	}

	private void CreateImage()
	{
		_Image = new Bitmap(32, 32);
		Color[] cols = null;
		if (_Pattern == null && _PreviewSprite != null)
			cols = ((Texture2D) _PreviewSprite.texture).GetPixels();

		for (int y = 0; y < 32; y++)
		{
			for (int x = 0; x < 32; x++)
			{
				if (_Pattern != null)
				{
					int index = _Pattern.GetPixel(x, y);
					System.Drawing.Color col = System.Drawing.Color.FromArgb(0, 0, 0, 0);
					if (index < 15)
						col = System.Drawing.Color.FromArgb(_Pattern.Palette[index].R, _Pattern.Palette[index].G, _Pattern.Palette[index].B);
					_Image.SetPixel(x, y, col);
				}
				else if (cols != null)
				{
					_Image.SetPixel(x, 32 - y, System.Drawing.Color.FromArgb((byte) (cols[x + y * 32].a * 255f), (byte) (cols[x + y * 32].r * 255f), (byte) (cols[x + y * 32].g * 255f), (byte) (cols[x + y * 32].b * 255f)));
				}
			}
		}
	}

	private void CreatePixels()
	{
		_Pixels = this.Pattern.GetPixels();
	}

	public EditPattern(Bitmap bitmap)
	{
		_Image = bitmap;
		_Pattern = new DesignPattern();
		_Pattern.FromBitmap(_Image);
	}

	public EditPattern(DesignPattern pattern)
	{
		_Pattern = pattern;
	}
}