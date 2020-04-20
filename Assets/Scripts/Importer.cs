using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using System;
using UnityEngine;
using UnityEngine.UI;

public unsafe class Importer : MonoBehaviour
{
	public PixelGrid LeftGrid;
	public PixelGrid RightGrid;
	public MenuButton CancelButton;
	public MenuButton SaveButton;
	public Slider SaturationSlider;
	public Slider ContrastSlider;
	public Slider BrightnessSlider;
	public Slider SizeSlider;
	public Slider XSlider;
	public Slider YSlider;
	public TMPro.TMP_InputField SaturationInput;
	public TMPro.TMP_InputField ContrastInput;
	public TMPro.TMP_InputField BrightnessInput;
	public TMPro.TMP_InputField SizeInput;
	public TMPro.TMP_InputField XInput;
	public TMPro.TMP_InputField YInput;

	private System.Action<TextureBitmap> OnConfirm;
	private System.Action OnCancel;
	private TextureBitmap OriginalBitmap;
	private TextureBitmap LeftBitmap;
	private TextureBitmap RightBitmap;

	private int Contrast = 50;
	private int Saturation = 50;
	private int Brightness = 50;
	private float XOffset = 0f;
	private float YOffset = 0f;
	private float SizeOffset = 0f;
	private (int, int, int, int) Rect;
	private (int, int) ResultSize;
	private TextureBitmap.Color* Colors;

	public void Show(TextureBitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<TextureBitmap> onConfirm, System.Action onCancel)
	{
		Rect = rect;
		ResultSize = resultSize;
		OriginalBitmap = bitmap;
		Colors = OriginalBitmap.GetColors();

		BrightnessSlider.value = 0;
		ContrastSlider.value = 0;
		SaturationSlider.value = 100;
		XSlider.value = 0;
		YSlider.value = 0;
		SizeSlider.value = 0;
		BrightnessInput.text = "0";
		ContrastInput.text = "0";
		SizeInput.text = "0";
		XInput.text = "0";
		YInput.text = "0";
		XOffset = 0;
		YOffset = 0;
		SizeOffset = 0;
		SaturationInput.text = "100";
		Contrast = 0;
		Saturation = 100;
		Brightness = 0;

		OnConfirm = onConfirm;
		OnCancel = onCancel;

		LeftBitmap = new TextureBitmap(ResultSize.Item1, ResultSize.Item2);

		var colors = LeftBitmap.GetColors();
		int w = LeftBitmap.Width;
		int h = LeftBitmap.Height;
		int ow = OriginalBitmap.Width;
		for (var y = 0; y < h; y++)
		{
			for (var x = 0; x < w; x++)
			{
				float originalX = ((float) Rect.Item1) + x * ((((float) Rect.Item3)) / ((float) ResultSize.Item1));
				float originalY = ((float) Rect.Item2) + y * ((((float) Rect.Item4)) / ((float) ResultSize.Item2));
				int px = Mathf.RoundToInt(originalX);
				int py = Mathf.RoundToInt(originalY);

				var col = Colors[(px + py * ow)];
				*(colors + x + y * w) = col;
			}
		}
		LeftBitmap.Texture.filterMode = FilterMode.Point;
		LeftBitmap.Apply();

		RightBitmap = new TextureBitmap(ResultSize.Item1, ResultSize.Item2);

		float pixelSize = (384f / ((float) ResultSize.Item1));
		LeftGrid.SetSize(ResultSize.Item1, ResultSize.Item2, pixelSize);
		LeftGrid.PixelImage.texture = LeftBitmap.Texture;

		RightGrid.SetSize(ResultSize.Item1, ResultSize.Item2, pixelSize);

		gameObject.SetActive(true);
		UpdateTarget();
	}
	
	static (double, double, double) RGBToHSL((int, int, int) rgb)
	{
		float h;
		float s;
		float v;
		UnityEngine.Color.RGBToHSV(new Color(rgb.Item1 / 255f, rgb.Item2 / 255f, rgb.Item3 / 255f), out h, out s, out v);
		return (h, s, v);
	}

	public static (byte, byte, byte) HSLToRGB((double, double, double) hsl)
	{
		var c = UnityEngine.Color.HSVToRGB((float) hsl.Item1, (float) hsl.Item2, (float) hsl.Item3);
		return ((byte) (c.r * 255f), (byte) (c.g * 255f), (byte) (c.b * 255f));
	}

	void UpdateTarget()
	{
		if (Colors == null) return;
		RightBitmap.Clear();
		
		float sizeOffset = SizeOffset * (Rect.Item3 / ResultSize.Item1);
		float xOffset = XOffset * (Rect.Item3 / ResultSize.Item1);
		float yOffset = YOffset * (Rect.Item3 / ResultSize.Item1);
		int ow = OriginalBitmap.Width;
		int oh = OriginalBitmap.Height;
		int w = RightBitmap.Width;
		int h = RightBitmap.Height;

		var contrast = (259f * (Contrast + 255f)) / (255f * (259f - Contrast));
		unsafe
		{
			var colors = RightBitmap.GetColors();
			var index = 0;
			for (var y = 0; y < h; y++)
			{
				for (var x = 0; x < w; x++)
				{
					float originalX = ((float) Rect.Item1) + xOffset + x * ((((float) Rect.Item3) + sizeOffset) / ((float) ResultSize.Item1));
					float originalY = ((float) Rect.Item2) + yOffset + y * ((((float) Rect.Item4) + sizeOffset) / ((float) ResultSize.Item2));
					int px = Mathf.RoundToInt(originalX);
					int py = Mathf.RoundToInt(originalY);

					var col = Colors[(px + py * ow)];
					col.R = (byte) Math.Min(255, Math.Max(0, (contrast * (col.R - 128) + 128) + Brightness * 4));
					col.G = (byte) Math.Min(255, Math.Max(0, (contrast * (col.G - 128) + 128) + Brightness * 4));
					col.B = (byte) Math.Min(255, Math.Max(0, (contrast * (col.B - 128) + 128) + Brightness * 4));
					var hsl = RGBToHSL((col.R, col.G, col.B));
					hsl.Item2 *= (Saturation) / 100.0;
					hsl.Item1 = Math.Max(0f, Math.Min(1, hsl.Item1));
					hsl.Item2 = Math.Max(0f, Math.Min(1, hsl.Item2));
					hsl.Item3 = Math.Max(0f, Math.Min(1, hsl.Item3));
					var rgb = HSLToRGB(hsl);
					col.R = rgb.Item1;
					col.G = rgb.Item2;
					col.B = rgb.Item3;
					*(colors + x + y * w) = col;
				}
			}
		}

		RightBitmap.Quantize(new WuColorQuantizer(), 15);
		RightBitmap.Texture.filterMode = FilterMode.Point;
		RightBitmap.Apply();
		RightGrid.PixelImage.texture = RightBitmap.Texture;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
		// @TODO: Add transition
	}

	private void Start()
	{
		BrightnessSlider.onValueChanged.AddListener((val) => {
			BrightnessInput.text = ((int) val) + "";
			Brightness = (int) val;
			UpdateTarget();
		});
		SaturationSlider.onValueChanged.AddListener((val) => {
			SaturationInput.text = ((int) val) + "";
			Saturation = (int) val;
			UpdateTarget();
		});
		ContrastSlider.onValueChanged.AddListener((val) => {
			ContrastInput.text = ((int) val) + "";
			Contrast = (int) val;
			UpdateTarget();
		});


		XSlider.onValueChanged.AddListener((val) => {
			val = Mathf.FloorToInt(val * 10f) / 10f;
			XInput.text = (val) + "";
			XOffset = val;
			UpdateTarget();
		});
		YSlider.onValueChanged.AddListener((val) => {
			val = Mathf.FloorToInt(val * 10f) / 10f;
			YInput.text = (val) + "";
			YOffset = val;
			UpdateTarget();
		});
		SizeSlider.onValueChanged.AddListener((val) => {
			val = Mathf.FloorToInt(val * 10f) / 10f;
			SizeInput.text = (val) + "";
			SizeOffset = val;
			UpdateTarget();
		});

		ContrastInput.onValueChanged.AddListener((val) => {
			try
			{
				int n = int.Parse(val);
				ContrastSlider.value = n;
				Contrast = (int) ContrastSlider.value;
				ContrastInput.text = Contrast + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});
		SaturationInput.onValueChanged.AddListener((val) => {
			try
			{
				int n = int.Parse(val);
				SaturationSlider.value = n;
				Saturation = (int) SaturationSlider.value;
				SaturationInput.text = Saturation + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});
		BrightnessInput.onValueChanged.AddListener((val) =>
		{
			try
			{
				int n = int.Parse(val);
				BrightnessSlider.value = n;
				Brightness = (int) BrightnessSlider.value;
				BrightnessInput.text = Brightness + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});
		XInput.onValueChanged.AddListener((val) =>
		{
			try
			{
				float n = float.Parse(val);
				n = Mathf.Floor(n * 10f) / 10f;
				XSlider.value = n;
				XOffset = n;
				XInput.text = n + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});
		YInput.onValueChanged.AddListener((val) =>
		{
			try
			{
				float n = float.Parse(val);
				n = Mathf.Floor(n * 10f) / 10f;
				YSlider.value = n;
				YOffset = n;
				YInput.text = n + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});
		SizeInput.onValueChanged.AddListener((val) =>
		{
			try
			{
				float n = float.Parse(val);
				n = Mathf.Floor(n * 10f) / 10f;
				SizeSlider.value = n;
				SizeOffset = n;
				SizeInput.text = n + "";
				UpdateTarget();
			}
			catch (Exception) { }
		});


		CancelButton.OnClick = () =>
		{
			Colors = null;
			LeftBitmap.Dispose();
			RightBitmap.Dispose();
			OnCancel?.Invoke();
		};

		SaveButton.OnClick = () =>
		{
			Colors = null;
			LeftBitmap.Dispose();
			OnConfirm?.Invoke(RightBitmap);
		};
	}
}
