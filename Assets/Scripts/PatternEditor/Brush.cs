using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Brush
{
	public int Size;
	public float Hardness = 100f;
	public float[] Alphas;
	public Texture2D BrushOverlayTexture;
	public Texture2D BrushTexture;
	public Sprite BrushSprite;
	public PatternEditor Editor;

	public void RecalculateBrush()
	{
		if (Editor == null)
			return;

		int brushWidth = 4;
		if (BrushOverlayTexture != null)
		{
			this.BrushOverlayTexture.Resize(this.Size * this.Editor.PixelSize, this.Size * this.Editor.PixelSize);
		}
		else this.BrushOverlayTexture = new Texture2D(this.Size * this.Editor.PixelSize, this.Size * this.Editor.PixelSize, TextureFormat.ARGB32, false);

		if (BrushTexture != null)
		{
			this.BrushTexture.Resize(this.Size, this.Size);
			GameObject.DestroyImmediate(BrushSprite);
		}
		else
		{
			this.BrushTexture = new Texture2D(this.Size, this.Size, TextureFormat.ARGB32, false);
			BrushTexture.filterMode = FilterMode.Point;
		}

		Vector2 center = new Vector2(0.5f, 0.5f);
		Alphas = new float[Size * Size];

		UnityEngine.Color[] brushColors = new UnityEngine.Color[BrushTexture.width * BrushTexture.height];
		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				int index = x + y * Size;
				Vector2 c = new Vector2(
					((float) x) / ((float) Size) + 0.5f / ((float) Size),
					((float) y) / ((float) Size) + 0.5f / ((float) Size));
				Alphas[index] = Mathf.Max(0f, Mathf.Min(1f, (0.5f - Vector2.Distance(c, new Vector2(0.5f, 0.5f))) * (Hardness * Hardness * Size)));
				brushColors[index] = new UnityEngine.Color(1f, 1f, 1f, Alphas[index]);
			}
		}
		BrushTexture.SetPixels(brushColors);
		BrushTexture.Apply();
		BrushSprite = Sprite.Create(BrushTexture, new UnityEngine.Rect(0, 0, BrushTexture.width, BrushTexture.height), new Vector2(0.5f, 0.5f));
		
		var brushColor = new UnityEngine.Color(1f, 1f, 1f, 0.5f);
		UnityEngine.Color[] brushOverlayColors = new UnityEngine.Color[BrushOverlayTexture.width * BrushOverlayTexture.height];
		for (int i = 0; i < brushOverlayColors.Length; i++)
			brushOverlayColors[i] = new UnityEngine.Color(0f, 0f, 0f, 0f);
		for (int x = 0; x < Size; x++)
		{
			for (int y = 0; y < Size; y++)
			{
				int index = x + y * Size;
				if (Alphas[index] > 0f)
				{
					if (x > 0 && y > 0 && Alphas[index - 1 - Size] <= 0f && Alphas[index - 1] > 0f && Alphas[index - Size] > 0f)
					{
						// draw top left
						for (int nX = 0; nX < brushWidth; nX++)
							for (int nY = 0; nY < brushWidth; nY++)
								brushOverlayColors[x * Editor.PixelSize + nX + (y * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (x < Size - 1 && y > 0 && Alphas[index + 1 - Size] <= 0f && Alphas[index + 1] > 0f && Alphas[index - Size] > 0f)
					{
						// draw top right
						for (int nX = -brushWidth; nX < 0; nX++)
							for (int nY = 0; nY < brushWidth; nY++)
								brushOverlayColors[(x + 1) * Editor.PixelSize + nX + (y * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (x > 0 && y < Size - 1 && Alphas[index - 1 + Size] <= 0f && Alphas[index - 1] > 0f && Alphas[index + Size] > 0f)
					{
						// draw bottom left
						for (int nX = 0; nX < brushWidth; nX++)
							for (int nY = -brushWidth; nY < 0; nY++)
								brushOverlayColors[x * Editor.PixelSize + nX + ((y + 1) * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (x < Size - 1 && y < Size - 1 && Alphas[index + 1 + Size] <= 0f && Alphas[index + 1] > 0f && Alphas[index + Size] > 0f)
					{
						// draw bottom right
						for (int nX = -brushWidth; nX < 0; nX++)
							for (int nY = -brushWidth; nY < 0; nY++)
								brushOverlayColors[(x + 1) * Editor.PixelSize + nX + ((y + 1) * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (x == 0 || Alphas[index - 1] <= 0f)
					{
						// draw left line
						for (int nX = 0; nX < brushWidth; nX++)
							for (int nY = 0; nY < Editor.PixelSize; nY++)
								brushOverlayColors[x * Editor.PixelSize + nX + (y * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (x == Size - 1 || Alphas[index + 1] <= 0f)
					{
						// draw right line
						for (int nX = -brushWidth; nX < 0; nX++)
							for (int nY = 0; nY < Editor.PixelSize; nY++)
								brushOverlayColors[(x + 1) * Editor.PixelSize + nX + (y * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (y == 0 || Alphas[index - Size] <= 0f)
					{
						// draw top line
						for (int nX = 0; nX < Editor.PixelSize; nX++)
							for (int nY = 0; nY < brushWidth; nY++)
								brushOverlayColors[x * Editor.PixelSize + nX + (y * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
					if (y == Size - 1 || Alphas[index + Size] <= 0f)
					{
						// draw bottom line
						for (int nX = 0; nX < Editor.PixelSize; nX++)
							for (int nY = -brushWidth; nY < 0; nY++)
								brushOverlayColors[x * Editor.PixelSize + nX + ((y + 1) * Editor.PixelSize + nY) * BrushOverlayTexture.width] = brushColor;
					}
				}
			}
		}

		BrushOverlayTexture.SetPixels(brushOverlayColors);
		BrushOverlayTexture.Apply();

		Editor.BrushPreviewUpdated();
	}

	public void Draw(IBrushDrawable drawable, int brushX, int brushY, UnityEngine.Color color)
	{
		for (int by = 0; by < Size; by++)
		{
			for (int bx = 0; bx < Size; bx++)
			{
				int fx = brushX + bx;
				int fy = brushY + by;

				if (drawable.IsDrawable(fx, fy))
				{
					int index = fx + (fy * drawable.Width);
					int brushIndex = (int) (bx + by * Size);

					UnityEngine.Color A = color;
					A.a *= Alphas[brushIndex];

					drawable.Colors[index] = drawable.Colors[index].AlphaComposite(A);
				}
			}
		}
	}

	public void Erase(IBrushDrawable drawable, int brushX, int brushY)
	{
		for (int by = 0; by < Size; by++)
		{
			for (int bx = 0; bx < Size; bx++)
			{
				int fx = brushX + bx;
				int fy = brushY + by;
				if (drawable.IsDrawable(fx, fy))
				{
					int index = fx + (fy * drawable.Width);
					int brushIndex = (int) (bx + by * Size);

					drawable.Colors[index].a = Mathf.Clamp01(drawable.Colors[index].a - Alphas[brushIndex]);
				}
			}
		}
	}

	public void DrawLine(IBrushDrawable drawable, int fromX, int fromY, int toX, int toY, UnityEngine.Color color)
	{
		var distanceX = (float) Mathf.Abs(fromX - toX);
		var distanceY = (float) Mathf.Abs(fromY - toY);

		float stepX = 0f;
		float stepY = 0f;
		int iterations = 0;
		if (distanceX > distanceY)
		{
			iterations = (int) distanceX;
			if (iterations > 0)
			{
				stepX = fromX < toX ? 1f : -1f;
				stepY = (toY - fromY) / distanceX;
			}
		}
		else
		{
			iterations = (int) distanceY;
			if (iterations > 0)
			{
				stepX = (toX - fromX) / distanceY;
				stepY = fromY < toY ? 1f : -1f;
			}
		}

		float lx = fromX;
		float ly = fromY;
		for (int li = 0; li < iterations; li++)
		{
			Draw(drawable, (int) Mathf.RoundToInt(lx), (int) Mathf.RoundToInt(ly), color);
			ly += stepY;
			lx += stepX;
		}
		Draw(drawable, (int) (toX), (int) (toY), color);

	}
}
