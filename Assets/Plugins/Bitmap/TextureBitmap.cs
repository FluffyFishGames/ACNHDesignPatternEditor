using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Runtime.InteropServices;
using UnityEngine.Rendering;
using SimplePaletteQuantizer.Quantizers;


public unsafe class TextureBitmap
{
	[StructLayout(LayoutKind.Sequential)]
	public struct Color
	{
		public byte R;
		public byte G;
		public byte B;
		public byte A;

		public int Distance(Color other)
		{
			var dr = (int) Mathf.Abs((this.R - other.R));
			var dg = (int) Mathf.Abs((this.G - other.G));
			var db = (int) Mathf.Abs((this.B - other.B));
			var da = (int) Mathf.Abs((this.A - other.A));
			return Mathf.Max(dr, dg, db, da);
		}

		public static Color FromARGB(int argb)
		{
			return new Color(
				(byte) ((argb >> 16) & 0xFF),
				(byte) ((argb >> 8) & 0xFF),
				(byte) ((argb) & 0xFF),
				(byte) ((argb >> 24) & 0xFF)
			);
		}

		public float GetHue()
		{
			float delta, min;
			float h = 0, s, v;

			min = Math.Min(Math.Min(R, G), B);
			v = Math.Max(Math.Max(R, G), B);
			delta = v - min;

			if (v == 0.0)
				s = 0;
			else
				s = delta / v;

			if (s == 0)
				h = 0.0f;
			else
			{
				if (R == v)
					h = (G - B) / delta;
				else if (G == v)
					h = 2 + (B - R) / delta;
				else if (B == v)
					h = 4 + (R - G) / delta;

				h *= 60;

				if (h < 0.0)
					h = h + 360;
			}

			return h;
		}

		public float GetSaturation()
		{
			float delta, min;
			float s, v;

			min = Math.Min(Math.Min(R, G), B);
			v = Math.Max(Math.Max(R, G), B);
			delta = v - min;

			if (v == 0.0)
				s = 0;
			else
				s = delta / v;

			return s;
		}

		public float GetBrightness()
		{
			float min;
			float v;

			min = Math.Min(Math.Min(R, G), B);
			v = Math.Max(Math.Max(R, G), B);
			
			return v;
		}

		public int ToARGB()
		{
			return (((int) A << 24) | ((int) R << 16) | ((int) G << 8) | ((int) B));
		}

		public Color(byte R, byte G, byte B, byte A)
		{
			this.R = R;
			this.G = G;
			this.B = B;
			this.A = A;
		}

		public Color AlphaComposite(Color color)
		{
			float br = this.R / 255f;
			float bg = this.G / 255f;
			float bb = this.B / 255f;
			float ba = this.A / 255f;
			float cr = color.R / 255f;
			float cg = color.G / 255f;
			float cb = color.B / 255f;
			float ca = color.A / 255f;

			float alpha = ba + ca - ba * ca;
			float sbr = br * ba;
			float sbg = bg * ba;
			float sbb = bb * ba;
			float scr = cr * ca;
			float scg = cg * ca;
			float scb = cb * ca;

			if (alpha > 0f)
				return new Color()
				{
					R = (byte) (UnityEngine.Mathf.Clamp01((scr + sbr * (1 - ca)) / alpha) * 255f),
					G = (byte) (UnityEngine.Mathf.Clamp01((scg + sbg * (1 - ca)) / alpha) * 255f),
					B = (byte) (UnityEngine.Mathf.Clamp01((scb + sbb * (1 - ca)) / alpha) * 255f),
					A = (byte) (UnityEngine.Mathf.Clamp01(alpha) * 255f)
				};
			else
				return new Color() { R = 255, G = 255, B = 255, A = 0 };
		}

		public override int GetHashCode()
		{
			return ToARGB();
		}
	}

	private static Dictionary<SkiaSharp.SKColorType, byte> ColorSize = new Dictionary<SkiaSharp.SKColorType, byte>()
	{
		{ SkiaSharp.SKColorType.Alpha8,      1 },
		{ SkiaSharp.SKColorType.Argb4444,    2 },
		{ SkiaSharp.SKColorType.Bgra8888,    4 },
		{ SkiaSharp.SKColorType.Gray8,       1 },
		{ SkiaSharp.SKColorType.Rgb101010x,  4 },
		{ SkiaSharp.SKColorType.Rgb565,      2 },
		{ SkiaSharp.SKColorType.Rgb888x,     4 },
		{ SkiaSharp.SKColorType.Rgba1010102, 4 },
		{ SkiaSharp.SKColorType.Rgba8888,    4 }
	};

	private static Dictionary<SkiaSharp.SKColorType, Action<IntPtr, IntPtr, int, int>> ColorTransformers = new Dictionary<SkiaSharp.SKColorType, Action<IntPtr, IntPtr, int, int>>()
	{
		{SkiaSharp.SKColorType.Bgra8888,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 4;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 0) = *(ptrIn + i + 2);
					*(ptrOut + o + 1) = *(ptrIn + i + 1);
					*(ptrOut + o + 2) = *(ptrIn + i + 0);
					*(ptrOut + o + 3) = *(ptrIn + i + 3);
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Alpha8,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 1;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 0) = 0;
					*(ptrOut + o + 1) = 0;
					*(ptrOut + o + 2) = 0;
					*(ptrOut + o + 3) = *(ptrIn + i + 0);
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Argb4444,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 2;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 3) = (byte) (*(ptrIn + i + 0) >> 4);
					*(ptrOut + o + 0) = (byte) (*(ptrIn + i + 0) & 0x0F);
					*(ptrOut + o + 1) = (byte) (*(ptrIn + i + 1) >> 4);
					*(ptrOut + o + 2) = (byte) (*(ptrIn + i + 1) & 0x0F);
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Gray8,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 1;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 3) = *(ptrIn + i + 0);
					*(ptrOut + o + 0) = *(ptrIn + i + 0);
					*(ptrOut + o + 1) = *(ptrIn + i + 0);
					*(ptrOut + o + 2) = 255;
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Rgb101010x,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 4;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					byte byte0 = *(ptrIn + i + 0);
					byte byte1 = *(ptrIn + i + 1);
					byte byte2 = *(ptrIn + i + 2);
					byte byte3 = *(ptrIn + i + 3);
					int r = (((int)byte0) << 2) + (byte1 >> 6);
					int g = ((byte1 & (0xFF >> 2)) << 4) + (byte2 >> 4);
					int b = ((byte2 & (0xFF >> 4)) << 6) + (byte3 >> 2);
					*(ptrOut + o + 0) = (byte) (r / 4);
					*(ptrOut + o + 1) = (byte) (g / 4);
					*(ptrOut + o + 2) = (byte) (b / 4);
					*(ptrOut + o + 3) = 255;
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Rgb565,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 2;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					byte byte0 = *(ptrIn + i + 0);
					byte byte1 = *(ptrIn + i + 1);
					int r = (((int)byte0) >> 3);
					int g = ((byte0 & (0xFF >> 3)) << 3) + (byte1 >> 3);
					int b = ((byte1 & (0xFF >> 3)));
					*(ptrOut + o + 0) = (byte) (r * 8.22f);
					*(ptrOut + o + 1) = (byte) (g * 4.04f);
					*(ptrOut + o + 2) = (byte) (b * 8.22f);
					*(ptrOut + o + 3) = 255;
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Rgb888x,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 4;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 0) = *(ptrIn + i + 0);
					*(ptrOut + o + 1) = *(ptrIn + i + 1);
					*(ptrOut + o + 2) = *(ptrIn + i + 2);
					*(ptrOut + o + 3) = 255;
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Rgba1010102,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 4;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					byte byte0 = *(ptrIn + i + 0);
					byte byte1 = *(ptrIn + i + 1);
					byte byte2 = *(ptrIn + i + 2);
					byte byte3 = *(ptrIn + i + 3);
					int r = (((int)byte0) << 2) + (byte1 >> 6);
					int g = ((byte1 & (0xFF >> 2)) << 4) + (byte2 >> 4);
					int b = ((byte2 & (0xFF >> 4)) << 6) + (byte3 >> 2);
					int a = ((byte3 & (0xFF >> 2)));
					*(ptrOut + o + 0) = (byte) (r / 4);
					*(ptrOut + o + 1) = (byte) (g / 4);
					*(ptrOut + o + 2) = (byte) (b / 4);
					*(ptrOut + o + 3) = (byte) (a * 85);
					o += 4;
				}
			}
		},
		{SkiaSharp.SKColorType.Rgba8888,
			(IntPtr @in, IntPtr @out, int width, int height) =>
			{
				byte pixelSize = 4;
				int size = width * height * pixelSize;
				byte* ptrIn = (byte*) @in.ToPointer();
				byte* ptrOut = (byte*) @out.ToPointer();
				for (int i = 0, o = 0; i < size; i+= pixelSize)
				{
					*(ptrOut + o + 0) = *(ptrIn + i + 0);
					*(ptrOut + o + 1) = *(ptrIn + i + 1);
					*(ptrOut + o + 2) = *(ptrIn + i + 2);
					*(ptrOut + o + 3) = *(ptrIn + i + 3);
					o += 4;
				}
			}
		}
	};
	public Texture2D Texture { get; private set; }
	public IntPtr Bytes { get; private set; }
	public int Width { get; private set; }
	public int Height { get; private set; }
	public int PixelSize { get { return 4; } }


	private Dictionary<SkiaSharp.SKColorType, TextureFormat> SkiaToTexture = new Dictionary<SkiaSharp.SKColorType, TextureFormat>()
	{ 
		{ SkiaSharp.SKColorType.Bgra8888, TextureFormat.BGRA32 },
		{ SkiaSharp.SKColorType.Rgba8888, TextureFormat.RGBA32 },
		{ SkiaSharp.SKColorType.Rgb888x,  TextureFormat.RGBA32 }
	};
	private CommandBuffer CommandBuffer;

#if UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("NativeTextureBitmap.dll")]
#else
	[System.Runtime.InteropServices.DllImport("__Internal")]
#endif
	static extern System.IntPtr GetTextureBitmapUpdateCallback();
#if UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("NativeTextureBitmap.dll")]
#else
	[System.Runtime.InteropServices.DllImport("__Internal")]
#endif
	static extern void RegisterTexture(uint id, IntPtr ptr);
#if UNITY_EDITOR
	[System.Runtime.InteropServices.DllImport("NativeTextureBitmap.dll")]
#else
	[System.Runtime.InteropServices.DllImport("__Internal")]
#endif
	static extern void UnregisterTexture(uint id);

	private static uint CurrentID = 0;
	private uint ID;

	public enum ImageFormat
	{
		PNG,
		JPG,
		WEBP,
		GIF,
		BMP
	};

	public Color* GetColors()
	{
		return (Color*) (Bytes.ToPointer());
	}

	public TextureBitmap(int width, int height, bool createBackgroundTexture = true)
	{
		ID = CurrentID;
		CurrentID++;

		Width = width;
		Height = height;

		Bytes = Marshal.AllocHGlobal(width * height * PixelSize);
		RegisterTexture(ID, Bytes);

		if (createBackgroundTexture)
		{
			CreateBackgroundTexture();
		}
	}

	public void CreateBackgroundTexture()
	{
		if (this.Texture == null)
		{
			Texture = new Texture2D(Width, Height, TextureFormat.RGBA32, false);
			CommandBuffer = new CommandBuffer();
			CommandBuffer.IssuePluginCustomTextureUpdateV2(GetTextureBitmapUpdateCallback(), this.Texture, ID);
		}
	}

	public class Rectangle
	{
		public int X;
		public int Y;
		public int Width;
		public int Height;

		public Rectangle(int x, int y, int width, int height)
		{
			X = x;
			Y = y;
			Width = width;
			Height = height;
		}
	}

	public class Point
	{
		public int X;
		public int Y;

		public Point(int x, int y)
		{
			X = x;
			Y = y;
		}
	}

	public void Clear()
	{
		int w = Width;
		int h = Height;
		int m = Width * Height * PixelSize;
		var ptr = (byte*) Bytes.ToPointer();
		for (int i = 0; i < m; i++)
			*(ptr + i) = 0;
	}

	public void AlphaComposite(TextureBitmap other, Color tint, Point point = null, Rectangle otherRect = null, Func<int, int, bool> isDrawable = null)
	{
		int myWidth = Width;
		int myHeight = Height;
		int myPixelSize = PixelSize;
		int otherWidth = other.Width;
		int otherHeight = other.Height;
		int otherPixelSize = other.PixelSize;
		if (point == null) point = new Point(0, 0);
		if (otherRect == null) otherRect = new Rectangle(0, 0, otherWidth, otherHeight);
		float tintR = tint.R / 255f;
		float tintG = tint.G / 255f;
		float tintB = tint.B / 255f;
		float tintA = tint.A / 255f;
		const int BackgroundR = 0, BackgroundG = 1, BackgroundB = 2, BackgroundA = 3, OtherR = 4, OtherG = 5, OtherB = 6, OtherA = 7, Alpha = 8, ScaledBackgroundR = 9, ScaledBackgroundG = 10, ScaledBackgroundB = 11, ScaledOtherR = 12, ScaledOtherG = 13, ScaledOtherB = 14;
		IntPtr cache = Marshal.AllocHGlobal(4 * 15);
		float* cachePointer = (float*) cache.ToPointer();
		byte* myPointer = (byte*) Bytes.ToPointer();
		byte* otherPointer = (byte*) other.Bytes.ToPointer();

		int w = point.X + otherRect.Width;
		int h = point.Y + otherRect.Height;
		if (w > Width) w = Width;
		if (h > Height) h = Height;

		int ox = otherRect.X;
		int oy = otherRect.Y;
		for (int py = point.Y; py < h; py++)
		{
			ox = otherRect.X;
			for (int px = point.X; px < w; px++)
			{
				if (px >= 0 && px < myWidth && py >= 0 && py < myHeight)
				{
					if (isDrawable != null && !isDrawable(px, py))
					{
						ox++;
						continue;
					}
					int myIndex = (px + (myHeight - 1 - py) * myWidth) * myPixelSize;
					int otherIndex = (ox + (otherHeight - 1 - oy) * otherWidth) * otherPixelSize;

					*(cachePointer + BackgroundR) = *(myPointer + myIndex + 0) / 255f;
					*(cachePointer + BackgroundG) = *(myPointer + myIndex + 1) / 255f;
					*(cachePointer + BackgroundB) = *(myPointer + myIndex + 2) / 255f;
					*(cachePointer + BackgroundA) = *(myPointer + myIndex + 3) / 255f;
					*(cachePointer + OtherR) = (*(otherPointer + otherIndex + 0) / 255f * tintR);
					*(cachePointer + OtherG) = (*(otherPointer + otherIndex + 1) / 255f * tintG);
					*(cachePointer + OtherB) = (*(otherPointer + otherIndex + 2) / 255f * tintB);
					*(cachePointer + OtherA) = (*(otherPointer + otherIndex + 3) / 255f * tintA);

					*(cachePointer + Alpha) = *(cachePointer + BackgroundA) + *(cachePointer + OtherA) - *(cachePointer + BackgroundA) * *(cachePointer + OtherA);

					if (*(cachePointer + Alpha) > 0f)
					{
						*(cachePointer + ScaledBackgroundR) = *(cachePointer + BackgroundR) * *(cachePointer + BackgroundA);
						*(cachePointer + ScaledBackgroundG) = *(cachePointer + BackgroundG) * *(cachePointer + BackgroundA);
						*(cachePointer + ScaledBackgroundB) = *(cachePointer + BackgroundB) * *(cachePointer + BackgroundA);
						*(cachePointer + ScaledOtherR) = *(cachePointer + OtherR) * *(cachePointer + OtherA);
						*(cachePointer + ScaledOtherG) = *(cachePointer + OtherG) * *(cachePointer + OtherA);
						*(cachePointer + ScaledOtherB) = *(cachePointer + OtherB) * *(cachePointer + OtherA);

						*(myPointer + myIndex + 0) = (byte) (UnityEngine.Mathf.Clamp01((*(cachePointer + ScaledOtherR) + *(cachePointer + ScaledBackgroundR) * (1 - *(cachePointer + OtherA))) / *(cachePointer + Alpha)) * 255f);
						*(myPointer + myIndex + 1) = (byte) (UnityEngine.Mathf.Clamp01((*(cachePointer + ScaledOtherG) + *(cachePointer + ScaledBackgroundG) * (1 - *(cachePointer + OtherA))) / *(cachePointer + Alpha)) * 255f);
						*(myPointer + myIndex + 2) = (byte) (UnityEngine.Mathf.Clamp01((*(cachePointer + ScaledOtherB) + *(cachePointer + ScaledBackgroundB) * (1 - *(cachePointer + OtherA))) / *(cachePointer + Alpha)) * 255f);
						*(myPointer + myIndex + 3) = (byte) (UnityEngine.Mathf.Clamp01(*(cachePointer + Alpha)) * 255f);
					}
					else
					{
						*(myPointer + myIndex + 0) = 255;
						*(myPointer + myIndex + 1) = 255;
						*(myPointer + myIndex + 2) = 255;
						*(myPointer + myIndex + 3) = 0;
					}
				}
				ox++;
			}
			oy++;
		}
		Marshal.FreeHGlobal(cache);
	}

	public void Subtract(TextureBitmap other, Color tint, Point point = null, Rectangle otherRect = null, Func<int, int, bool> isDrawable = null)
	{
		int myWidth = Width;
		int myHeight = Height;
		int myPixelSize = PixelSize;
		int otherWidth = other.Width;
		int otherHeight = other.Height;
		int otherPixelSize = other.PixelSize;

		float tintR = tint.R / 255f;
		float tintG = tint.G / 255f;
		float tintB = tint.B / 255f;
		float tintA = tint.A / 255f;

		byte* myPointer = (byte*) Bytes.ToPointer();
		byte* otherPointer = (byte*) other.Bytes.ToPointer();

		if (point == null) point = new Point(0, 0);
		if (otherRect == null) otherRect = new Rectangle(0, 0, otherWidth, otherHeight);
		int w = point.X + otherRect.Width;
		int h = point.Y + otherRect.Height;
		if (w > Width) w = Width;
		if (h > Height) h = Height;
		int ox = otherRect.X;
		int oy = otherRect.Y;

		for (int py = point.Y; py < h; py++)
		{
			ox = otherRect.X;
			for (int px = point.X; px < w; px++)
			{
				if (px >= 0 && px < myWidth && py >= 0 && py < myHeight)
				{
					if (isDrawable != null && !isDrawable(px, py))
					{
						ox++;
						continue;
					}
					int myIndex = (px + (myHeight - 1 - py) * myWidth) * myPixelSize;
					int otherIndex = (ox + (otherHeight - 1 - oy) * otherWidth) * otherPixelSize;

					*(myPointer + myIndex + 0) = (byte) UnityEngine.Mathf.Clamp((int) (*(myPointer + myIndex + 0)) - (int) (*(otherPointer + otherIndex + 0) * tintR), 0, 255);
					*(myPointer + myIndex + 1) = (byte) UnityEngine.Mathf.Clamp((int) (*(myPointer + myIndex + 1)) - (int) (*(otherPointer + otherIndex + 1) * tintG), 0, 255);
					*(myPointer + myIndex + 2) = (byte) UnityEngine.Mathf.Clamp((int) (*(myPointer + myIndex + 2)) - (int) (*(otherPointer + otherIndex + 2) * tintB), 0, 255);
					*(myPointer + myIndex + 3) = (byte) UnityEngine.Mathf.Clamp((int) (*(myPointer + myIndex + 3)) - (int) (*(otherPointer + otherIndex + 3) * tintA), 0, 255);
				}
				ox++;
			}
			oy++;
		}
	}

	public int[] ConvertToInt()
	{
		int w = Width;
		int h = Height;
		int m = w * h;
		Color* myPointer = (Color*) Bytes.ToPointer();
		int[] ret = new int[m];
		for (int i = 0; i < m; i++)
		{
			ret[i] = (*(myPointer + i)).ToARGB();
		}
		return ret;
	}
	
	public void FromInt(int[] data)
	{
		int w = Width;
		int h = Height;
		int m = w * h;
		Color* myPointer = (Color*) Bytes.ToPointer();
		for (int i = 0; i < m; i++)
		{
			*(myPointer + i) = Color.FromARGB(data[i]);
		}
	}

	public void Replace(TextureBitmap other, Color tint, Point point = null, Rectangle otherRect = null)
	{
		int myWidth = Width;
		int myHeight = Height;
		int myPixelSize = PixelSize;
		int otherWidth = other.Width;
		int otherHeight = other.Height;
		int otherPixelSize = other.PixelSize;

		float tintR = tint.R / 255f;
		float tintG = tint.G / 255f;
		float tintB = tint.B / 255f;
		float tintA = tint.A / 255f;

		byte* myPointer = (byte*) Bytes.ToPointer();
		byte* otherPointer = (byte*) other.Bytes.ToPointer();

		if (point == null) point = new Point(0, 0);
		if (otherRect == null) otherRect = new Rectangle(0, 0, otherWidth, otherHeight);
		int w = point.X + otherRect.Width;
		int h = point.Y + otherRect.Height;
		if (w > Width) w = Width;
		if (h > Height) h = Height;
		int ox = otherRect.X;
		int oy = otherRect.Y;

		for (int py = point.Y; py < h; py++)
		{
			ox = otherRect.X;
			for (int px = point.X; px < w; px++)
			{
				if (px >= 0 && px < myWidth && py >= 0 && py < myHeight)
				{
					int myIndex = (px + (myHeight - 1 - py) * myWidth) * myPixelSize;
					int otherIndex = (ox + (otherHeight - 1 - oy) * otherWidth) * otherPixelSize;

					*(myPointer + myIndex + 0) = *(otherPointer + otherIndex + 0);
					*(myPointer + myIndex + 1) = *(otherPointer + otherIndex + 1);
					*(myPointer + myIndex + 2) = *(otherPointer + otherIndex + 2);
					*(myPointer + myIndex + 3) = *(otherPointer + otherIndex + 3);
				}
				ox++;
			}
			oy++;
		}
	}

	public TextureBitmap Clone()
	{
		var ret = new TextureBitmap(this.Width, this.Height);
		Buffer.MemoryCopy(this.Bytes.ToPointer(), ret.Bytes.ToPointer(), this.Width * this.Height * PixelSize, this.Width * this.Height * PixelSize);
		return ret;
	}

	public void CopyFrom(TextureBitmap bitmap)
	{
		this.Resize(bitmap.Width, bitmap.Height);
		Buffer.MemoryCopy(bitmap.Bytes.ToPointer(), this.Bytes.ToPointer(), this.Width * this.Height * PixelSize, this.Width * this.Height * PixelSize);
	}

	public void CopyFrom(IntPtr pointer)
	{
		Buffer.MemoryCopy(pointer.ToPointer(), this.Bytes.ToPointer(), this.Width * this.Height * PixelSize, this.Width * this.Height * PixelSize);
	}

	public void Resample(ResamplingFilters filter, int newWidth, int newHeight)
	{
		ResamplingService resamplingService = new ResamplingService();
		resamplingService.Filter = filter;
		var array = ResamplingFilter.ConvertByteArrayToArray((byte*) this.Bytes.ToPointer(), this.Width, this.Height);
		var result = resamplingService.Resample(array, newWidth, newHeight);
		this.InternalResize(newWidth, newHeight);
		ResamplingFilter.ConvertArrayToByteArray(result, (byte*) this.Bytes.ToPointer());
	}

	public enum CropMode
	{
		Scale = 0,
		Fit = 1,
		Cover = 2
	}

	public void ResampleAndCrop(ResamplingFilters filter, CropMode mode, int newWidth, int newHeight)
	{
		// fit
		int w = newWidth;
		int h = newHeight;
		if (mode == CropMode.Fit)
		{
			float factor1 = ((float) w / (float) h);
			float factor2 = ((float) Width / (float) Height);
			if (factor1 > factor2)
			{
				h = newHeight;
				w = (int) ((((float) h) / ((float) Height)) * Width);
			}
			else
			{
				w = newWidth;
				h = (int) ((((float) w) / ((float) Width)) * Height);
			}
		}
		else if (mode == CropMode.Cover)
		{
			float factor1 = ((float) w / (float) h);
			float factor2 = ((float) Width / (float) Height);
			if (factor1 > factor2)
			{
				w = newWidth;
				h = (int) ((((float) w) / ((float) Width)) * Height);
			}
			else
			{
				h = newHeight;
				w = (int) ((((float) h) / ((float) Height)) * Width);
			}
		}

		this.Resample(filter, w, h);

		Color* myPointer = (Color*) Bytes.ToPointer();
		var copy = Marshal.AllocHGlobal(w * h * PixelSize);
		Color* copyPointer = (Color*) copy.ToPointer();
		Buffer.MemoryCopy(myPointer, copyPointer, w * h * PixelSize, w * h * PixelSize);

		this.InternalResize(newWidth, newHeight);
		myPointer = (Color*) Bytes.ToPointer();
		
		int ox = (newWidth - w) / 2;
		int oy = (newHeight - h) / 2;
		for (int y = 0; y < newHeight; y++)
		{
			for (int x = 0; x < newWidth; x++)
			{
				if (x >= ox && x < ox + w && y >= oy && y < oy + h)
					*(myPointer + x + y * newWidth) = *(copyPointer + (x - ox) + (y - oy) * w);
				else *(myPointer + x + y * newWidth) = new Color(0, 0, 0, 0);
			}
		}
	}
	
	public void Move(Point point)
	{
		var w = Width;
		var h = Height;
		var ox = point.X;
		var oy = point.Y;
		Color* myPointer = (Color*) Bytes.ToPointer();
		var copy = Marshal.AllocHGlobal(w * h * PixelSize);
		Color* copyPointer = (Color*) copy.ToPointer();
		Buffer.MemoryCopy(myPointer, copyPointer, w * h * PixelSize, w * h * PixelSize);
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				if (x >= ox && x < ox + w && y >= oy && y < oy + h)
					*(myPointer + x + y * w) = *(copyPointer + (x - ox) + (y - oy) * w);
				else *(myPointer + x + y * w) = new Color(0, 0, 0, 0);
			}
		}
		Marshal.FreeHGlobal(copy);
	}

	public void Crop(Rectangle rect, bool keepSize = false)
	{
		var w = Width;
		var h = Height;
		var rw = rect.Width;
		var rh = rect.Height;
		var rx = rect.X;
		var ry = rect.Y;
		Color* myPointer = (Color*) Bytes.ToPointer();
		var cropData = Marshal.AllocHGlobal(rect.Width * rect.Height * PixelSize);
		Color* cropDataPointer = (Color*) cropData.ToPointer();
		for (int y = 0; y < rh; y++)
		{
			for (int x = 0; x < rw; x++)
			{
				int fx = rx + x;
				int fy = ry + y;
				if (fx >= 0 && fy >= 0 && fx < w && fy < h)
					*(cropDataPointer + x + y * rw) = *(myPointer + fx + (fy) * w);
			}
		}
		if (!keepSize)
		{
			this.InternalResize(rw, rh);
			this.Clear();
			System.Buffer.MemoryCopy((byte*) cropDataPointer, (byte*) this.Bytes.ToPointer(), rw * rh * PixelSize, rw * rh * PixelSize);
		}
		else
		{
			this.Clear();
			for (int y = 0; y < rh; y++)
			{
				for (int x = 0; x < rw; x++)
				{
					int fx = rx + x;
					int fy = ry + y;
					if (fx >= 0 && fy >= 0 && fx < w && fy < h)
						*(myPointer + fx + (fy) * w) = *(cropDataPointer + x + y * rw);
				}
			}
		}
		Marshal.FreeHGlobal(cropData);
	}

	public void Quantize(IColorQuantizer quantizer, int colorCount)
	{
		int myWidth = Width;
		int myHeight = Height;
		Color* colors = GetColors();
		quantizer.Prepare(this);
		for (int y = 0; y < myHeight; y++)
		{
			for (int x = 0; x < myWidth; x++)
			{
				var col = *(colors + x + y * myWidth);
				quantizer.AddColor(col, x, y);
			}
		}
		var palette = quantizer.GetPalette(colorCount);
		for (int y = 0; y < myHeight; y++)
		{
			for (int x = 0; x < myWidth; x++)
			{
				int index = quantizer.GetPaletteIndex(*(colors + x + y * myWidth), x, y);
				//Debug.Log(index);
				*(colors + x + y * Width) = palette[index];
			}
		}
		quantizer.Finish();
	}

	public void SetPixel(int x, int y, Color color)
	{
		Color* myPointer = (Color*) Bytes.ToPointer();
		int myIndex = x + y * Width;
		*(myPointer + myIndex) = color;
	}

	public Color GetPixel(int x, int y)
	{
		Color* myPointer = (Color*) Bytes.ToPointer();
		int myIndex = x + y * Width;
		return *(myPointer + myIndex);
	}

	public void Resize(int width, int height)
	{
		InternalResize(width, height);
	}

	~TextureBitmap()
	{
		/*Marshal.FreeHGlobal(Bytes);
		UnregisterTexture(ID);*/
	}

	public void Apply()
	{
		if (this.Texture != null)
			Graphics.ExecuteCommandBuffer(CommandBuffer);
	}

	public void Dispose()
	{
		if (Texture != null)
			GameObject.Destroy(Texture);
		Marshal.FreeHGlobal(Bytes);
		UnregisterTexture(ID);
	}

	private void InternalResize(int width, int height)
	{
		if (Width != width || Height != height)
		{
			Width = width;
			Height = height;

			Marshal.FreeHGlobal(Bytes);

			Bytes = Marshal.AllocHGlobal(Width * Height * PixelSize);
			if (this.Texture != null)
			{
				UnregisterTexture(ID);
				this.Texture.Resize(Width, Height);
				this.Texture.Apply();
				RegisterTexture(ID, Bytes);
			}
		}
	}

	public void FlipY()
	{
		int w = Width;
		int h = Height;
		Color* myPointer = (Color*) this.Bytes.ToPointer();
		for (int y = 0; y < h / 2; y++)
		{
			for (int x = 0; x < w; x++)
			{
				var bottom = *(myPointer + x + (h - 1 - y) * Width);
				*(myPointer + x + (h - 1 - y) * Width) = *(myPointer + x + y * Width);
				*(myPointer + x + y * Width) = bottom;
			}
		}
	}

	public static TextureBitmap Load(string filename, bool createBackgroundTexture = true)
	{
		return Load(System.IO.File.ReadAllBytes(filename), createBackgroundTexture);
	}

	public static TextureBitmap Load(byte[] bytes, bool createBackgroundTexture = true)
	{
		var bitmap = SkiaSharp.SKBitmap.Decode(bytes);
		if (ColorTransformers.ContainsKey(bitmap.ColorType))
		{
			var result = new TextureBitmap(bitmap.Width, bitmap.Height, createBackgroundTexture);
			var reader = ColorTransformers[bitmap.ColorType];
			int pixelSize = ColorSize[bitmap.ColorType];

			var pixels = bitmap.GetPixels();
			result.InternalResize(bitmap.Width, bitmap.Height);
			ColorTransformers[bitmap.ColorType](pixels, result.Bytes, result.Width, result.Height);
			result.FlipY();
			bitmap.Dispose();
			result.Apply();
			return result;
		}
		else
		{
			bitmap.Dispose();
			throw new ArgumentException("Image have an unsupported color type: " + bitmap.ColorType);
		}
	}

	public void LoadFrom(string filename)
	{
		this.LoadFrom(System.IO.File.ReadAllBytes(filename));
	}

	public void LoadFrom(byte[] bytes)
	{
		var bitmap = SkiaSharp.SKBitmap.Decode(bytes);
		if (ColorTransformers.ContainsKey(bitmap.ColorType))
		{
			var reader = ColorTransformers[bitmap.ColorType];
			int pixelSize = ColorSize[bitmap.ColorType];

			var pixels = bitmap.GetPixels();
			InternalResize(bitmap.Width, bitmap.Height);
			ColorTransformers[bitmap.ColorType](pixels, Bytes, Width, Height);
			FlipY();
			bitmap.Dispose();
			this.Apply();
		}
		else
		{
			bitmap.Dispose();
			throw new ArgumentException("Image have an unsupported color type: " + bitmap.ColorType);
		}
	}

	public void UltraContrast(int threshold)
	{
		int m = Width * Height;
		var ptr = GetColors();
		for (int i = 0; i < m; i++)
		{
			var col = *(ptr + i);
			var avg = (col.R + col.G + col.B) / 3;
			if (avg > threshold)
				*(ptr + i) = new Color(255, 255, 255, 255);
			else
				*(ptr + i) = new Color(0, 0, 0, 255);
		}
	}

	public void Save(string filename)
	{
		filename = System.IO.Path.GetFullPath(filename);
		if (filename.ToLowerInvariant().EndsWith(".png"))
			Save(filename, SkiaSharp.SKEncodedImageFormat.Png);
		else if (filename.ToLowerInvariant().EndsWith(".jpg") || filename.ToLowerInvariant().EndsWith(".jpeg"))
			Save(filename, SkiaSharp.SKEncodedImageFormat.Jpeg);
		else if (filename.ToLowerInvariant().EndsWith(".bmp"))
			Save(filename, SkiaSharp.SKEncodedImageFormat.Bmp);
		else if (filename.ToLowerInvariant().EndsWith(".gif"))
			Save(filename, SkiaSharp.SKEncodedImageFormat.Gif);
		else if (filename.ToLowerInvariant().EndsWith(".webp"))
			Save(filename, SkiaSharp.SKEncodedImageFormat.Webp);
	}

	public void Save(string filename, SkiaSharp.SKEncodedImageFormat format)
	{
		SkiaSharp.SKBitmap bitmap = new SkiaSharp.SKBitmap(this.Width, this.Height, false);
		int w = Width;
		int h = Height;
		var colorPtr = this.GetColors();
		for (int y = 0; y < h; y++)
		{
			for (int x = 0; x < w; x++)
			{
				var col = *(colorPtr + x + (h - 1 - y) * h);
				bitmap.SetPixel(x, y, new SkiaSharp.SKColor(col.R, col.G, col.B, col.A));
			}
		}
		var img = SkiaSharp.SKImage.FromBitmap(bitmap);
		var data = img.Encode(format, 100);
		System.IO.File.WriteAllBytes(filename, data.ToArray());
		img.Dispose();
		bitmap.Dispose();
	}
}
