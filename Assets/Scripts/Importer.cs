using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Importer : MonoBehaviour
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

	private System.Action<System.Drawing.Bitmap> OnConfirm;
	private System.Action OnCancel;
	private Texture2D Texture;
	private Texture2D FinishedTexture;
	private System.Drawing.Bitmap OriginalBitmap;
	private System.Drawing.Bitmap FinishedBitmap;
	private int Contrast = 50;
	private int Saturation = 50;
	private int Brightness = 50;
	private float XOffset = 0f;
	private float YOffset = 0f;
	private float SizeOffset = 0f;
	private (int, int, int, int) Rect;
	private (int, int) ResultSize;
	private byte[] Colors;

	public void Show(System.Drawing.Bitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<System.Drawing.Bitmap> onConfirm, System.Action onCancel)
	{
		Rect = rect;
		ResultSize = resultSize;
		OriginalBitmap = bitmap;

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

		var leftBitmap = new System.Drawing.Bitmap(ResultSize.Item1, ResultSize.Item2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(leftBitmap))
		{
			graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
			graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.NearestNeighbor;
			graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
			//draw the image into the target bitmap
			graphics.DrawImage(bitmap, new System.Drawing.Rectangle(0, 0, ResultSize.Item1, ResultSize.Item2), new System.Drawing.Rectangle(Rect.Item1, Rect.Item2, Rect.Item3, Rect.Item4), System.Drawing.GraphicsUnit.Pixel);
		}

		Texture = leftBitmap.ToTexture2D(Texture);
		Texture.filterMode = FilterMode.Point;
		leftBitmap.Dispose();

		float pixelSize = (384f / ((float) ResultSize.Item1));
		LeftGrid.SetSize(ResultSize.Item1, ResultSize.Item2, pixelSize);
		LeftGrid.PixelImage.texture = Texture;

		RightGrid.SetSize(ResultSize.Item1, ResultSize.Item2, pixelSize);
		Colors = OriginalBitmap.GetBytes();

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
		var bitmap = new System.Drawing.Bitmap(ResultSize.Item1, ResultSize.Item2, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.WriteOnly, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		var pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		var padding = data.Stride - (data.Width * pixelSize);

		float sizeOffset = SizeOffset * (Rect.Item3 / ResultSize.Item1);
		float xOffset = XOffset * (Rect.Item3 / ResultSize.Item1);
		float yOffset = YOffset * (Rect.Item3 / ResultSize.Item1);
		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{

					float originalX = ((float) Rect.Item1) + xOffset + x * ((((float) Rect.Item3) + sizeOffset) / ((float) ResultSize.Item1));
					float originalY = ((float) Rect.Item2) + yOffset + y * ((((float) Rect.Item4) + sizeOffset) / ((float) ResultSize.Item2));
					int px = Mathf.RoundToInt(originalX);
					int py = Mathf.RoundToInt(originalY);

					*(ptr + index + 2) = Colors[(px + py * OriginalBitmap.Width) * 4 + 0];
					*(ptr + index + 1) = Colors[(px + py * OriginalBitmap.Width) * 4 + 1];
					*(ptr + index + 0) = Colors[(px + py * OriginalBitmap.Width) * 4 + 2];
					*(ptr + index + 3) = Colors[(px + py * OriginalBitmap.Width) * 4 + 3];
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		var quantizer = new WuColorQuantizer();

		data = bitmap.LockBits(new System.Drawing.Rectangle(System.Drawing.Point.Empty, bitmap.Size), System.Drawing.Imaging.ImageLockMode.ReadWrite, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
		pixelSize = data.PixelFormat == System.Drawing.Imaging.PixelFormat.Format32bppArgb ? 4 : 3;
		padding = data.Stride - (data.Width * pixelSize);

		var contrast = (259f * (Contrast + 255f)) / (255f * (259f - Contrast));
		unsafe
		{
			byte* ptr = (byte*) data.Scan0.ToPointer();

			var index = 0;
			for (var y = 0; y < data.Height; y++)
			{
				for (var x = 0; x < data.Width; x++)
				{
					int r = *(ptr + index + 2);
					int g = *(ptr + index + 1);
					int b = *(ptr + index + 0);
					int a = *(ptr + index + 3);
					r = (int) (contrast * (r - 128) + 128) + Brightness * 4;
					g = (int) (contrast * (g - 128) + 128) + Brightness * 4;
					b = (int) (contrast * (b - 128) + 128) + Brightness * 4;
					r = Math.Min(255, Math.Max(0, r));
					g = Math.Min(255, Math.Max(0, g));
					b = Math.Min(255, Math.Max(0, b));
					var hsl = RGBToHSL((r, g, b));
					/*if (Brightness < 0)
						hsl.Item3 *= 1f - (Brightness / -50f);
					else
						hsl.Item3 *= 1f + (Brightness / 10f);
						*/
					hsl.Item2 *= (Saturation) / 100.0;

					hsl.Item1 = Math.Max(0f, Math.Min(1, hsl.Item1));
					hsl.Item2 = Math.Max(0f, Math.Min(1, hsl.Item2));
					hsl.Item3 = Math.Max(0f, Math.Min(1, hsl.Item3));

					var rgb = HSLToRGB(hsl);
					*(ptr + index + 2) = (byte) rgb.Item1;
					*(ptr + index + 1) = (byte) rgb.Item2;
					*(ptr + index + 0) = (byte) rgb.Item3;
					index += pixelSize;
				}
				index += padding;
			}
		}
		bitmap.UnlockBits(data);

		FinishedBitmap = (System.Drawing.Bitmap) ImageBuffer.QuantizeImage(bitmap, quantizer, 15, 1);
		FinishedTexture = FinishedBitmap.ToTexture2D(FinishedTexture);
		FinishedTexture.filterMode = FilterMode.Point;
		RightGrid.PixelImage.texture = FinishedTexture;
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
			OnCancel?.Invoke();
		};

		SaveButton.OnClick = () =>
		{
			OnConfirm?.Invoke(FinishedBitmap);
		};
	}
}
