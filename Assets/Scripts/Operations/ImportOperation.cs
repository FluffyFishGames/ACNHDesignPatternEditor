/*using MyHorizons.Data;
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
using ZXing.QrCode;

public class ImportOperation : IOperation, IPatternOperation, IChangeNameOperation
{
	private System.Drawing.Bitmap Image;
	private System.Drawing.Bitmap Result;
	private DesignPattern Pattern;
	private EditPattern EditPattern;
	private bool _IsParsing;
	private bool _IsReady;
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
	public DesignPattern.TypeEnum Type = DesignPattern.TypeEnum.SimplePattern;
	public ImportOperation(DesignPattern pattern)
	{
		this.Pattern = pattern;
		this.Name = this.Pattern.Name;
	}

	public void SetBytes(byte[] bytes)
	{
		var qrCode = new ACNLFileFormat(bytes);
		IsQRCode = true;
		this.Type = qrCode.Type;
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
				PossibleFormats = new List<BarcodeFormat>() { BarcodeFormat.QR_CODE }
			}
		};


		var result = scanner.Decode(image);
		if (result != null)
		{
			var n = (List<byte[]>) result.ResultMetadata[ResultMetadataType.BYTE_SEGMENTS];
			if (n.Count >= 1)
			{
				var bytes = n[0];
				IsQRCode = true;
				SetBytes(bytes);
			}
			else this.Image = image;
		}
		else
			this.Image = image;
	}

	public void ParsePattern(ICrop crop, ISampling sampling, IColorQuantizer quantizer, IColorCache colorCache)
	{

	}

	public EditPattern GetEditPattern()
	{
		if (IsReady)
		{
			if (EditPattern != null)
				EditPattern.Dispose();
			EditPattern = new EditPattern(this.Result);
			EditPattern.Pattern.Name = Pattern.Name;
			EditPattern.Pattern.PersonalID = new MyHorizons.Data.PlayerData.PersonalID() { UniqueId = 0xFFFFFFFF, TownId = Pattern.PersonalID.TownId };
			EditPattern.Pattern.PersonalID.SetName(Pattern.PersonalID.GetName());
			IsReady = false;
		}
		return EditPattern;
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
			StandaloneFileBrowser.OpenFilePanelAsync("Import image", "", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif", "acnl" }) }, false, (path) =>
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
						_IsFinished = true;
						return;
					}
					catch (Exception e)
					{
						Controller.Instance.Popup.SetText("Invalid image file.", false, () =>
						{
							return true;
						});
						_IsFinished = true;
						return;
					}
					Debug.Log(Type);
					if (IsQRCode && Pattern.IsPro && Type == DesignPattern.TypeEnum.SimplePattern)
					{
						Debug.Log("Pro Pattern in simple");
						Controller.Instance.Popup.SetText("Invalid pattern type. You can't put a simple design on a pro design spot.", false, () =>
						{
							return true;
						});
						_IsFinished = true;
						return;
					}
					if (IsQRCode && !Pattern.IsPro && Type != DesignPattern.TypeEnum.SimplePattern)
					{
						Debug.Log("Simple Pattern in pro");
						Controller.Instance.Popup.SetText("Invalid pattern type. You can't put a pro design on a simple design spot.", false, () =>
						{
							return true;
						});
						_IsFinished = true;
						return;
					}
					if (Type == DesignPattern.TypeEnum.Unsupported)
					{
						Debug.Log("Unsupported");
						Controller.Instance.Popup.SetText("Unsupported pattern type.", false, () =>
						{
							return true;
						});
						_IsFinished = true;
						return;
					}

					Controller.Instance.PatternSelector.ActionMenu.Close();
					if (Pattern.IsPro && !IsQRCode)
					{
						Controller.Instance.SwitchToClothSelector(
							(type) =>
							{
								Type = type;
								Controller.Instance.SwitchToPatternEditor(
									Pattern,
									() =>
									{
										Controller.Instance.SwitchToNameInput(
											() =>
											{
												EditPattern.Pattern.Name = this.Name;
												Controller.Instance.CurrentSavegame.DesignPatterns[Pattern.Index].CopyFrom(EditPattern.Pattern);
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
							},
							() =>
							{
								_IsFinished = true;
							}
						);
					}
					else
					{
						Controller.Instance.SwitchToPatternEditor(
							Pattern,
							() =>
							{
								Controller.Instance.SwitchToNameInput(
									() =>
									{
										EditPattern.Pattern.Name = this.Name;
										Controller.Instance.CurrentSavegame.DesignPatterns[Pattern.Index].CopyFrom(EditPattern.Pattern);
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
}*/