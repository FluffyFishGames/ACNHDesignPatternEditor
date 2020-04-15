using MyHorizons.Data;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class ACNHDesignExtractor
{
	public static (int, int, int, int) FindPattern(System.Drawing.Bitmap bitmap, DesignPattern.TypeEnum type)
	{
		bool isPro = type != DesignPattern.TypeEnum.SimplePattern;
		var colors = bitmap.ToColors();
		bool[] bw = new bool[bitmap.Width * bitmap.Height]; // true = black
		int min = 180;
		int max = 235;
		for (int x = 0; x < bitmap.Width; x++)
		{
			for (int y = 0; y < bitmap.Height; y++)
			{
				float h;
				float s;
				float v;
				int r = (int) (colors[x + y * bitmap.Width].r * 255f);
				int g = (int) (colors[x + y * bitmap.Width].g * 255f);
				int b = (int) (colors[x + y * bitmap.Width].b * 255f);
				r = r * (r >= min && r <= max ? 0 : 1);
				g = g * (g >= min && g <= max ? 0 : 1);
				b = b * (b >= min && b <= max ? 0 : 1);
				bw[x + y * bitmap.Width] = r == 0 && g == 0;
			}
		}

		int widthThreshold = 5;
		int minWidth = 2;
		List<(int, int, int)> horizontalLines = new List<(int, int, int)>();
		List<(int, int, int)> verticalLines = new List<(int, int, int)>();

		for (int y = 0; y < bitmap.Height; y++)
		{
			int currentDashWidth = 0;
			int currentSpaceWidth = 0;
			int dashes = 0;
			int dashWidth = 0;
			int start = 0;
			bool black = false;

			for (int x = 0; x < bitmap.Width; x++)
			{
				int idx = x + y * bitmap.Width;
				if (bw[idx])
				{
					if (!black)
					{
						currentDashWidth = 0;
						if (dashes > 0 && currentSpaceWidth > dashWidth) { dashes = 0; dashWidth = 0; }
						if (dashes == 0)
							start = x;
					}
					black = true;
					currentDashWidth++;
				}
				else
				{
					if (dashes == 0 && currentDashWidth >= minWidth)
						dashWidth = currentDashWidth;

					if (dashWidth > 0)
					{
						if (black)
						{
							currentSpaceWidth = 0;
							if (currentDashWidth < dashWidth - widthThreshold || currentDashWidth < minWidth || currentDashWidth > dashWidth + widthThreshold) { dashes = 0; dashWidth = 0; }
							else
							{
								dashes++;
								if (dashes >= 14) 
								{
									horizontalLines.Add((start, y, x));
									dashWidth = 0; 
									dashes = 0; 
									continue; 
								}
							}
						}
						if (dashes > 0) currentSpaceWidth++;
					}
					black = false;
				}
			}
		}

		for (int x = 0; x < bitmap.Width; x++)
		{
			int currentDashWidth = 0;
			int currentSpaceWidth = 0;
			int dashes = 0;
			int dashWidth = 0;
			int start = 0;
			bool black = false;

			for (int y = 0; y < bitmap.Height; y++)
			{
				int idx = x + y * bitmap.Width;
				if (bw[idx])
				{
					if (!black)
					{
						currentDashWidth = 0;
						if (dashes > 0 && currentSpaceWidth > dashWidth) { dashes = 0; dashWidth = 0; }
						if (dashes == 0)
							start = y;
					}
					black = true;
					currentDashWidth++;
				}
				else
				{
					if (dashes == 0 && currentDashWidth >= minWidth)
						dashWidth = currentDashWidth;

					if (dashWidth > 0)
					{
						if (black)
						{
							currentSpaceWidth = 0;
							if (currentDashWidth < dashWidth - widthThreshold || currentDashWidth < minWidth || currentDashWidth > dashWidth + widthThreshold) { dashes = 0; dashWidth = 0; }
							else
							{
								dashes++;
								if (dashes >= 14)
								{
									verticalLines.Add((x, start, y));
									dashWidth = 0;
									dashes = 0;
									continue;
								}
							}
						}
						if (dashes > 0) currentSpaceWidth++;
					}
					black = false;
				}
			}
		}

		UnityEngine.Debug.Log("VERTICAL: " + verticalLines.Count);
		UnityEngine.Debug.Log("HORIZONTAL: " + horizontalLines.Count);
		int sizeThreshold = 10;

		// find pairing horizontal lines
		List<List<int>> horizontalGroups = new List<List<int>>();
		List<int> foundHorizontal = new List<int>();
		for (int i = 0; i < horizontalLines.Count - 1; i++)
		{
			if (!foundHorizontal.Contains(i))
			{
				List<int> group = new List<int>();
				group.Add(i);
				for (int j = i + 1; j < horizontalLines.Count; j++)
				{
					if (UnityEngine.Mathf.Abs(horizontalLines[i].Item1 - horizontalLines[j].Item1) < sizeThreshold &&
						UnityEngine.Mathf.Abs(horizontalLines[i].Item3 - horizontalLines[j].Item3) < sizeThreshold)
					{
						group.Add(j);
						foundHorizontal.Add(j);
					}
				}
				horizontalGroups.Add(group);
			}
		}

		// find pairing vertical lines
		List<List<int>> verticalGroups = new List<List<int>>();
		List<int> foundVertical = new List<int>();
		for (int i = 0; i < verticalLines.Count - 1; i++)
		{
			if (!foundVertical.Contains(i))
			{
				List<int> group = new List<int>();
				group.Add(i);
				for (int j = i + 1; j < verticalLines.Count; j++)
				{
					if (UnityEngine.Mathf.Abs(verticalLines[i].Item2 - verticalLines[j].Item2) < sizeThreshold &&
						UnityEngine.Mathf.Abs(verticalLines[i].Item3 - verticalLines[j].Item3) < sizeThreshold)
					{
						group.Add(j);
						foundVertical.Add(j);
					}
				}
				verticalGroups.Add(group);
			}
		}
		UnityEngine.Debug.Log("VERTICAL: " + verticalGroups.Count);
		UnityEngine.Debug.Log("HORIZONTAL: " + horizontalGroups.Count);

		(int, int) whiteR = (235, 255);
		(int, int) whiteG = (235, 255);
		(int, int) whiteB = (200, 255);
		List<(int, int, int, int, bool[])> rects = new List<(int, int, int, int, bool[])>();
		for (int i = 0; i < verticalGroups.Count; i++)
		{
			var vTop = verticalLines[verticalGroups[i][0]].Item2;
			var vBottom = verticalLines[verticalGroups[i][0]].Item3;
			var vX = verticalLines[verticalGroups[i][0]].Item1;

			for (int j = 0; j < horizontalGroups.Count; j++)
			{
				var hLeft = horizontalLines[horizontalGroups[j][0]].Item1;
				var hRight = horizontalLines[horizontalGroups[j][0]].Item3;
				var hY = horizontalLines[horizontalGroups[j][0]].Item2;

				UnityEngine.Debug.Log(vTop + "-" + vBottom + "(" + vX + ") " + hLeft + "-" + hRight + "(" + hY + ")");
				if ((UnityEngine.Mathf.Abs(hY - vTop) < sizeThreshold ||
					UnityEngine.Mathf.Abs(hY - vBottom) < sizeThreshold) &&
					(UnityEngine.Mathf.Abs(vX - hLeft) < sizeThreshold ||
					UnityEngine.Mathf.Abs(vX - hRight) < sizeThreshold))
				{
					// its a rect!
					int w = hRight - hLeft;
					int h = vBottom - vTop;
					bool[] rect = new bool[w * h];

					// find outer top
					int outerTop = -1;
					for (int y = -30; y < 0; y++)
					{
						int r = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmap.Width].r * 255f);
						int g = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmap.Width].g * 255f);
						int b = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmap.Width].b * 255f);
						if (r >= 240 && g >= 240 && b >= 220)
						{
							outerTop = vTop + y;
							break;
						}
					}
					// find outer left
					int outerLeft = -1;
					for (int x = -30; x < 0; x++)
					{
						int r = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmap.Width].r * 255f);
						int g = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmap.Width].g * 255f);
						int b = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmap.Width].b * 255f);
						if (r >= 240 && g >= 240 && b >= 220)
						{
							outerLeft = hLeft + x;
							break;
						}
					}
					// find outer bottom
					int outerBottom = -1;
					for (int y = 29; y >= 0; y--)
					{
						int r = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmap.Width].r * 255f);
						int g = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmap.Width].g * 255f);
						int b = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmap.Width].b * 255f);
						if (r >= 240 && g >= 240 && b >= 220)
						{
							outerBottom = vBottom + y + 1;
							break;
						}
					}
					// find outer right
					int outerRight = -1;
					for (int x = 29; x >= 0; x--)
					{
						int r = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmap.Width].r * 255f);
						int g = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmap.Width].g * 255f);
						int b = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmap.Width].b * 255f);
						if (r >= 240 && g >= 240 && b >= 220)
						{
							outerRight = hRight + x + 1;
							break;
						}
					}

					UnityEngine.Debug.LogError(outerLeft + "," + outerTop + " (" + (outerBottom - outerTop) + "x" + (outerRight - outerLeft) + ")");
					if (outerRight == -1 || outerLeft == -1 || outerTop == -1 || outerBottom == -1)
					{
						return (-1, -1, -1, -1);
					}

					int padding = UnityEngine.Mathf.CeilToInt(((float) (outerRight - outerLeft)) * 0.090f);
					/*
					for (int x = 0; x < w; x++)
					{
						for (int y = 0; y < h; y++)
						{
							int r = (int) (colors[(hLeft + x) + (vTop + y) * bitmap.Width].r * 255f);
							int g = (int) (colors[(hLeft + x) + (vTop + y) * bitmap.Width].g * 255f);
							int b = (int) (colors[(hLeft + x) + (vTop + y) * bitmap.Width].b * 255f);
							rect[x + y * w] = !(r >= whiteR.Item1 && r <= whiteR.Item2 && g >= whiteG.Item1 && g <= whiteG.Item2 && b >= whiteB.Item1 && b <= whiteB.Item2);
						}
					}

					*/
					int realLeft = outerLeft + padding;
					int realRight = outerRight - (padding - 1);
					int realTop = outerTop + padding;
					int realBottom = outerBottom - (padding - 1);

					return (realLeft, realTop, realRight - realLeft, realBottom - realTop);
					/*
					int nonWhiteThreshold = (vBottom - vTop) / 5;
					for (int x = 0; x < w; x++)
					{
						int nonWhite = 0;
						for (int y = 0; y < h; y++)
						{
							if (rect[x + y * w]) nonWhite++;
						}
						if (nonWhite > nonWhiteThreshold)
						{
							realLeft = hLeft + x;
							break;
						}
					}

					for (int x = w - 1; x >= 0; x--)
					{
						int nonWhite = 0;
						for (int y = 0; y < h; y++)
						{
							if (rect[x + y * w]) nonWhite++;
						}
						if (nonWhite > nonWhiteThreshold)
						{
							realRight = hLeft + x + 2;
							break;
						}
					}

					nonWhiteThreshold = (vBottom - vTop) / 8;
					for (int y = 0; y < h; y++)
					{
						int nonWhite = 0;
						for (int x = 0; x < w; x++)
						{
							if (rect[x + y * w]) nonWhite++;
						}
						if (nonWhite > nonWhiteThreshold)
						{
							realTop = vTop + y;
							break;
						}
					}

					realBottom = realTop + (realRight - realLeft);*/
					
					/*UnityEngine.Debug.Log(realLeft + "," + realTop + "," + realRight + "," + realBottom);
					rects.Add((realLeft, realTop, realRight - realLeft, realBottom - realTop, rect));

					break;*/
				}
			}
		}
		/*
		if (rects.Count > 0)
		{
			var w = isPro ? 64 : 32;
			var h = w;

			float center = UnityEngine.Mathf.Ceil((float) rects[0].Item3) / ((float) w);
			var result = new System.Drawing.Bitmap((int) (rects[0].Item3), (int) (rects[0].Item4), System.Drawing.Imaging.PixelFormat.Format24bppRgb);
			using (Graphics graphics = Graphics.FromImage(result))
			{
				graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
				graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
				graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
				if (isPro)
				{
					graphics.DrawImage(bitmap, new Rectangle(0, 0, rects[0].Item3, rects[0].Item4), new Rectangle(rects[0].Item1, rects[0].Item2, rects[0].Item3, rects[0].Item4), GraphicsUnit.Pixel);
				}
				else
				{
					graphics.DrawImage(bitmap, new Rectangle(0, 0, rects[0].Item3, rects[0].Item4), new Rectangle(rects[0].Item1, rects[0].Item2, rects[0].Item3, rects[0].Item4), GraphicsUnit.Pixel);
				}
			}

			result.Save("result.png");
			var sampler = new NearestNeighbourSampling();
			var r = sampler.Resample(result, w, h);
			result.Dispose();
			result = r;
			foreach (var part in DesignPatternInformation.Types[type].Parts)
				for (var y = 0; y < part.Height; y++)
					for (var x = 0; x < part.Width; x++)
						if (part.Visible[x + y * part.Width] == '0')
							result.SetPixel(part.X + x, part.Y + y, Color.FromArgb(0, 0, 0, 0));

			return result;
		}*/
		return (-1, -1, -1, -1);
	}
}
