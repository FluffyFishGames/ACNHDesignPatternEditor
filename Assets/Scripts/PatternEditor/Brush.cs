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
	public Sprite BrushSprite;
	public PatternEditor Editor;
	protected TextureBitmap BrushTexture;

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

		if (this.Size == 0) this.Size = 1;
		if (BrushTexture != null)
		{
			this.BrushTexture.Resize(this.Size, this.Size);
			GameObject.Destroy(BrushSprite);
		}
		else
		{
			this.BrushTexture = new TextureBitmap(this.Size, this.Size);
		}

		Vector2 center = new Vector2(0.5f, 0.5f);
		Alphas = new float[Size * Size];

		for (int y = 0; y < Size; y++)
		{
			for (int x = 0; x < Size; x++)
			{
				int index = x + y * Size;
				Vector2 c = new Vector2(
					((float) x) / ((float) Size) + 0.5f / ((float) Size),
					((float) y) / ((float) Size) + 0.5f / ((float) Size));
				float alpha = Mathf.Max(0f, Mathf.Min(1f, (0.5f - Vector2.Distance(c, new Vector2(0.5f, 0.5f))) * (Hardness * Hardness * Size)));
				
				Alphas[x + y * Size] = alpha;
				this.BrushTexture.SetPixel(x, y, new TextureBitmap.Color(255, 255, 255, (byte) (alpha * 255f)));
			}
		}
		this.BrushTexture.Apply();
		BrushSprite = Sprite.Create(BrushTexture.Texture, new UnityEngine.Rect(0, 0, BrushTexture.Width, BrushTexture.Height), new Vector2(0.5f, 0.5f));
		
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
		drawable.Bitmap.AlphaComposite(BrushTexture, new TextureBitmap.Color((byte) (color.r * 255f), (byte) (color.g * 255f), (byte) (color.b * 255f), (byte) (color.a * 255f)), new TextureBitmap.Point(brushX, brushY), null, drawable.IsDrawable);
	}

	public void Erase(IBrushDrawable drawable, int brushX, int brushY)
	{
		drawable.Bitmap.Subtract(BrushTexture, new TextureBitmap.Color(0, 0, 0, 255), new TextureBitmap.Point(brushX, brushY), null, drawable.IsDrawable);
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

	public void Dispose()
	{
		GameObject.Destroy(this.BrushOverlayTexture);
		GameObject.Destroy(this.BrushSprite);
		if (this.BrushTexture != null)
			this.BrushTexture.Dispose();
		this.Alphas = null;
	}
}
