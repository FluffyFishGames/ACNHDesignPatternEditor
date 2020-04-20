using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PixelGrid : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler, IPointerUpHandler
{
	public PatternEditor Editor;
	public RectTransform EditGrid;
	public RectTransform BrushOverlay;
	public RawImage BrushOverlayImage;
	public RawImage PixelImage;
	public RawImage GridImage;
	public Image LineImage;
	public Image UVImage;

	public Toggle ShowUVToggle;

	private Pixel[] Pixels;
	private bool MouseOver = false;
	private bool LastMouseDown = false;
	private bool MouseDown = false;
	[HideInInspector]
	public RectTransform RectTransform;
	private int LastBrushX;
	private int LastBrushY;
	public bool PixelsUpdated = false;
	private Texture2D PixelTexture;
	private Texture2D GridTexture;

	public void OnPointerDown(PointerEventData eventData)
	{
		MouseDown = true;
	}

	public void OnPointerUp(PointerEventData eventData)
	{
		MouseDown = false;
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		MouseOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		MouseOver = false;
	}

	public void SetSize(int width, int height, float pixelSize)
	{
		var color = new Color(99f / 255f, 71f / 255f, 58f / 255f);
		GridTexture = GridCreator.CreateGrid(color, width, height, (int) pixelSize);

		GridImage.raycastTarget = false;
		GridImage.texture = GridTexture;
		EditGrid.sizeDelta = new Vector2(pixelSize * width, pixelSize * height);
		PixelTexture = new Texture2D((int) (width), (int) (height), TextureFormat.ARGB32, true);
		PixelTexture.filterMode = FilterMode.Point;
		PixelImage.texture = PixelTexture;

		Pixels = new Pixel[width * height];
		for (int x = 0; x < width; x++)
			for (int y = 0; y < height; y++)
				Pixels[x + y * width] = new Pixel(this, PixelTexture, x, y, (int) pixelSize);
	}

	public void Dispose()
	{
		GameObject.Destroy(GridTexture);
		GameObject.Destroy(PixelTexture);
	}

	public void UpdateImage()
	{
		var subPattern = Editor.CurrentPattern.CurrentSubPattern;
		PixelTexture = Editor.CurrentPattern.CurrentSubPattern.Bitmap.Texture;
		PixelImage.texture = PixelTexture;

		/*for (int y = 0; y < subPattern.Height; y++)
		{
			for (int x = 0; x < subPattern.Width; x++)
			{
				int idx = x + y * subPattern.Width;
				var col = subPattern.Bitmap.GetPixel(x, y);
				col.A = (byte) (col.A * (Editor.CurrentPattern.CurrentSubPattern.IsVisible(x, y) ? 1f : 0.5f));
				Pixels[idx].SetColor(new UnityEngine.Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f));
			}
		}
		PixelTexture.Apply();*/
	}

	public void SubPatternChanged()
	{
		var subPattern = Editor.CurrentPattern.CurrentSubPattern;
		if (Editor.Previews.AllLines.ContainsKey(Editor.CurrentPattern.Type))
		{
			var texture = Editor.Previews.AllLines[Editor.CurrentPattern.Type];
			var uvTexture = Editor.Previews.AllUVs[Editor.CurrentPattern.Type];
			float uvX = ((float) subPattern.Part.X) / ((float) Editor.CurrentPattern.Width);
			float uvY = ((float) subPattern.Part.Y) / ((float) Editor.CurrentPattern.Height);
			float uvW = ((float) subPattern.Part.Width) / ((float) Editor.CurrentPattern.Width);
			float uvH = ((float) subPattern.Part.Height) / ((float) Editor.CurrentPattern.Height);
			if (LineImage != null)
			{
				LineImage.sprite = Sprite.Create(texture, new Rect(uvX * texture.width, (1f - uvY - uvH) * texture.height, uvW * texture.width, uvH * texture.height), new Vector2(0.5f, 0.5f));
				LineImage.color = new Color(1f, 1f, 1f, 0.5f);
			}

			if (UVImage != null)
			{
				UVImage.sprite = Sprite.Create(uvTexture, new Rect(uvX * uvTexture.width, (1f - uvY - uvH) * uvTexture.height, uvW * uvTexture.width, uvH * uvTexture.height), new Vector2(0.5f, 0.5f));
				UVImage.color = new Color(1f, 1f, 1f, 0.5f);
			}
		}
		else
		{
			if (LineImage != null)
				LineImage.color = new Color(0f, 0f, 0f, 0f);
			if (UVImage != null)
				UVImage.color = new Color(0f, 0f, 0f, 0f);
		}
	}

	public void BrushPreviewUpdated()
	{
		BrushOverlayImage.texture = Editor.CurrentBrush.BrushOverlayTexture;
		BrushOverlay.sizeDelta = new Vector2(BrushOverlayImage.texture.width, BrushOverlayImage.texture.height);
	}

	void Start()
	{
		if (ShowUVToggle != null)
		{
			ShowUVToggle.onValueChanged.AddListener((val) =>
			{
				UVImage.gameObject.SetActive(val);
			});
		}
	}

	public void PatternLoaded()
	{
		if (ShowUVToggle != null)
		{
			ShowUVToggle.isOn = false;
			UVImage.gameObject.SetActive(false);

			ShowUVToggle.gameObject.SetActive(Editor.CurrentPattern.Type != MyHorizons.Data.DesignPattern.TypeEnum.SimplePattern);
		}
	}

	void OnEnable()
	{
		RectTransform = GetComponent<RectTransform>();
	}

	private bool CursorSet = false;

	void Update()
	{
		Vector2 vec;
		if (Editor == null)
			return;
		if (Editor.CurrentTool == null)
			return;

		if (RectTransformUtility.ScreenPointToLocalPointInRectangle(RectTransform, Input.mousePosition, Camera.main, out vec))
		{
			Vector2 pos = (vec + new Vector2(RectTransform.sizeDelta.x / 2f, -RectTransform.sizeDelta.y / 2f)) / Editor.PixelSize;
			pos.y = -pos.y;

			float x = 0f;
			float y = 0f;
			if (((int) Editor.CurrentBrush.Size) % 2 == 0)
			{
				x = (Mathf.Round(pos.x));
				y = (Mathf.Round(pos.y));
			}
			else
			{
				x = (Mathf.Round(pos.x - 0.5f) + 0.5f);
				y = (Mathf.Round(pos.y - 0.5f) + 0.5f);
			}
			int brushX = (int) (x - Editor.CurrentBrush.Size / 2f);
			int brushY = (int) (y - Editor.CurrentBrush.Size / 2f);

			if (!LastMouseDown && MouseDown)
				Editor.CurrentTool.MouseDown(brushX, brushY);

			if (pos.x >= 0 && pos.x < Editor.CurrentPattern.CurrentSubPattern.Width &&
				pos.y >= 0 && pos.y < Editor.CurrentPattern.CurrentSubPattern.Height)
			{
				if (Editor.Tools.CurrentTool == Tools.Tool.ColorPicker)
				{
					CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ColorPicker);
					CursorSet = true;
				}
				else if (CursorSet) { CursorSet = false; CursorManager.Instance.RemoveCursor(-1); }
				if (!BrushOverlay.gameObject.activeSelf)
					BrushOverlay.gameObject.SetActive(true);
				if (((int) Editor.CurrentBrush.Size) % 2 == 0)
				{
					BrushOverlay.anchoredPosition = new Vector2(x * Editor.PixelSize, -y * Editor.PixelSize);
				}
				else
				{
					BrushOverlay.anchoredPosition = new Vector2(x * Editor.PixelSize, -y * Editor.PixelSize);
				}
			}
			else if (BrushOverlay.gameObject.activeSelf)
			{
				if (CursorSet) { CursorSet = false; CursorManager.Instance.RemoveCursor(-1); }
				BrushOverlay.gameObject.SetActive(false);
			}

			if (LastBrushX != brushX || LastBrushY != brushY)
			{
				if (Editor.CurrentTool != null)
				{
					if (MouseDown)
						Editor.CurrentTool.MouseDrag(brushX, brushY, LastBrushX, LastBrushY);
					else
						Editor.CurrentTool.MouseMove(brushX, brushY);
				}
				LastBrushX = brushX;
				LastBrushY = brushY;
			}

			if (!Input.GetMouseButton(0))
				MouseDown = false;
			if (LastMouseDown && !MouseDown)
				Editor.CurrentTool.MouseUp(brushX, brushY);

			LastMouseDown = MouseDown;
		}
		/*
		if (PixelsUpdated)
			PixelTexture.Apply();*/
	}
}
