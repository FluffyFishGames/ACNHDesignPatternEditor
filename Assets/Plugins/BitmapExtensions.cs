using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public static class BitmapExtensions
{
	public static System.Drawing.Color Interpolate(this Bitmap bitmap, float x, float y)
	{
		int left = Mathf.FloorToInt(x);
		int top = Mathf.FloorToInt(y);
		float fx = x - left;
		float fy = y - top;
		var topLeft = bitmap.GetPixel(left, top);
		var topRight = bitmap.GetPixel(left + 1, top);
		var bottomLeft = bitmap.GetPixel(left, top + 1);
		var bottomRight = bitmap.GetPixel(left + 1, top + 1);

		float topR = topLeft.R * fx + topRight.R * (1f - fx);
		float topG = topLeft.G * fx + topRight.G * (1f - fx);
		float topB = topLeft.B * fx + topRight.B * (1f - fx);

		float bottomR = bottomLeft.R * fx + bottomRight.R * (1f - fx);
		float bottomG = bottomLeft.G * fx + bottomRight.G * (1f - fx);
		float bottomB = bottomLeft.B * fx + bottomRight.B * (1f - fx);

		float finalR = topR * fy + bottomR * (1f - fy);
		float finalG = topR * fy + bottomR * (1f - fy);
		float finalB = topR * fy + bottomR * (1f - fy);

		return System.Drawing.Color.FromArgb(255, (int) finalR, (int) finalG, (int) finalB);
	}

	public static void Background(this Bitmap bitmap, System.Drawing.Color backgroundColor)
	{
		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					if (*(ptr + index + 3) == 0)
					{
						(*(ptr + index + 2)) = backgroundColor.R;
						(*(ptr + index + 1)) = backgroundColor.G;
						(*(ptr + index + 0)) = backgroundColor.B;
						(*(ptr + index + 3)) = backgroundColor.A;
					}
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);
	}

	public static void UltraContrast(this Bitmap bitmap, int threshold = 128)
	{
		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					int mid = ((*(ptr + index + 2)) + (*(ptr + index + 1)) + (*(ptr + index + 0))) / 3;
					if (mid > threshold)
					{ 
						(*(ptr + index + 2)) = 255;
						(*(ptr + index + 1)) = 255;
						(*(ptr + index + 0)) = 255;
						(*(ptr + index + 3)) = 255;
					}
					else
					{
						(*(ptr + index + 2)) = 0;
						(*(ptr + index + 1)) = 0;
						(*(ptr + index + 0)) = 0;
						(*(ptr + index + 3)) = 255;
					}
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);
	}


	public static void RemoveColorful(this Bitmap bitmap, int threshold = 16)
	{
		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					int r = ((*(ptr + index + 2)));
					int g = ((*(ptr + index + 1)));
					int b = ((*(ptr + index + 0)));
					var vibrance = Mathf.Max(r, g, b) - Mathf.Min(r, g, b);
					if (vibrance > threshold)
					{
						(*(ptr + index + 2)) = 255;
						(*(ptr + index + 1)) = 255;
						(*(ptr + index + 0)) = 255;
						(*(ptr + index + 3)) = 255;
					}
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);
	}

	public static Texture2D ToTexture2D(this Bitmap bitmap, Texture2D texture)
	{
		if (texture == null)
			texture = new Texture2D(bitmap.Width, bitmap.Height, bitmap.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? TextureFormat.ARGB32 : TextureFormat.RGB24, false);
		else if (texture.width != bitmap.Width || texture.height != bitmap.Height)
			texture.Resize(bitmap.Width, bitmap.Height);

		UnityEngine.Color32[] colors = new UnityEngine.Color32[texture.width * texture.height];

		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					colors[x + (data.Height - 1 - y) * data.Width] = new Color32(*(ptr + index + 2), *(ptr + index + 1), *(ptr + index + 0), (byte) (*(ptr + index + 3) * 1f));
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		texture.SetPixels32(colors);
		texture.Apply();
		return texture;
	}

	public static byte[] GetBytes(this Bitmap bitmap)
	{
		byte[] colors = new byte[bitmap.Width * bitmap.Height * 4];

		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					colors[(x + y * data.Width) * 4 + 0] = *(ptr + index + 2);
					colors[(x + y * data.Width) * 4 + 1] = *(ptr + index + 1);
					colors[(x + y * data.Width) * 4 + 2] = *(ptr + index + 0);
					colors[(x + y * data.Width) * 4 + 3] = *(ptr + index + 3);
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		return colors;
	}


	public static void FromBytes(this Bitmap bitmap, byte[] colors)
	{
		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					*(ptr + index + 2) = colors[(x + y * data.Width) * 4 + 0];
					*(ptr + index + 1) = colors[(x + y * data.Width) * 4 + 1];
					*(ptr + index + 0) = colors[(x + y * data.Width) * 4 + 2];
					*(ptr + index + 3) = colors[(x + y * data.Width) * 4 + 3];
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);
	}

	public static UnityEngine.Color[] ToColors(this Bitmap bitmap)
	{
		UnityEngine.Color[] colors = new UnityEngine.Color[bitmap.Width * bitmap.Height];

		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					colors[x + y * data.Width] = new UnityEngine.Color(
						*(ptr + index + 2) / 255f, 
						*(ptr + index + 1) / 255f, 
						*(ptr + index + 0) / 255f, 
						*(ptr + index + 3) / 255f);
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		return colors;
	}

	class ConvolutionMatrix
	{
		public ConvolutionMatrix()
		{
			Pixel = 1;
			Factor = 1;
		}

		public void Apply(int Val)
		{
			TopLeft = TopMid = TopRight = MidLeft = MidRight = BottomLeft = BottomMid = BottomRight = Pixel = Val;
		}

		public int TopLeft { get; set; }

		public int TopMid { get; set; }

		public int TopRight { get; set; }

		public int MidLeft { get; set; }

		public int MidRight { get; set; }

		public int BottomLeft { get; set; }

		public int BottomMid { get; set; }

		public int BottomRight { get; set; }

		public int Pixel { get; set; }

		public int Factor { get; set; }

		public int Offset { get; set; }
	}

	class Convolution
	{
		public void Convolution3x3(ref Bitmap bmp)
		{
			int Factor = Matrix.Factor;

			if (Factor == 0) return;

			int TopLeft = Matrix.TopLeft;
			int TopMid = Matrix.TopMid;
			int TopRight = Matrix.TopRight;
			int MidLeft = Matrix.MidLeft;
			int MidRight = Matrix.MidRight;
			int BottomLeft = Matrix.BottomLeft;
			int BottomMid = Matrix.BottomMid;
			int BottomRight = Matrix.BottomRight;
			int Pixel = Matrix.Pixel;
			int Offset = Matrix.Offset;

			Bitmap TempBmp = (Bitmap) bmp.Clone();

			BitmapData bmpData = bmp.LockBits(new Rectangle(0, 0, bmp.Width, bmp.Height), ImageLockMode.ReadWrite, PixelFormat.Format24bppRgb);
			BitmapData TempBmpData = TempBmp.LockBits(new Rectangle(0, 0, TempBmp.Width, TempBmp.Height), ImageLockMode.ReadOnly, PixelFormat.Format24bppRgb);

			unsafe
			{
				byte* ptr = (byte*) bmpData.Scan0.ToPointer();
				byte* TempPtr = (byte*) TempBmpData.Scan0.ToPointer();

				int Pix = 0;
				int Stride = bmpData.Stride;
				int DoubleStride = Stride * 2;
				int Width = bmp.Width - 2;
				int Height = bmp.Height - 2;
				int stopAddress = (int) ptr + bmpData.Stride * bmpData.Height;

				for (int y = 0; y < Height; ++y)
					for (int x = 0; x < Width; ++x)
					{
						Pix = (((((TempPtr[2] * TopLeft) + (TempPtr[5] * TopMid) + (TempPtr[8] * TopRight)) +
						  ((TempPtr[2 + Stride] * MidLeft) + (TempPtr[5 + Stride] * Pixel) + (TempPtr[8 + Stride] * MidRight)) +
						  ((TempPtr[2 + DoubleStride] * BottomLeft) + (TempPtr[5 + DoubleStride] * BottomMid) + (TempPtr[8 + DoubleStride] * BottomRight))) / Factor) + Offset);

						if (Pix < 0) Pix = 0;
						else if (Pix > 255) Pix = 255;

						ptr[5 + Stride] = (byte) Pix;

						Pix = (((((TempPtr[1] * TopLeft) + (TempPtr[4] * TopMid) + (TempPtr[7] * TopRight)) +
							  ((TempPtr[1 + Stride] * MidLeft) + (TempPtr[4 + Stride] * Pixel) + (TempPtr[7 + Stride] * MidRight)) +
							  ((TempPtr[1 + DoubleStride] * BottomLeft) + (TempPtr[4 + DoubleStride] * BottomMid) + (TempPtr[7 + DoubleStride] * BottomRight))) / Factor) + Offset);

						if (Pix < 0) Pix = 0;
						else if (Pix > 255) Pix = 255;

						ptr[4 + Stride] = (byte) Pix;

						Pix = (((((TempPtr[0] * TopLeft) + (TempPtr[3] * TopMid) + (TempPtr[6] * TopRight)) +
							  ((TempPtr[0 + Stride] * MidLeft) + (TempPtr[3 + Stride] * Pixel) + (TempPtr[6 + Stride] * MidRight)) +
							  ((TempPtr[0 + DoubleStride] * BottomLeft) + (TempPtr[3 + DoubleStride] * BottomMid) + (TempPtr[6 + DoubleStride] * BottomRight))) / Factor) + Offset);

						if (Pix < 0) Pix = 0;
						else if (Pix > 255) Pix = 255;

						ptr[3 + Stride] = (byte) Pix;

						ptr += 3;
						TempPtr += 3;
					}
			}

			bmp.UnlockBits(bmpData);
			TempBmp.UnlockBits(TempBmpData);
		}

		public ConvolutionMatrix Matrix { get; set; }
	}

	public static void ApplySharpen(this Bitmap bmp, int weight)
	{
		ConvolutionMatrix m = new ConvolutionMatrix();
		m.Apply(0);
		m.Pixel = weight;
		m.TopMid = m.MidLeft = m.MidRight = m.BottomMid = -2;
		m.Factor = weight - 8;

		Convolution C = new Convolution();
		C.Matrix = m;
		C.Convolution3x3(ref bmp);
	}

	public static Bitmap MedianFilter(this Bitmap sourceBitmap,
									   int matrixSize)
	{
		BitmapData sourceData =
				   sourceBitmap.LockBits(new Rectangle(0, 0,
				   sourceBitmap.Width, sourceBitmap.Height),
				   ImageLockMode.ReadOnly,
				   PixelFormat.Format32bppArgb);


		byte[] pixelBuffer = new byte[sourceData.Stride *
									  sourceData.Height];


		byte[] resultBuffer = new byte[sourceData.Stride *
									   sourceData.Height];


		Marshal.Copy(sourceData.Scan0, pixelBuffer, 0,
								   pixelBuffer.Length);


		sourceBitmap.UnlockBits(sourceData);


		int filterOffset = (matrixSize - 1) / 2;
		int calcOffset = 0;


		int byteOffset = 0;


		List<int> neighbourPixels = new List<int>();
		byte[] middlePixel;


		for (int offsetY = filterOffset; offsetY <
			sourceBitmap.Height - filterOffset; offsetY++)
		{
			for (int offsetX = filterOffset; offsetX <
				sourceBitmap.Width - filterOffset; offsetX++)
			{
				byteOffset = offsetY *
							 sourceData.Stride +
							 offsetX * 4;


				neighbourPixels.Clear();


				for (int filterY = -filterOffset;
					filterY <= filterOffset; filterY++)
				{
					for (int filterX = -filterOffset;
						filterX <= filterOffset; filterX++)
					{


						calcOffset = byteOffset +
									 (filterX * 4) +
									 (filterY * sourceData.Stride);


						neighbourPixels.Add(BitConverter.ToInt32(
										 pixelBuffer, calcOffset));
					}
				}


				neighbourPixels.Sort();

				middlePixel = BitConverter.GetBytes(
								   neighbourPixels[filterOffset]);


				resultBuffer[byteOffset] = middlePixel[0];
				resultBuffer[byteOffset + 1] = middlePixel[1];
				resultBuffer[byteOffset + 2] = middlePixel[2];
				resultBuffer[byteOffset + 3] = middlePixel[3];
			}
		}


		Bitmap resultBitmap = new Bitmap(sourceBitmap.Width,
										 sourceBitmap.Height);


		BitmapData resultData =
				   resultBitmap.LockBits(new Rectangle(0, 0,
				   resultBitmap.Width, resultBitmap.Height),
				   ImageLockMode.WriteOnly,
				   PixelFormat.Format32bppArgb);


		Marshal.Copy(resultBuffer, 0, resultData.Scan0,
								   resultBuffer.Length);


		resultBitmap.UnlockBits(resultData);


		return resultBitmap;
	}

	public enum SharpenType {
		Sharpen7To1,
		Sharpen9To1,
		Sharpen12To1,
		Sharpen24To1,
		Sharpen48To1,
		Sharpen5To4,
		Sharpen10To8,
		Sharpen11To8,
		Sharpen821
	}
	public static Bitmap SharpenEdgeDetect(this Bitmap sourceBitmap,
												SharpenType sharpen,
													   int bias = 0,
											 bool grayscale = false,
												  bool mono = false,
										   int medianFilterSize = 0)
	{
		Bitmap resultBitmap = null;


		if (medianFilterSize == 0)
		{
			resultBitmap = sourceBitmap;
		}
		else
		{
			resultBitmap =
			sourceBitmap.MedianFilter(medianFilterSize);
		}


		switch (sharpen)
		{
			case SharpenType.Sharpen7To1:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							 Matrix.Sharpen7To1,
							 1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen9To1:
			{
				resultBitmap =
					resultBitmap.SharpenEdgeDetect(
								Matrix.Sharpen9To1,
								1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen12To1:
			{
				resultBitmap =
					resultBitmap.SharpenEdgeDetect(
								Matrix.Sharpen12To1,
								1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen24To1:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen24To1,
							1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen48To1:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen48To1,
							1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen5To4:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen5To4,
							1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen10To8:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen10To8,
							1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen11To8:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen11To8,
							3.0 / 1.0, bias, grayscale, mono);
			}
			break;
			case SharpenType.Sharpen821:
			{
				resultBitmap =
				resultBitmap.SharpenEdgeDetect(
							Matrix.Sharpen821,
							8.0 / 1.0, bias, grayscale, mono);
			}
			break;
		}


		return resultBitmap;
	}

	public static class Matrix
	{
		public static double[,] Sharpen7To1
		{
			get
			{
				return new double[,]
				{  { 1,  1,  1, },
			   { 1, -7,  1, },
			   { 1,  1,  1, }, };
			}
		}


		public static double[,] Sharpen9To1
		{
			get
			{
				return new double[,]
				{  { -1, -1, -1, },
			   { -1,  9, -1, },
			   { -1, -1, -1, }, };
			}
		}


		public static double[,] Sharpen12To1
		{
			get
			{
				return new double[,]
				{  { -1, -1, -1, },
			   { -1, 12, -1, },
			   { -1, -1, -1, }, };
			}
		}


		public static double[,] Sharpen24To1
		{
			get
			{
				return new double[,]
				{  { -1, -1, -1, -1, -1, },
			   { -1, -1, -1, -1, -1, },
			   { -1, -1, 24, -1, -1, },
			   { -1, -1, -1, -1, -1, },
			   { -1, -1, -1, -1, -1, }, };
			}
		}


		public static double[,] Sharpen48To1
		{
			get
			{
				return new double[,]
				{  {  -1, -1, -1, -1, -1, -1, -1, },
			   {  -1, -1, -1, -1, -1, -1, -1, },
			   {  -1, -1, -1, -1, -1, -1, -1, },
			   {  -1, -1, -1, 48, -1, -1, -1, },
			   {  -1, -1, -1, -1, -1, -1, -1, },
			   {  -1, -1, -1, -1, -1, -1, -1, },
			   {  -1, -1, -1, -1, -1, -1, -1, }, };
			}
		}


		public static double[,] Sharpen5To4
		{
			get
			{
				return new double[,]
				{  {  0, -1,  0, },
			   { -1,  5, -1, },
			   {  0, -1,  0, }, };
			}
		}


		public static double[,] Sharpen10To8
		{
			get
			{
				return new double[,]
				{  {  0, -2,  0, },
			   { -2, 10, -2, },
			   {  0, -2,  0, }, };
			}
		}


		public static double[,] Sharpen11To8
		{
			get
			{
				return new double[,]
				{  {  0, -2,  0, },
			   { -2, 11, -2, },
			   {  0, -2,  0, }, };
			}
		}


		public static double[,] Sharpen821
		{
			get
			{
				return new double[,]
				{  {  -1, -1, -1, -1, -1, },
			   {  -1,  2,  2,  2, -1, },
			   {  -1,  2,  8,  2,  1, },
			   {  -1,  2,  2,  2, -1, },
			   {  -1, -1, -1, -1, -1, }, };
			}
		}
	}

	private static Bitmap SharpenEdgeDetect(this Bitmap sourceBitmap,
											  double[,] filterMatrix,
												   double factor = 1,
														int bias = 0,
											  bool grayscale = false,
												   bool mono = false)
	{
		BitmapData sourceData = sourceBitmap.LockBits(new Rectangle(0, 0,
								 sourceBitmap.Width, sourceBitmap.Height),
												   ImageLockMode.ReadOnly,
											 PixelFormat.Format32bppArgb);


		byte[] pixelBuffer = new byte[sourceData.Stride * sourceData.Height];
		byte[] resultBuffer = new byte[sourceData.Stride * sourceData.Height];


		Marshal.Copy(sourceData.Scan0, pixelBuffer, 0, pixelBuffer.Length);
		sourceBitmap.UnlockBits(sourceData);


		if (grayscale == true)
		{
			for (int pixel = 0; pixel < pixelBuffer.Length; pixel += 4)
			{
				pixelBuffer[pixel] =
					(byte) (pixelBuffer[pixel] * 0.11f);


				pixelBuffer[pixel + 1] =
					(byte) (pixelBuffer[pixel + 1] * 0.59f);


				pixelBuffer[pixel + 2] =
					(byte) (pixelBuffer[pixel + 2] * 0.3f);
			}
		}


		double blue = 0.0;
		double green = 0.0;
		double red = 0.0;


		int filterWidth = filterMatrix.GetLength(1);
		int filterHeight = filterMatrix.GetLength(0);


		int filterOffset = (filterWidth - 1) / 2;
		int calcOffset = 0;


		int byteOffset = 0;


		for (int offsetY = filterOffset; offsetY <
			sourceBitmap.Height - filterOffset; offsetY++)
		{
			for (int offsetX = filterOffset; offsetX <
				sourceBitmap.Width - filterOffset; offsetX++)
			{
				blue = 0;
				green = 0;
				red = 0;


				byteOffset = offsetY *
							 sourceData.Stride +
							 offsetX * 4;


				for (int filterY = -filterOffset;
					filterY <= filterOffset; filterY++)
				{
					for (int filterX = -filterOffset;
						filterX <= filterOffset; filterX++)
					{
						calcOffset = byteOffset +
									 (filterX * 4) +
									 (filterY * sourceData.Stride);


						blue += (double) (pixelBuffer[calcOffset]) *
								filterMatrix[filterY + filterOffset,
													filterX + filterOffset];


						green += (double) (pixelBuffer[calcOffset + 1]) *
								 filterMatrix[filterY + filterOffset,
													filterX + filterOffset];


						red += (double) (pixelBuffer[calcOffset + 2]) *
							   filterMatrix[filterY + filterOffset,
												  filterX + filterOffset];
					}
				}


				if (mono == true)
				{
					blue = resultBuffer[byteOffset] - factor * blue;
					green = resultBuffer[byteOffset + 1] - factor * green;
					red = resultBuffer[byteOffset + 2] - factor * red;


					blue = (blue > bias ? 255 : 0);


					green = (blue > bias ? 255 : 0);


					red = (blue > bias ? 255 : 0);
				}
				else
				{
					blue = resultBuffer[byteOffset] -
						   factor * blue + bias;


					green = resultBuffer[byteOffset + 1] -
							factor * green + bias;


					red = resultBuffer[byteOffset + 2] -
						  factor * red + bias;


					blue = (blue > 255 ? 255 :
						   (blue < 0 ? 0 :
							blue));


					green = (green > 255 ? 255 :
							(green < 0 ? 0 :
							 green));


					red = (red > 255 ? 255 :
						  (red < 0 ? 0 :
						   red));
				}


				resultBuffer[byteOffset] = (byte) (blue);
				resultBuffer[byteOffset + 1] = (byte) (green);
				resultBuffer[byteOffset + 2] = (byte) (red);
				resultBuffer[byteOffset + 3] = 255;
			}
		}


		Bitmap resultBitmap = new Bitmap(sourceBitmap.Width, sourceBitmap.Height);

		BitmapData resultData = resultBitmap.LockBits(new Rectangle(0, 0,
								 resultBitmap.Width, resultBitmap.Height),
												  ImageLockMode.WriteOnly,
											 PixelFormat.Format32bppArgb);


		Marshal.Copy(resultBuffer, 0, resultData.Scan0, resultBuffer.Length);
		resultBitmap.UnlockBits(resultData);


		return resultBitmap;
	}
}
