using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.EuclideanDistance;
using SimplePaletteQuantizer.ColorCaches.LocalitySensitiveHash;
using SimplePaletteQuantizer.ColorCaches.Octree;
using SimplePaletteQuantizer.Quantizers;
using SimplePaletteQuantizer.Quantizers.DistinctSelection;
using SimplePaletteQuantizer.Quantizers.MedianCut;
using SimplePaletteQuantizer.Quantizers.Octree;
using SimplePaletteQuantizer.Quantizers.Popularity;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

public class Pattern
{
	public PatternEditor Editor;
	public List<SubPattern> SubPatterns;
	public IColorQuantizer Quantizer;
	public IColorCache ColorCache;
	public TextureBitmap Bitmap;
	public TextureBitmap PreviewBitmap;
	public TextureBitmap UpscaledPreviewBitmap;

	private DesignPattern DesignPattern;
	private DesignPatternInformation.DesignPatternInfo Info;
	private bool IsParsing = false;
	private bool IsReady = false;
	private UnityEngine.Texture2D UpscaledPreviewTexture;
	private UnityEngine.Texture2D PreviewTexture;
	private UnityEngine.Sprite PreviewSprite;
	private int _CurrentSubPattern;
	private bool Disposed = false;
	private Thread PreviewThread;
	private bool Reparse;
	private bool NeedReparse;
	private ManualResetEvent ReparseLock = new ManualResetEvent(false);

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
			return _Type == DesignPattern.TypeEnum.SimplePattern ? 32 : 64;
		}
	}

	public int Height
	{
		get
		{
			return _Type == DesignPattern.TypeEnum.SimplePattern ? 32 : 64;
		}
	}

	private void StartPreviewThread()
	{
		PreviewThread = new Thread(() =>
		{
			while (true)
			{
				ReparseLock.WaitOne();
				if (Disposed)
				{
					Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Preview generator thread stopped!");
					return;
				}
				ReparseLock.Reset();
				try
				{
					if (Quantizer is BaseColorCacheQuantizer colorCacheQuantizer)
						colorCacheQuantizer.ChangeCacheProvider(ColorCache);

					this.PreviewBitmap.CopyFrom(this.Bitmap);
					this.PreviewBitmap.Quantize(Quantizer, 15);
					
					int[] src = this.PreviewBitmap.ConvertToInt();
					int[] target = new int[UpscaledPreviewBitmap.Width * UpscaledPreviewBitmap.Height];
					Scaler.ScaleImage(4, src, target, this.PreviewBitmap.Width, this.PreviewBitmap.Height, new xBRZNet.ScalerCfg(), 0, int.MaxValue);

					this.UpscaledPreviewBitmap.FromInt(target);
					IsReady = true;
					IsParsing = false;
				}
				catch (System.Exception e)
				{
					Logger.Log(Logger.Level.ERROR, "[PatternEditor/Pattern] Error while generating pattern preview: " + e.ToString());
				}
			}
		});
		PreviewThread.Start();
	}

	public Pattern(PatternEditor editor, DesignPattern pattern)
	{
		try
		{
			Logger.Log(Logger.Level.INFO, "[PatternEditor/Pattern] Creating new pattern");
			StartPreviewThread();
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Preview generator thread started!");
			_Type = pattern.Type;
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Pattern type: " + _Type.ToString()); 
			Bitmap = new TextureBitmap(pattern.Width, pattern.Height);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created TextureBitmap " + pattern.Width + "x" + pattern.Height);
			Bitmap.Clear();
			PreviewBitmap = new TextureBitmap(pattern.Width, pattern.Height);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created preview TextureBitmap " + pattern.Width + "x" + pattern.Height);
			PreviewBitmap.Clear();
			PreviewSprite = UnityEngine.Sprite.Create(PreviewBitmap.Texture, new UnityEngine.Rect(0, 0, PreviewBitmap.Width, PreviewBitmap.Height), new UnityEngine.Vector2(0.5f, 0.5f));
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created preview sprite");

			UpscaledPreviewBitmap = new TextureBitmap(pattern.Width * 4, pattern.Height * 4);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created upscaled preview TextureBitmap " + (pattern.Width * 4) + "x" + (pattern.Height * 4));
			UpscaledPreviewBitmap.Clear();

			Quantizer = Quantizers[0];
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Selected Quantizer: " + Quantizer.GetType().ToString());
			ColorCache = ColorCaches[0];
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Selected Color Cache: " + ColorCache.GetType().ToString());

			Editor = editor;
			DesignPattern = pattern;
			var colors = pattern.GetPixels();

			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Parsing colors of pattern...");
			unsafe
			{
				var bitmapColors = Bitmap.GetColors();
				for (int y = 0; y < Height; y++)
				{
					for (int x = 0; x < Width; x++)
					{
						var col = new TextureBitmap.Color(
							(byte) (colors[x + y * Width].r * 255f),
							(byte) (colors[x + y * Width].g * 255f),
							(byte) (colors[x + y * Width].b * 255f),
							(byte) (colors[x + y * Width].a * 255f)
						);
						*(bitmapColors + x + (Height - 1 - y) * Width) = col;
					}
				}
			}
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Parsed " + (Width * Height) + " pixels.");
			Info = DesignPatternInformation.Types[pattern.Type];
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Pattern information obtained.");
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[PatternEditor/Pattern] Error while creating pattern: " + e.ToString());
		}
	}

	public void Load()
	{
		try
		{
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Creating sub patterns...");
			SubPatterns = new List<SubPattern>();
			foreach (var part in Info.Parts)
			{
				Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Creating sub pattern #" + SubPatterns.Count + " (X:" + part.X + ";Y:" + part.Y + ";W:" + part.Width + ";H:" + part.Height + ")");
				SubPatterns.Add(new SubPattern(this, part));
			}

			Editor.SetSize(CurrentSubPattern.Width, CurrentSubPattern.Height);
			CurrentSubPattern.SelectLayer(0);
			CurrentSubPattern.UpdateImage();
			NeedReparse = true;
			Editor.LayersChanged();
			Editor.SubPatternChanged(CurrentSubPattern.Part);
			Editor.Tools.HistoryChanged(CurrentSubPattern.History);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[PatternEditor/Pattern] Error while loading: " + e.ToString());
		}
	}

	public void ChangeQuantizer(int num)
	{
		Quantizer = Quantizers[num];
		NeedReparse = true;
	}

	public void ChangeColorCache(int num)
	{
		ColorCache = ColorCaches[num];
		NeedReparse = true;
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
		var p = new SimpleDesignPattern();
		p.Type = this.Type;
		//p.IsPro = this.Type != DesignPattern.TypeEnum.SimplePattern;
		p.Image = new byte[p.Width / 2 * p.Height];
		p.Empty();
		var colors = p.GetPixels();

		unsafe
		{
			var bitmapColors = Bitmap.GetColors();
			for (int y = 0; y < Height; y++)
			{
				for (int x = 0; x < Width; x++)
				{
					var col = new TextureBitmap.Color(
						(byte) (colors[x + y * Width].r * 255f),
						(byte) (colors[x + y * Width].g * 255f),
						(byte) (colors[x + y * Width].b * 255f),
						(byte) (colors[x + y * Width].a * 255f)
					);
					*(bitmapColors + x + y * Width) = col;
				}
			}
		}
		Load();
	}

	public byte[] ToBytes()
	{
		using (MemoryStream stream = new MemoryStream())
		{
			BinaryWriter writer = new BinaryWriter(stream);
			writer.Write((byte) 0x01); // version
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
						writer.Write((byte) sol.Crop);
						writer.Write((byte) sol.Resampler);
						var nameBytes = System.Text.Encoding.UTF8.GetBytes(sol.Name);
						writer.Write(nameBytes.Length);
						writer.Write(nameBytes);
						writer.Write(sol.Bitmap.Width);
						writer.Write(sol.Bitmap.Height);
						int m = sol.Bitmap.Width * sol.Bitmap.Height * sol.Bitmap.PixelSize;
						unsafe 
						{
							byte* ptr = (byte*) sol.Bitmap.Bytes.ToPointer();
							for (int k = 0; k < m; k++)
								writer.Write(*(ptr + k));
						}
					}
					else
					{
						writer.Write((byte)0x00); // raster layer
						var nameBytes = System.Text.Encoding.UTF8.GetBytes(layer.Name);
						writer.Write(nameBytes.Length);
						writer.Write(nameBytes);
						unsafe
						{
							int m = layer.Width * layer.Height * layer.Texture.PixelSize;
							byte* ptr = (byte*) layer.Texture.Bytes.ToPointer();
							for (int k = 0; k < m; k++)
								writer.Write(*(ptr + k));
						}
					}
				}
			}
			return stream.ToArray();
		}
	}

	public Pattern(PatternEditor editor, byte[] bytes)
	{
		try
		{
			Editor = editor;
			
			using (MemoryStream stream = new MemoryStream(bytes))
			{
				BinaryReader reader = new BinaryReader(stream);
				byte version = reader.ReadByte();
				var t = (DesignPattern.TypeEnum) reader.ReadByte();
				if (!Editor.IsPro && t != DesignPattern.TypeEnum.SimplePattern)
					throw new ArgumentException("Simple design spot can't hold pro design.", "project");
				if (Editor.IsPro && t == DesignPattern.TypeEnum.SimplePattern)
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
							var bitmap = new TextureBitmap(bitmapWidth, bitmapHeight);
							int m = bitmapWidth * bitmapHeight * bitmap.PixelSize;
							unsafe
							{
								byte* ptr = (byte*) bitmap.Bytes.ToPointer();
								for (int k = 0; k < m; k++)
									*(ptr + k) = bitmapPixels[k];
							}
							if (version == 0)
								bitmap.FlipY();
							SmartObjectLayer layer = new SmartObjectLayer(subPattern, name, bitmap, objectX, objectY, objectW, objectH);
							subPattern.Layers.Add(layer);
							layer.Crop = (TextureBitmap.CropMode) crop;
							layer.Resampler = (ResamplingFilters) resampling;
							layer.Bitmap = bitmap;
							layer.UpdateColors();
							layer.UpdateTexture();
						}
						else if (layerType == 0x00)
						{
							int nameLength = reader.ReadInt32();
							string name = System.Text.Encoding.UTF8.GetString(reader.ReadBytes(nameLength));
							RasterLayer layer = new RasterLayer(subPattern, name);

							int m = layer.Width * layer.Height * layer.Texture.PixelSize; 
							byte[] bitmapPixels = reader.ReadBytes(m);
							layer.Texture = new TextureBitmap(layer.Width, layer.Height);
							
							unsafe
							{
								byte* ptr = (byte*) layer.Texture.Bytes.ToPointer();
								for (int k = 0; k < m; k++)
									*(ptr + k) = bitmapPixels[k];
							}
							if (version == 0)
								layer.Texture.FlipY();

							layer.UpdateTexture();
							subPattern.Layers.Add(layer);
						}
					}
				}
			}
			GC.Collect();
			_CurrentSubPattern = 0;

			Logger.Log(Logger.Level.INFO, "[PatternEditor/Pattern] Creating new pattern");
			StartPreviewThread();
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Pattern type: " + _Type.ToString());
			Bitmap = new TextureBitmap(Width, Height);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created TextureBitmap " + Width + "x" + Height);
			Bitmap.Clear();
			PreviewBitmap = new TextureBitmap(Width, Height);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created preview TextureBitmap " + Width + "x" + Height);
			PreviewBitmap.Clear();
			PreviewSprite = UnityEngine.Sprite.Create(PreviewBitmap.Texture, new UnityEngine.Rect(0, 0, PreviewBitmap.Width, PreviewBitmap.Height), new UnityEngine.Vector2(0.5f, 0.5f));
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created preview sprite");

			UpscaledPreviewBitmap = new TextureBitmap(Width * 4, Height * 4);
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Created upscaled preview TextureBitmap " + (Width * 4) + "x" + (Height * 4));
			UpscaledPreviewBitmap.Clear();

			Info = DesignPatternInformation.Types[this._Type];
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor/Pattern] Pattern information obtained.");

		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[PatternEditor/Pattern] Error while creating pattern: " + e.ToString());
			this.Dispose();
			throw e;
		}
	}

	public void ProjectLoaded()
	{
		for (int i = 0; i < SubPatterns.Count; i++)
			SubPatterns[i].UpdateImage(false);

		Editor.SetSize(CurrentSubPattern.Width, CurrentSubPattern.Height);
		Editor.SetType(this._Type);
		Editor.Show(null, null, null);
		Editor.LayersChanged();
		Editor.SubPatternChanged(CurrentSubPattern.Part);
		Editor.PixelGrid.UpdateImage();
		Editor.Tools.HistoryChanged(CurrentSubPattern.History);
		RegeneratePreview();
	}

	public UnityEngine.Texture2D GetUpscaledPreview()
	{
		return this.UpscaledPreviewBitmap.Texture;
	}

	public UnityEngine.Sprite GetPreviewSprite()
	{
		return PreviewSprite;
	}

	public bool Update()
	{
		if (NeedReparse)
		{
			if (!IsParsing)
			{
				IsParsing = true;
				ReparseLock.Set();
				NeedReparse = false;
			}
		}
		if (IsReady)
		{
			PreviewBitmap.Apply();
			PreviewBitmap.Texture.filterMode = UnityEngine.FilterMode.Point;
			PreviewBitmap.Texture.wrapMode = UnityEngine.TextureWrapMode.Clamp;
			UpscaledPreviewBitmap.Apply();

			IsReady = false;
			return true;
		}
		return false;
	}

	public void RegeneratePreview()
	{
		NeedReparse = true;
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

	public void Dispose()
	{
		if (!Disposed)
		{
			Disposed = true;
			ReparseLock.Set();
			Bitmap.Dispose();
			PreviewBitmap.Dispose();
			UpscaledPreviewBitmap.Dispose();
			if (SubPatterns != null)
				for (var i = 0; i < SubPatterns.Count; i++)
					SubPatterns[i].Dispose();
		}
	}
}
