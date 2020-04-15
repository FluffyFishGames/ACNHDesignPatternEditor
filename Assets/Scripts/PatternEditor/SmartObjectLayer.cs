using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SmartObjectLayer : Layer, ITransformable
{
	public System.Drawing.Bitmap Bitmap;
	public ISampling Resampler;
	public ICrop Crop;
	public int ObjectX
	{
		get { return _ObjectX; }
		set { _ObjectX = value; }
	}
	public int ObjectY
	{
		get { return _ObjectY; }
		set { _ObjectY = value; }
	}
	public int ObjectWidth
	{
		get { return _ObjectWidth; }
		set { _ObjectWidth = value; }
	}
	public int ObjectHeight
	{
		get { return _ObjectHeight; }
		set { _ObjectHeight = value; }
	}

	private int _ObjectX;
	private int _ObjectY;
	private int _ObjectWidth;
	private int _ObjectHeight;

	public static List<ICrop> Crops = new List<ICrop>()
	{
		null,
		new Fit(),
		new Cover()
	};

	public static List<ISampling> Resamplers = new List<ISampling>()
	{
		new NearestNeighbourSampling(),
		new BillinearSampling(),
		new BicubicSampling()
	};

	public SmartObjectLayer(SubPattern pattern, string name, System.Drawing.Bitmap bitmap, int x, int y, int width, int height) : base(pattern, name)
	{
		Crop = Crops[0];
		Resampler = Resamplers[0];
		Bitmap = bitmap;
		_ObjectX = x;
		_ObjectY = y;
		_ObjectWidth = width;
		_ObjectHeight = height;
	}
	
	public void UpdateColors()
	{
		int desiredWidth = _ObjectWidth;
		int desiredHeight = _ObjectHeight;

		if (Crop != null)
		{
			Crop.SetImage(Bitmap, desiredWidth, desiredHeight);
			desiredWidth = Crop.GetWidth();
			desiredHeight = Crop.GetHeight();
		}
		var resampled = Resampler.Resample(Bitmap, desiredWidth, desiredHeight);

		var colors = resampled.ToColors();
		for (int i = 0; i < Colors.Length; i++)
			Colors[i] = new UnityEngine.Color(0f, 0f, 0f, 0f);

		var startX = (int) (_ObjectX + (_ObjectWidth - desiredWidth) / 2f);
		var startY = (int) (_ObjectY + (_ObjectHeight - desiredHeight) / 2f);

		for (int y = _ObjectY; y < _ObjectY + _ObjectHeight; y++)
		{
			for (int x = _ObjectX; x < _ObjectX + _ObjectWidth; x++)
			{
				if (x >= startX && y >= startY && x < startX + desiredWidth && y < startY + desiredHeight)
				{
					int px = x;
					int py = y;
					if (px >= 0 && px < _Width && py >= 0 && py < _Height)
						Colors[px + py * _Width] = colors[(x - startX) + (y - startY) * desiredWidth];
				}
			}
		}

		UpdateTexture();
	}

	public void ChangeResampling(int num)
	{
		Resampler = Resamplers[num];
	}

	public int GetResampling()
	{
		return Resamplers.IndexOf(Resampler);
	}

	public void ChangeCrop(int num)
	{
		Crop = Crops[num];
	}

	public int GetCrop()
	{
		return Crops.IndexOf(Crop);
	}

	public override void Dispose()
	{
		base.Dispose();
		this.Bitmap.Dispose();
	}
}