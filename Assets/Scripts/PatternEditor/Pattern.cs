using MyHorizons.Data;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.EuclideanDistance;
using SimplePaletteQuantizer.ColorCaches.LocalitySensitiveHash;
using SimplePaletteQuantizer.ColorCaches.Octree;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;
using SimplePaletteQuantizer.Quantizers.DistinctSelection;
using SimplePaletteQuantizer.Quantizers.MedianCut;
using SimplePaletteQuantizer.Quantizers.Octree;
using SimplePaletteQuantizer.Quantizers.Popularity;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public class Pattern
{
	public PatternEditor Editor;
	public List<SubPattern> SubPatterns;
	public UnityEngine.Color[] Colors;
	public IColorQuantizer Quantizer;
	public IColorCache ColorCache;

	private DesignPattern DesignPattern;
	private DesignPatternInformation.DesignPatternInfo Info;
	private bool IsParsing = false;
	private bool IsReady = false;
	private Bitmap Result;
	private UnityEngine.Texture2D UpscaledPreviewTexture;
	private UnityEngine.Texture2D PreviewTexture;
	private UnityEngine.Sprite PreviewSprite;
	private int _CurrentSubPattern;

	private static List<IColorQuantizer> Quantizers = new List<IColorQuantizer>() {
		new WuColorQuantizer(),
		new DistinctSelectionQuantizer(),
		new PopularityQuantizer(),
		new MedianCutQuantizer(),
		new OctreeQuantizer()
	};

	private static List<IColorCache> ColorCaches = new List<IColorCache>()
	{
		new EuclideanDistanceColorCache(),
		new LshColorCache(),
		new OctreeColorCache()
	};

	public SubPattern CurrentSubPattern
	{
		get
		{
			return SubPatterns[_CurrentSubPattern];
		}
	}

	public int Width
	{
		get
		{
			return DesignPattern.Width;
		}
	}

	public int Height
	{
		get
		{
			return DesignPattern.Height;
		}
	}

	public Pattern(PatternEditor editor, DesignPattern pattern)
	{
		_Type = pattern.Type;
		Result = new Bitmap(1, 1);
		Quantizer = Quantizers[0];
		ColorCache = ColorCaches[0];

		Editor = editor;
		DesignPattern = pattern;
		Colors = pattern.GetPixels();

		Info = DesignPatternInformation.Types[pattern.Type];

		UpscaledPreviewTexture = new UnityEngine.Texture2D(Width * 4, Height * 4, UnityEngine.TextureFormat.RGB24, false);
		PreviewTexture = new UnityEngine.Texture2D(Width, Height, UnityEngine.TextureFormat.ARGB32, false);
	}

	public void Load()
	{
		SubPatterns = new List<SubPattern>();
		foreach (var part in Info.Parts)
			SubPatterns.Add(new SubPattern(this, part));

		Editor.SetSize(CurrentSubPattern.Width, CurrentSubPattern.Height);
		CurrentSubPattern.SelectLayer(0);
		CurrentSubPattern.UpdateImage();
		this.RegeneratePreview();
		Editor.LayersChanged();
		Editor.SubPatternChanged(CurrentSubPattern.Part);
		Editor.Tools.HistoryChanged(CurrentSubPattern.History);
	}

	public void ChangeQuantizer(int num)
	{
		Quantizer = Quantizers[num];
		RegeneratePreview();
	}

	public void ChangeColorCache(int num)
	{
		ColorCache = ColorCaches[num];
		RegeneratePreview();
	}

	public void NextSubPattern()
	{
		_CurrentSubPattern++;
		if (_CurrentSubPattern >= SubPatterns.Count)
			_CurrentSubPattern = 0;
		Editor.SetSize(CurrentSubPattern.Width, CurrentSubPattern.Height);
		CurrentSubPattern.UpdateImage();
		CurrentSubPattern.Selected();
		Editor.SubPatternChanged(CurrentSubPattern.Part);
		Editor.Tools.HistoryChanged(CurrentSubPattern.History);
	}

	public DesignPattern.TypeEnum Type
	{
		get
		{
			return _Type;
		}
	}

	private xBRZNet.xBRZScaler Scaler = new xBRZNet.xBRZScaler();
	private DesignPattern.TypeEnum _Type;

	public void SetType(DesignPattern.TypeEnum type)
	{
		_Type = type;
		Editor.SetType(type);
		Info = DesignPatternInformation.Types[Type];
		this.Load();
	}

	public void Clear()
	{
		var p = new DesignPattern();
		p.Type = this.Type;
		p.IsPro = this.Type != DesignPattern.TypeEnum.SimplePattern;
		p.Pixels = new byte[p.Width / 2 * p.Height];
		p.Empty();
		Colors = p.GetPixels();
		Load();
	}

	public byte[] ToBytes()
	{
		using (MemoryStream stream = new MemoryStream())
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((byte) 0x00); // version
			writer.Write((byte) this.Type); // type;
			writer.Write((byte) Quantizers.IndexOf(Quantizer));
			writer.Write((byte) ColorCaches.IndexOf(ColorCache));
			for (int i = 0; i < SubPatterns.Count; i++)
			{
				writer.Write((byte) SubPatterns[i].Layers.Count); // count of layers
				for (int j = 0; j < SubPatterns[i].Layers.Count; j++)
				{
					var layer = SubPatterns[i].Layers[j];
					if (layer is SmartObjectLayer sol)
					{
						writer.Write((byte)0x01); // smart layer
						writer.Write(sol.ObjectX);
						writer.Write(sol.ObjectY);
						writer.Write(sol.ObjectWidth);
						writer.Write(sol.ObjectHeight);
						writer.Write((byte) SmartObjectLayer.Crops.IndexOf(sol.Crop));
						writer.Write((byte) SmartObjectLayer.Resamplers.IndexOf(sol.Resampler));
						var nameBytes = System.Text.Encoding.UTF8.GetBytes(sol.Name);
						writer.Write(nameBytes.Length);
						writer.Write(nameBytes);
						writer.Write(sol.Bitmap.Width);
						writer.Write(sol.Bitmap.Height);
						writer.Write(sol.Bitmap.GetBytes());
					}
					else
					{
						writer.Write((byte)0x00); // raster layer
						var nameBytes = System.Text.Encoding.UTF8.GetBytes(layer.Name);
						writer.Write(nameBytes.Length);
						writer.Write(nameBytes);
						for (int y = 0; y < layer.Height; y++)
						{
							for (int x = 0; x < layer.Width; x++)
							{
								writer.Write(((byte) (layer.Colors[x + y * layer.Width].r * 255f)));
								writer.Write(((byte) (layer.Colors[x + y * layer.Width].g * 255f)));
								writer.Write(((byte) (layer.Colors[x + y * layer.Width].b * 255f)));
								writer.Write(((byte) (layer.Colors[x + y * layer.Width].a * 255f)));
							}
						}
					}
				}
			}
			return stream.ToArray();
		}
	}

	public void FromBytes(byte[] bytes)
	{
		using (MemoryStream stream = new MemoryStream(bytes))
		{
			BinaryReader reader = new BinaryReader(stream);
			byte version = reader.ReadByte();
			if (version == 0x00)
			{
				var t = (DesignPattern.TypeEnum) reader.ReadByte();
				if (this._Type == DesignPattern.TypeEnum.SimplePattern && t != DesignPattern.TypeEnum.SimplePattern)
					throw new ArgumentException("Simple design spot can't hold pro design.", "project");
				if (this._Type != DesignPattern.TypeEnum.SimplePattern && t == DesignPattern.TypeEnum.SimplePattern)
					throw new ArgumentException("Pro design spot can't hold simple design.", "project");
				this._Type = t;
				Quantizer = Quantizers[reader.ReadByte()];
				ColorCache = ColorCaches[reader.ReadByte()];

				var info = DesignPatternInformation.Types[this._Type];
				this.SubPatterns = new List<SubPattern>();
				for (int i = 0; i < info.Parts.Count; i++)
				{
					var subPattern = new SubPattern(this, info.Parts[i], true);
					this.SubPatterns.Add(subPattern);

					int layerCount = reader.ReadByte();
					for (int j = 0; j < layerCount; j++)
					{
						var layerType = reader.ReadByte();
						if (layerType == 0x01)
						{
							int objectX = reader.ReadInt32();
							int objectY = reader.ReadInt32();
							int objectW = reader.ReadInt32();
							int objectH = reader.ReadInt32();
							byte crop = reader.ReadByte();
							byte resampling = reader.ReadByte();
							int nameLength = reader.ReadInt32();
							string name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
							int bitmapWidth = reader.ReadInt32();
							int bitmapHeight = reader.ReadInt32();
							byte[] bitmapPixels = reader.ReadBytes(bitmapWidth * bitmapHeight * 4);
							var bitmap = new System.Drawing.Bitmap(bitmapWidth, bitmapHeight, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
							bitmap.FromBytes(bitmapPixels);
							SmartObjectLayer layer = new SmartObjectLayer(subPattern, name, bitmap, objectX, objectY, objectW, objectH);
							subPattern.Layers.Add(layer);
							layer.Crop = SmartObjectLayer.Crops[crop];
							layer.Resampler = SmartObjectLayer.Resamplers[resampling];
							layer.Colors = new UnityEngine.Color[layer.Width * layer.Height]; 
							layer.UpdateColors();
							layer.UpdateTexture();
						}
						else if (layerType == 0x00)
						{
							int nameLength = reader.ReadInt32();
							string name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
							RasterLayer layer = new RasterLayer(subPattern, name);
							layer.Colors = new UnityEngine.Color[layer.Width * layer.Height];
							for (int y = 0; y < layer.Height; y++)
								for (int x = 0; x < layer.Width; x++)
									layer.Colors[x + y * layer.Width] = new UnityEngine.Color(reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f, reader.ReadByte() / 255f);
							layer.UpdateTexture();
							subPattern.Layers.Add(layer);
						}
					}
				}
			}
		}
		_CurrentSubPattern = 0;
		Editor.SetSize(CurrentSubPattern.Width, CurrentSubPattern.Height);
		Editor.LayersChanged();
		Editor.SubPatternChanged(CurrentSubPattern.Part);
		Editor.SetType(this._Type);
		Editor.Show(null, null, null);
		Editor.OnImageUpdated();
		Editor.Tools.HistoryChanged(CurrentSubPattern.History);
		for (int i = 0; i < SubPatterns.Count; i++)
			SubPatterns[i].UpdateImage(false);
		this.RegeneratePreview();
	}

	private bool ParseResult()
	{
		if (IsReady)
		{
			lock (Result)
			{
				UnityEngine.Color[] cols = new UnityEngine.Color[Width * Height];
				System.Drawing.Bitmap resultBitmap;
				lock (Result)
				{
					resultBitmap = (Bitmap) Result.Clone();
				}
				PreviewTexture = resultBitmap.ToTexture2D(PreviewTexture);
				PreviewSprite = UnityEngine.Sprite.Create(PreviewTexture, new UnityEngine.Rect(0, 0, PreviewTexture.width, PreviewTexture.height), new UnityEngine.Vector2(0.5f, 0.5f));
				resultBitmap.Background(System.Drawing.Color.FromArgb(255, 255, 255));
				var scaledImage = Scaler.ScaleImage(Result, 4, null);
				UpscaledPreviewTexture = scaledImage.ToTexture2D(UpscaledPreviewTexture);
				scaledImage.Dispose();
				resultBitmap.Dispose();
				//Result.Dispose();
				IsReady = false;
				return true;
			}
		}
		return false;
	}

	public UnityEngine.Texture2D GetUpscaledPreview()
	{
		return UpscaledPreviewTexture;
	}

	public UnityEngine.Sprite GetPreviewSprite()
	{
		return PreviewSprite;
	}

	public bool Update()
	{
		return ParseResult();
	}

	public bool IsVisible(int x, int y)
	{
		foreach (var subPattern in SubPatterns)
		{
			if (x >= subPattern.Part.X && x < subPattern.Part.X + subPattern.Part.Width &&
				y >= subPattern.Part.Y && y < subPattern.Part.Y + subPattern.Part.Height)
			{
				int px = x - subPattern.Part.X;
				int py = y - subPattern.Part.Y;
				if (subPattern.Part.Visible[px + py * subPattern.Part.Width] == '1')
					return true;
				else
					return false;
			}
		}
		return false;
	}

	public void RegeneratePreview()
	{
		if (!this.IsParsing)
		{
			IsParsing = true;
			Thread thread = new Thread(() =>
			{
				try
				{
					var bitmap = new Bitmap(Width, Height, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
					var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
					var padding = data.Stride - (data.Width * pixelSize);

					List<int> transparentPixels = new List<int>();
					unsafe
					{
						byte* ptr = (byte*) data.Scan0.ToPointer();

						var index = 0;
						for (var y = 0; y < data.Height; y++)
						{
							for (var x = 0; x < data.Width; x++)
							{
								if (IsVisible(x, y))
								{
									int idx = x + y * Width;
									(*(ptr + index + 2)) = (byte) (Colors[idx].r * 255f);
									(*(ptr + index + 1)) = (byte) (Colors[idx].g * 255f);
									(*(ptr + index + 0)) = (byte) (Colors[idx].b * 255f);
									(*(ptr + index + 3)) = 255;
									if (Colors[idx].a == 0f)
										transparentPixels.Add(idx);
								}
								index += pixelSize;
							}
							index += padding;
						}
					}
					bitmap.UnlockBits(data);

					if (Quantizer is BaseColorCacheQuantizer colorCacheQuantizer)
						colorCacheQuantizer.ChangeCacheProvider(ColorCache);

					var targetImage = (Bitmap) ImageBuffer.QuantizeImage(bitmap, Quantizer, null, 15, 1);
					bitmap.Dispose();

					bitmap = targetImage;

					lock (Result)
					{
						if (Result != null)
							Result.Dispose();
						Result = bitmap;
						IsReady = true;
						IsParsing = false;
					}
				}
				catch (Exception e)
				{
					IsParsing = false;
				}
			});
			thread.Start();
		}
	}

	public void Dispose()
	{
		for (var i = 0; i < SubPatterns.Count; i++)
			SubPatterns[i].Dispose();
	}
}
