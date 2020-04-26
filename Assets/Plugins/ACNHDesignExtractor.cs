using System.Collections.Generic;

public class ACNHDesignExtractor
{
	public static (int, int, int, int) FindPattern(TextureBitmap bitmap, DesignPattern.TypeEnum type)
	{
		bool isPro = type != DesignPattern.TypeEnum.SimplePattern;
		unsafe
		{
			var colors = bitmap.GetColors();
			int bitmapWidth = bitmap.Width;
			int bitmapHeight = bitmap.Height;
			bool[] bw = new bool[bitmapWidth * bitmapHeight]; // true = black
			int min = 180;
			int max = 235;
			for (int x = 0; x < bitmapWidth; x++)
			{
				for (int y = 0; y < bitmapHeight; y++)
				{
					float h;
					float s;
					float v;
					var col = colors[x + y * bitmapWidth];
					int r = (int) (col.R);
					int g = (int) (col.G);
					int b = (int) (col.B);
					r = r * (r >= min && r <= max ? 0 : 1);
					g = g * (g >= min && g <= max ? 0 : 1);
					b = b * (b >= min && b <= max ? 0 : 1);
					bw[x + y * bitmapWidth] = r == 0 && g == 0;
				}
			}

			int widthThreshold = 5;
			int minWidth = 2;
			List<(int, int, int)> horizontalLines = new List<(int, int, int)>();
			List<(int, int, int)> verticalLines = new List<(int, int, int)>();

			for (int y = 0; y < bitmapHeight; y++)
			{
				int currentDashWidth = 0;
				int currentSpaceWidth = 0;
				int dashes = 0;
				int dashWidth = 0;
				int start = 0;
				bool black = false;

				for (int x = 0; x < bitmapWidth; x++)
				{
					int idx = x + y * bitmapWidth;
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

			for (int x = 0; x < bitmapWidth; x++)
			{
				int currentDashWidth = 0;
				int currentSpaceWidth = 0;
				int dashes = 0;
				int dashWidth = 0;
				int start = 0;
				bool black = false;

				for (int y = 0; y < bitmapHeight; y++)
				{
					int idx = x + y * bitmapWidth;
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
							int r = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmapWidth].R);
							int g = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmapWidth].G);
							int b = (int) (colors[(hLeft + w / 2) + (vTop + y) * bitmapWidth].B);
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
							int r = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmapWidth].R);
							int g = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmapWidth].G);
							int b = (int) (colors[(hLeft + x) + (vTop + h / 2) * bitmapWidth].B);
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
							int r = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmapWidth].R);
							int g = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmapWidth].G);
							int b = (int) (colors[(hLeft + w / 2) + (vBottom + y) * bitmapWidth].B);
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
							int r = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmapWidth].R);
							int g = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmapWidth].G);
							int b = (int) (colors[(hRight + x) + (vTop + h / 2) * bitmapWidth].B);
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

						int realLeft = outerLeft + padding;
						int realRight = outerRight - (padding - 1);
						int realTop = outerTop + padding;
						int realBottom = outerBottom - (padding - 1);

						return (realLeft, realTop, realRight - realLeft, realBottom - realTop);
					}
				}
			}
		}
		
		return (-1, -1, -1, -1);
	}
}
