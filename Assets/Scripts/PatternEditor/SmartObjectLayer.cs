using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class SmartObjectLayer : Layer, ITransformable
{
	public TextureBitmap Bitmap;
	public ResamplingFilters Resampler;
	public TextureBitmap.CropMode Crop;
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
	private int _PreviousWidth;
	private int _PreviousHeight;
	private ResamplingFilters _PreviousResampler;
	private TextureBitmap.CropMode _PreviousCrop;

	public SmartObjectLayer(SubPattern pattern, string name, TextureBitmap bitmap, int x, int y, int width, int height) : base(pattern, name)
	{
		Crop = TextureBitmap.CropMode.Scale;
		Resampler = ResamplingFilters.Box;
		Bitmap = bitmap;
		_ObjectX = x;
		_ObjectY = y;
		_ObjectWidth = width;
		_ObjectHeight = height;
	}
	
	public void UpdateColors()
	{
		if (_PreviousWidth != _ObjectWidth || _PreviousHeight != _ObjectHeight || _PreviousCrop != Crop || _PreviousResampler != Resampler)
		{
			Texture.CopyFrom(Bitmap);
			Texture.ResampleAndCrop(this.Resampler, this.Crop, _ObjectWidth, _ObjectHeight);
			UpdateTexture();
			_PreviousWidth = _ObjectWidth;
			_PreviousHeight = _ObjectHeight;
			_PreviousCrop = Crop;
			_PreviousResampler = Resampler;
		}
	}

	public void ChangeResampling(ResamplingFilters filter)
	{
		Resampler = filter;
	}

	public ResamplingFilters GetResampling()
	{
		return Resampler;
	}

	public void ChangeCrop(TextureBitmap.CropMode crop)
	{
		Crop = crop;
	}

	public TextureBitmap.CropMode GetCrop()
	{
		return Crop;
	}

	public override void Dispose()
	{
		base.Dispose();
		this.Bitmap.Dispose();
	}
}