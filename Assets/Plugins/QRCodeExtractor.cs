using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class QRCodeExtractor
{
	public class Result
	{

	}

	public enum Pixel
	{
		White,
		Black,
		Uncertain
	}

	private static int Scan(Pixel[] pixels, int x, int y, int imageWidth, int dirX, int dirY, int widthThreshold = 1)
	{
		int ops = 0;
		int startX = x;
		int startY = y;
		int width = 0;
		int uncertainWidth = 0;
		int phase = 0;
		int currentWidth = 0;
		int currentStartUncertain = 0;
		int currentEndUncertain = 0;
		int maxUncertainity = 4;
		bool startUncertain = true;
		(int, Pixel, Pixel)[] phases = new (int, Pixel, Pixel)[]
		{
			(1, Pixel.Black, Pixel.White),
			(1, Pixel.White, Pixel.Black),
			(3, Pixel.Black, Pixel.White),
			(1, Pixel.White, Pixel.Black),
			(1, Pixel.Black, Pixel.White)
		};
		var currentPhase = phases[phase];
		bool log = startX == 221 && startY == 293 && dirY == 1;
		while (true)
		{
			Pixel px = pixels[x + y * imageWidth];
			if (px == currentPhase.Item2) { currentWidth++; startUncertain = false; }
			if (px == Pixel.Uncertain) { if (startUncertain) currentStartUncertain++; else currentEndUncertain++; }
			if (currentStartUncertain >= maxUncertainity) return -1;
			if (currentEndUncertain >= maxUncertainity) return -1;
			if (phase > 0 && currentWidth > currentPhase.Item1 * uncertainWidth + widthThreshold) return -1;
			if (px == currentPhase.Item3) {
				if (phase == 0) { width = currentWidth; uncertainWidth = currentWidth + currentStartUncertain + currentEndUncertain; }
				else { if (currentWidth + currentStartUncertain + currentEndUncertain < width * currentPhase.Item1 - widthThreshold) return -1; }
				currentStartUncertain = currentEndUncertain;
				currentWidth = 1;
				currentEndUncertain = 0;
				phase++;
				if (phase == 5)
					return ops + 1;
				currentPhase = phases[phase];
			}

			x += dirX;
			y += dirY;

			if (x > imageWidth || x < 0 || x + y * imageWidth >= pixels.Length || y < 0)
				return -1;

			ops++;
			if (ops > 100)
				break;
		}
		return -1;
	}

	public static Bitmap ExtractQRCode(Bitmap bitmap)
	{
		System.Diagnostics.Stopwatch watch = new System.Diagnostics.Stopwatch();
		watch.Start();
		var result = new Result();

		var data = bitmap.LockBits(new Rectangle(Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);


		var brightnessThreshold = 90;
		var vibranceThreshold = 30;
		Pixel[] pixels = new Pixel[data.Width * data.Height];

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
					var vibrance = UnityEngine.Mathf.Max(r, g, b) - UnityEngine.Mathf.Min(r, g, b);
					var brightness = (r + g + b) / 3;
					// black
					if (brightness < brightnessThreshold && vibrance < vibranceThreshold)
						pixels[x + y * data.Width] = Pixel.Black;
					// white
					else if (brightness > 0xFF - brightnessThreshold && vibrance < vibranceThreshold)
						pixels[x + y * data.Width] = Pixel.White;
					else
						pixels[x + y * data.Width] = Pixel.Uncertain;
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		bool writeIntermediate = false;
		Bitmap test = null;
		if (writeIntermediate)
		{
			test = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			for (var y = 0; y < bitmap.Height; y++)
				for (var x = 0; x < bitmap.Width; x++)
					test.SetPixel(x, y,
						pixels[x + y * bitmap.Width] == Pixel.White ? Color.FromArgb(255, 255, 255, 255) :
						pixels[x + y * bitmap.Width] == Pixel.Black ? Color.FromArgb(255, 0, 0, 0) :
						Color.FromArgb(255, 120, 120, 120));
			test.Save("intermediate0.png");
			test.Dispose();

			test = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		}

		List<Color> colors = new List<Color>()
		{
			Color.FromArgb(255, 0, 0, 0),
			Color.FromArgb(255, 40, 40, 40),
			Color.FromArgb(255, 80, 80, 80),
			Color.FromArgb(255, 120, 120, 120),
			Color.FromArgb(255, 160, 160, 160),
			Color.FromArgb(255, 200, 200, 200)
		};

		List<(int, int, int)> verticalLines = new List<(int, int, int)>();
		List<(int, int, int)> horizontalLines = new List<(int, int, int)>();

		for (var x = 1; x < bitmap.Width - 1; x++)
		{
			for (var y = 1; y < bitmap.Height - 1; y++)
			{
				var px = pixels[x + y * bitmap.Width];
				var topPx = pixels[x + (y - 1) * bitmap.Width];
				var leftPx = pixels[x - 1 + y * bitmap.Width];
				if (px == Pixel.Black || px == Pixel.Uncertain)
				{
					if (topPx == Pixel.White)
					{
						int vertical = Scan(pixels, x, y, bitmap.Width, 0, 1, 0);
						if (vertical > -1) verticalLines.Add((x, y, vertical));
					}
					if (leftPx == Pixel.White)
					{
						int horizontal = Scan(pixels, x, y, bitmap.Width, 1, 0, 0);
						if (horizontal > -1) horizontalLines.Add((x, y, horizontal));
					}
				}
			}
		}

		int crossThreshold = 2;
		List<(int, int)> foundIntersections = new List<(int, int)>();
		List<(int, int, int)> stripes = new List<(int, int, int)>();

		for (int i = 0; i < verticalLines.Count; i++)
		{
			for (int j = 0; j < horizontalLines.Count; j++)
			{
				if (UnityEngine.Mathf.Abs(verticalLines[i].Item3 - horizontalLines[j].Item3) < crossThreshold)
				{
					var h1 = (horizontalLines[j].Item1, horizontalLines[j].Item2);
					var h2 = (horizontalLines[j].Item1 + horizontalLines[j].Item3, horizontalLines[j].Item2);
					var v1 = (verticalLines[i].Item1, verticalLines[i].Item2);
					var v2 = (verticalLines[i].Item1, verticalLines[i].Item2 + verticalLines[i].Item3);

					if (v1.Item1 > h1.Item1 && v2.Item1 < h2.Item1 &&  // x matches
						h1.Item2 > v1.Item2 && h2.Item2 < v2.Item2) // y matches
					{
						var intersection = (v1.Item1, h1.Item2);
						var stripeFound = false;
						for (int k = 0; k < stripes.Count; k++)
						{
							var stripe = stripes[k];
							if (stripe.Item2 == intersection.Item2)
							{
								var right = stripe.Item1 + stripe.Item3;
								if (right == intersection.Item1)
								{
									stripes[k] = (stripe.Item1, stripe.Item2, stripe.Item3 + 1);
									stripeFound = true;
									break;
								}
							}
						}
						if (!stripeFound) stripes.Add((intersection.Item1, intersection.Item2, 1));
					}
				}
			}
		}

		int ops = 0;
		List<(int, int, int, int)> markers = new List<(int, int, int, int)>();
		int minSize = 4;
		for (int i = 0; i < stripes.Count; i++)
		{
			var startStripe = i;
			if (stripes[startStripe].Item3 < minSize) continue;
			var rect = (stripes[startStripe].Item1, stripes[startStripe].Item2, stripes[startStripe].Item3, 1);
			ops = 0;
			while (true)
			{
				i++;
				if (i >= stripes.Count) break;
				if (stripes[i].Item3 < minSize) continue;

				if (stripes[i].Item2 - rect.Item4 == stripes[startStripe].Item2 &&
					stripes[i].Item1 == stripes[startStripe].Item1 &&
					stripes[i].Item3 == rect.Item3)
					rect.Item4++;
				else
				{
					i--;
					break;
				}
				ops++;
				if (ops > 50) break;
			}
			markers.Add(rect);
		}
		if (writeIntermediate)
		{
			for (var i = 0; i < markers.Count; i++)
			{
				for (var y = markers[i].Item2; y < markers[i].Item2 + markers[i].Item4; y++)
				{
					for (var x = markers[i].Item1; x < markers[i].Item1 + markers[i].Item3; x++)
					{
						test.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
					}
				}
			}

			test.Save("intermediate2.png");
			test.Dispose();

			test = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		}

		List<(int, int, int, int)> finalMarkers = new List<(int, int, int, int)>();
		for (var i = 0; i < markers.Count; i++)
		{
			var left = markers[i].Item1;
			var top = markers[i].Item2;
			var right = left + markers[i].Item3;
			var bottom = top + markers[i].Item4;
			var leftWhitePassed = false;
			var rightWhitePassed = false;
			var topWhitePassed = false;
			var bottomWhitePassed = false;
			int m = UnityEngine.Mathf.Max(markers[i].Item4, markers[i].Item3) + 1;
			int finalLeft = -1;
			int finalRight = -1;
			int finalTop = -1;
			int finalBottom = -1;
			for (int o = 1; o <= m; o++)
			{
				if (finalLeft == -1)
				{
					if (!leftWhitePassed && pixels[left - o + top * bitmap.Width] == Pixel.Black) leftWhitePassed = true;
					else if (leftWhitePassed && pixels[left - o + top * bitmap.Width] == Pixel.White) finalLeft = left - o + 1;
				}
				if (finalRight == -1)
				{
					if (!rightWhitePassed && pixels[right + o + top * bitmap.Width] == Pixel.Black) rightWhitePassed = true;
					else if (rightWhitePassed && pixels[right + o + top * bitmap.Width] == Pixel.White) finalRight = right + o;
				}
				if (finalTop == -1)
				{
					if (!topWhitePassed && pixels[left + (top - o) * bitmap.Width] == Pixel.Black) topWhitePassed = true;
					else if (topWhitePassed && pixels[left + (top - o) * bitmap.Width] == Pixel.White) finalTop = top - o + 1;
				}
				if (finalBottom == -1)
				{
					if (!bottomWhitePassed && pixels[left + (bottom + o) * bitmap.Width] == Pixel.Black) bottomWhitePassed = true;
					else if (bottomWhitePassed && pixels[left + (bottom + o) * bitmap.Width] == Pixel.White) finalBottom = bottom + o;
				}
			}
			finalMarkers.Add((finalLeft, finalTop, finalRight - finalLeft, finalBottom - finalTop));
		}
		if (writeIntermediate)
		{
			for (var i = 0; i < finalMarkers.Count; i++)
			{
				for (var y = finalMarkers[i].Item2; y < finalMarkers[i].Item2 + finalMarkers[i].Item4; y++)
				{
					for (var x = finalMarkers[i].Item1; x < finalMarkers[i].Item1 + finalMarkers[i].Item3; x++)
					{
						test.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
					}
				}
			}

			test.Save("intermediate3.png");
			test.Dispose();

			test = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		}

		int sizeThreshold = 1;

		List<int> found = new List<int>();
		List<(int, int, int)> qrCodes = new List<(int, int, int)>();
		int maxHeight = 0;
		int finalWidth = 0;
		for (int i = 0; i < finalMarkers.Count - 1; i++)
		{
			if (found.Contains(i)) continue;
			int rightMarker = -1;
			int bottomMarker = -1;
			for (int j = i + 1; j < finalMarkers.Count; j++)
			{
				if (UnityEngine.Mathf.Abs(finalMarkers[i].Item2 - finalMarkers[j].Item2) <= sizeThreshold &&
					UnityEngine.Mathf.Abs(finalMarkers[i].Item4 - finalMarkers[j].Item4) <= sizeThreshold &&
					finalMarkers[i].Item1 < finalMarkers[j].Item1 &&
					(rightMarker == -1 || finalMarkers[rightMarker].Item1 > finalMarkers[j].Item1))
				{
					rightMarker = j;
				}

				if (UnityEngine.Mathf.Abs(finalMarkers[i].Item1 - finalMarkers[j].Item1) <= sizeThreshold &&
					UnityEngine.Mathf.Abs(finalMarkers[i].Item3 - finalMarkers[j].Item3) <= sizeThreshold &&
					finalMarkers[i].Item2 < finalMarkers[j].Item2 &&
					(bottomMarker == -1 || finalMarkers[bottomMarker].Item2 > finalMarkers[j].Item2))
				{
					bottomMarker = j;
				}
			}

			var distanceX = (finalMarkers[rightMarker].Item1 + finalMarkers[rightMarker].Item3) - finalMarkers[i].Item1;
			var distanceY = (finalMarkers[bottomMarker].Item2 + finalMarkers[bottomMarker].Item4) - finalMarkers[i].Item2;
			if (UnityEngine.Mathf.Abs(distanceX - distanceY) <= sizeThreshold)
			{
				if ((distanceY * 1.2f) > maxHeight)
					maxHeight = (int) (distanceY * 1.2f);
				finalWidth += (int) (distanceX * 1.2f);
				found.Add(i);
				found.Add(rightMarker);
				found.Add(bottomMarker);
				qrCodes.Add((i, rightMarker, bottomMarker));
			}
		}

		if (writeIntermediate)
		{
			for (var i = 0; i < qrCodes.Count; i++)
			{
				var left = finalMarkers[qrCodes[i].Item1].Item1;
				var top = finalMarkers[qrCodes[i].Item1].Item2;
				var right = finalMarkers[qrCodes[i].Item2].Item1 + finalMarkers[qrCodes[i].Item2].Item3;
				var bottom = finalMarkers[qrCodes[i].Item3].Item2 + finalMarkers[qrCodes[i].Item3].Item4;
				for (var y = top; y < bottom; y++)
				{
					for (var x = left; x < right; x++)
					{
						test.SetPixel(x, y, Color.FromArgb(255, 200, 200, 200));
					}
				}
			}

			for (var i = 0; i < finalMarkers.Count; i++)
			{
				for (var y = finalMarkers[i].Item2; y < finalMarkers[i].Item2 + finalMarkers[i].Item4; y++)
				{
					for (var x = finalMarkers[i].Item1; x < finalMarkers[i].Item1 + finalMarkers[i].Item3; x++)
					{
						test.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));
					}
				}
			}
			test.Save("intermediate4.png");
			test.Dispose();

			test = new Bitmap(bitmap.Width, bitmap.Height, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		}

		var resultImage = new Bitmap(finalWidth, maxHeight, System.Drawing.Imaging.PixelFormat.Format24bppRgb);
		for (int x = 0; x < finalWidth; x++)
			for (int y = 0; y < maxHeight; y++)
				resultImage.SetPixel(x, y, Color.FromArgb(255, 255, 255, 255));

		var horizontalOffset = 0;
		
		using (Graphics graphics = Graphics.FromImage(resultImage))
		{
			for (int i = 0; i < qrCodes.Count; i++)
			{
				var left = finalMarkers[qrCodes[i].Item1].Item1;
				var top = finalMarkers[qrCodes[i].Item1].Item2;
				var right = finalMarkers[qrCodes[i].Item2].Item1 + finalMarkers[qrCodes[i].Item2].Item3;
				var bottom = finalMarkers[qrCodes[i].Item3].Item2 + finalMarkers[qrCodes[i].Item3].Item4;

				var width = right - left;
				var height = bottom - top;

				var horizontalPadding = (int) (width * 0.1f);
				var verticalPadding = (int) (height * 0.1f);

				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				graphics.DrawImage(bitmap, new Rectangle(horizontalOffset + horizontalPadding, verticalPadding, width, height), new Rectangle(left, top, width, height), GraphicsUnit.Pixel);
				horizontalOffset += width + horizontalPadding * 2;
			}
		}

		resultImage.Save("intermediate5.png");

		watch.Stop();
		UnityEngine.Debug.Log(watch.ElapsedMilliseconds);
		return resultImage;
	}
}
