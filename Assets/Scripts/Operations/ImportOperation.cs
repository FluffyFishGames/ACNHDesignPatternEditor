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
using WebPWrapper;
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

	public void SetBytes(byte[] bytes)
	{
		var qrCode = new ACFileFormat(bytes);
		IsQRCode = true;
		this.Image = qrCode.GetImage();
		this.Name = qrCode.Name;
		this.Username = qrCode.Username;
		this.Town = qrCode.TownName;
		this.Result = this.Image;
		this.IsReady = true;
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
			var resultBytes = new byte[result.RawBytes.Length - 2];
			for (int i = 5; i < result.RawBytes.Length * 2; i++)
			{
				int byteIndex = (i - 5) / 2;
				int rawByteIndex = i / 2;
				byte value = 0;
				if (i % 2 == 0)
					value = (byte) ((result.RawBytes[rawByteIndex] & 0xF0) >> 4);
				else
					value = (byte) (result.RawBytes[rawByteIndex] & 0x0F);
				if ((i - 5) % 2 == 0)
					resultBytes[byteIndex] += (byte) (value << 4);
				else
					resultBytes[byteIndex] += value;
			}
			var bytes = resultBytes;
			if (bytes[0x69] == 0x09)
			{
				IsQRCode = true;
			}
			if (IsQRCode)
			{
				SetBytes(bytes);
			}
			else
				this.Image = image;
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

				var transparentPixels = new bool[bmp.Width * bmp.Height];
				for (var y = 0; y < bmp.Height; y++)
				{
					for (var x = 0; x < bmp.Width; x++)
					{
						transparentPixels[x + y * bmp.Width] = bmp.GetPixel(x, y).A != 255;
					}
				}
				var targetImage = ImageBuffer.QuantizeImage(bmp, quantizer, null, 15, 1);

				bmp.Dispose();
				bmp = new Bitmap(targetImage);
				for (var y = 0; y < bmp.Height; y++)
				{
					for (var x = 0; x < bmp.Width; x++)
					{
						if (transparentPixels[x + y * bmp.Width])
							bmp.SetPixel(x, y, System.Drawing.Color.FromArgb(0, 0, 0, 0));
					}
				}
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
			StandaloneFileBrowser.OpenFilePanelAsync("Import image", "", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif", "acnl", "webp" }) }, false, (path) =>
			{
				if (path.Length > 0)
				{
					try
					{
						if (path[0].EndsWith(".acnl"))
						{
							var bytes = System.IO.File.ReadAllBytes(path[0]);
							this.SetBytes(bytes);
						}
						else
						{
							Bitmap bmp = null;
							var imageStream = new System.IO.FileStream(path[0], System.IO.FileMode.Open, System.IO.FileAccess.Read);
							byte[] fourBytes = new byte[4];
							imageStream.Read(fourBytes, 0, 4);
							if (fourBytes[0] == 0x52 && fourBytes[1] == 0x49 && fourBytes[2] == 0x46 && fourBytes[3] == 0x46)
							{
								using (WebP webp = new WebP())
									bmp = webp.Load(path[0]);
								imageStream.Close();
								imageStream.Dispose();
							}
							else
							{
								bmp = new Bitmap(System.Drawing.Image.FromFile(path[0]));
								imageStream.Close();
								imageStream.Dispose();
							}
							this.SetImage(bmp);
						}
					}
					catch (System.IO.FileLoadException e)
					{
						Controller.Instance.Popup.SetText("Failed to load the file. File error.", false, () =>
						{
							return true;
						});
						return;
					}
					catch (Exception e)
					{
						Controller.Instance.Popup.SetText("Invalid image file.", false, () =>
						{
							return true;
						});
						return;
					}
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