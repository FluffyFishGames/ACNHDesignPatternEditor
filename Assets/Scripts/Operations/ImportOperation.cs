using MyHorizons.Data;
using MyHorizons.Data.Save;
using SFB;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.Helpers;
using SimplePaletteQuantizer.Quantizers;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;
using ZXing;

public class ImportOperation : IOperation, IPatternOperation, IChangeNameOperation
{
	private System.Drawing.Bitmap Image;
	private System.Drawing.Bitmap Result;
	private DesignPattern Pattern;
	private DesignPattern ResultPattern;
	private bool _IsParsing;
	private bool _IsReady;
	private Sprite ResultPreview;
	private UnityEngine.Color[] ResultPixels;
	private string Name;
	private bool _IsFinished = false;

	public bool IsParsing
	{
		get
		{
			return _IsParsing;
		}
		private set
		{
			_IsParsing = value;
		}
	}

	public bool IsReady
	{
		get
		{
			return _IsReady;
		}
		private set
		{
			_IsReady = value;
		}
	}

	public int ImageWidth
	{
		get
		{
			return Image.Width;
		}
	}

	public int ImageHeight
	{
		get
		{
			return Image.Height;
		}
	}

	private bool _IsQRCode = false;

	public bool IsQRCode
	{
		get
		{
			return _IsQRCode;
		}
		private set
		{
			_IsQRCode = value;
		}
	}

	public string Username;
	public string Town;

	public ImportOperation(DesignPattern pattern)
	{
		this.Pattern = pattern;
		this.Name = this.Pattern.Name;
	}

	public void SetImage(System.Drawing.Bitmap image)
	{
		var scanner = new BarcodeReader()
		{
			TryInverted = true,
			AutoRotate = true,

			Options = new ZXing.Common.DecodingOptions()
			{
				TryHarder = true,
				CharacterSet = "ISO-8859-1",
				PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE },
			}
		};


		var result = scanner.Decode(image);
		if (result != null)
		{
			var bytes = Encoding.GetEncoding("ISO-8859-1").GetBytes(result.Text);
			if (bytes.Length == 0x26D && bytes[0x6A] == 0x09) // its a simple pattern
			{
				var qrCode = new ACQRCode(bytes);
				IsQRCode = true;
				this.Image = qrCode.GetImage();
				this.Name = qrCode.Name;
				this.Username = qrCode.Username;
				this.Town = qrCode.TownName;
				this.Result = this.Image;
				this.IsReady = true;
			}
		}
		else
			this.Image = image;
	}

	public void ParsePattern(ICrop crop, ISampling sampling, IColorQuantizer quantizer, IColorCache colorCache)
	{
		if (!this.IsParsing)
		{
			IsParsing = true;
			Thread thread = new Thread(() =>
			{
				var bmp = new Bitmap((System.Drawing.Image) Image.Clone());

				int desiredWidth = 32;
				int desiredHeight = 32;
				if (crop != null)
				{
					crop.SetImage(bmp);
					desiredWidth = crop.GetWidth();
					desiredHeight = crop.GetHeight();
				}

				if (quantizer is BaseColorCacheQuantizer colorCacheQuantizer)
					colorCacheQuantizer.ChangeCacheProvider(colorCache);

				var sampledBmp = sampling.Resample(bmp, desiredWidth, desiredHeight);
				bmp.Dispose();
				bmp = sampledBmp;

				Bitmap croppedBmp = new Bitmap(32, 32, System.Drawing.Imaging.PixelFormat.Format32bppArgb);
				using (System.Drawing.Graphics graphics = System.Drawing.Graphics.FromImage(croppedBmp))
				{
					graphics.CompositingQuality = System.Drawing.Drawing2D.CompositingQuality.HighQuality;
					graphics.DrawImage(bmp, (32 - bmp.Width) / 2, (32 - bmp.Height) / 2, bmp.Width, bmp.Height);
				}

				bmp.Dispose();
				bmp = croppedBmp;

				var targetImage = ImageBuffer.QuantizeImage(bmp, quantizer, null, 15, 1);

				bmp.Dispose();
				bmp = new Bitmap(targetImage);

				Result = bmp;
				IsReady = true;
				IsParsing = false;
			});
			thread.Start();
		}
	}

	public DesignPattern GetResultPattern()
	{
		if (IsReady)
		{
			if (ResultPattern != null)
			{
				if (ResultPreview != null)
				{
					GameObject.DestroyImmediate(ResultPreview.texture);
					GameObject.DestroyImmediate(ResultPreview);
				}
				ResultPreview = null;
				ResultPixels = null;
			}
			ResultPattern = new DesignPattern();
			ResultPattern.FromBitmap(Result);
			ResultPattern.Name = Pattern.Name;
			ResultPattern.PersonalID = new MyHorizons.Data.PlayerData.PersonalID() { UniqueId = 0xFFFFFFFF, TownId = Pattern.PersonalID.TownId };
			ResultPattern.PersonalID.SetName(Pattern.PersonalID.GetName());
			IsReady = false;
		}
		return ResultPattern;
	}

	public UnityEngine.Color[] GetPixels()
	{
		if (ResultPixels == null && GetResultPattern() != null)
			ResultPixels = GetResultPattern().GetPixels();
		return ResultPixels;
	}

	public Sprite GetPreview()
	{
		if (ResultPreview == null && GetResultPattern() != null)
			ResultPreview = GetResultPattern().GetPreview();
		return ResultPreview;
	}

	public string GetName()
	{
		return this.Name;
	}

	public bool IsFinished()
	{
		return _IsFinished;
	}

	public void Start()
	{
		Controller.Instance.Popup.SetText("Please select any <#FF6666>Image<#FFFFFF> file to import.", false, () => {
			StandaloneFileBrowser.OpenFilePanelAsync("Import image", "", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif" }) }, false, (path) =>
			{
				if (path.Length > 0)
				{
					var bmp = new Bitmap(System.Drawing.Image.FromFile(path[0]));
					this.SetImage(bmp);
					Controller.Instance.PatternSelector.ActionMenu.Close();
					Controller.Instance.SwitchToPatternEditor(
						() =>
						{
							Controller.Instance.SwitchToNameInput(
								() =>
								{
									ResultPattern.Name = this.Name;
									Controller.Instance.CurrentSavegame.DesignPatterns[Pattern.Index].CopyFrom(ResultPattern);
									_IsFinished = true;
								},
								() =>
								{
									_IsFinished = true;
								}
							);
						},
						() =>
						{
							_IsFinished = true;
						}
					);
				}
			});
			return true;
		});
	}

	public void Abort()
	{
		throw new NotImplementedException();
	}

	public DesignPattern GetPattern()
	{
		return this.Pattern;
	}

	public void SetName(string name)
	{
		this.Name = name;
	}
}