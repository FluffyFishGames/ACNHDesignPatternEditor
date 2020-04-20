using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using MyHorizons.Data;

public class PatternEditor : MonoBehaviour
{
	public LoadingAnimation Loading;
	public ColorPalette ColorPalette;
	public RectTransform RightPanel;
	public MenuButton SaveButton;
	public MenuButton CancelButton;
	public Image PreviewImage;
	public GameObject QRCode;
	public TMPro.TextMeshProUGUI NameValue;
	public TMPro.TextMeshProUGUI UsernameValue;
	public TMPro.TextMeshProUGUI TownValue;
	public GameObject EaselObject;
	public Previews Previews;
	public RawImage Preview;


	[Header("Sections")]
	public ColorEditor ColorEditor;
	[Header("PatternSections")]
	public Button SubPattern;
	public GameObject ShirtFront;
	public GameObject ShirtBack;
	public GameObject ShirtLeftArm;
	public GameObject ShirtRightArm;
	public GameObject CapFront;
	public GameObject CapBack;
	public GameObject CapBrim;
	public GameObject HatTop;
	public GameObject HatMiddle;
	public GameObject HatBottom;

	[HideInInspector]
	public Pattern CurrentPattern;
	[HideInInspector]
	public UnityEngine.Color CurrentColor
	{
		get
		{
			return this.ColorPalette.GetSelectedColor();
		}
	}
	[HideInInspector]
	public int PixelSize;
	[HideInInspector]
	public Brush CurrentBrush;

	public TransformTool TransformTool;
	public Tools Tools;
	public Layers Layers;

	private CanvasGroup MyCanvasGroup;
	private bool IsShown = false;
	private float ShowPhase = 0f;
	private System.Action CancelAction;
	private System.Action ConfirmAction;
	private Dictionary<DesignPatternInformation.PartType, GameObject> PartIcons;
	
	private bool Initialized = false;

	public PixelGrid PixelGrid;

	public void SetSize(int width, int height)
	{
		try
		{
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Changing size of editor to " + width + "x" + height);

			PixelSize = 18;
			var maxSize = 670;
			if (maxSize < PixelSize * width)
				PixelSize = maxSize / width;
			if (maxSize < PixelSize * height)
				PixelSize = maxSize / height;
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] New pixel size: " + PixelSize);

			PixelGrid.SetSize(width, height, PixelSize);
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while changing grid size: " + e.ToString()); }
	}

	public void Hide()
	{
		try
		{
			if (IsShown)
			{
				IsShown = false;
				CurrentPattern.Dispose();
			}
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while hiding: " + e.ToString()); }
	}

	public void OnImageUpdated()
	{
		try
		{
			PixelGrid.UpdateImage();
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error on OnImageUpdated callback: " + e.ToString()); }
	}

	public void OnLayerSelected(int num, Layer layer)
	{
		try
		{
			if (layer is SmartObjectLayer smartObjectLayer)
			{
				Tools.SwitchToolset(Tools.Toolset.SmartObjectLayer);
			}
			else
			{
				Tools.SwitchToolset(Tools.Toolset.RasterLayer);
			}
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error on OnLayerSelected callback: " + e.ToString()); }
	}

	public void MovePreview(float deltaX, float deltaY)
	{
		try
		{
			Previews.AllPreviews[CurrentPattern.Type].Move(deltaX, deltaY);
			Previews.AllPreviews[CurrentPattern.Type].Render();
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while moving preview: " + e.ToString()); }
	}

	public void BrushPreviewUpdated()
	{
		try
		{
			Tools.BrushPreviewUpdated();
			PixelGrid.BrushPreviewUpdated();
		}

		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error on BrushPreviewUpdated callback: " + e.ToString()); }
	}

	private DesignPattern.TypeEnum Type;

	public void SetType(DesignPattern.TypeEnum type)
	{
		this.Type = type;
	}

	public void Show(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		try
		{
			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Showing pattern editor...");

			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Creating new brush...");
			this.CurrentBrush = new Brush() { Editor = this };
			if (pattern != null)
			{
				Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Adding pattern to editor.");
				this.CurrentPattern = new Pattern(this, pattern);
				this.CurrentPattern.Load();

				IsShown = true;
				ConfirmAction = confirm;
				CancelAction = cancel;

				Type = pattern.Type;
			}

			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Setting textures to previews.");
			Preview.texture = Previews.AllPreviews[Type].Camera.targetTexture;
			Previews.AllPreviews[Type].ResetPosition();
			Previews.AllPreviews[Type].Render();

			Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Updating tools state.");
			Tools.PatternChanged();
			Tools.BrushUpdated();
			Tools.SwitchTool(Tools.Tool.None);
			Tools.SwitchToolset(Tools.Toolset.RasterLayer);
			PixelGrid.PatternLoaded();
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while showing PatternEditor: " + e.ToString()); }
	}

	void OnEnable()
	{
		if (!Initialized)
			Initialize();
	}

	void Start()
	{
		if (!Initialized)
			Initialize();
	}

	public void OpenColorEditor(ColorPaletteButton color)
	{
		try
		{
			ColorPalette.EditColor(color);
			if (color != null)
				ColorEditor.Show(color);
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while opening color editor: " + e.ToString()); }
	}

	public void LayersChanged()
	{
		try
		{
			Layers.UpdateLayers();
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error on LayersChanged callback: " + e.ToString()); }
	}

	void Initialize()
	{
		Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Initializing PatternEditor...");
		try
		{
			Initialized = true;
			Loading.gameObject.SetActive(false);
			MyCanvasGroup = GetComponent<CanvasGroup>();

			CancelButton.OnClick += () =>
			{
				//Debug.Log("CANCEL " + CancelAction);
				CancelAction?.Invoke();
				this.CurrentPattern.Dispose();
			};

			SaveButton.OnClick += () =>
			{
				//Debug.Log("CANCEL " + ConfirmAction);
				ConfirmAction?.Invoke();
				this.CurrentPattern.Dispose();
			};

			SubPattern.onClick.AddListener(() =>
			{
				CurrentPattern.NextSubPattern();
			});

			PartIcons = new Dictionary<DesignPatternInformation.PartType, GameObject>()
			{
				{ DesignPatternInformation.PartType.ShirtFront, ShirtFront },
				{ DesignPatternInformation.PartType.ShirtBack, ShirtBack },
				{ DesignPatternInformation.PartType.ShirtLeftArm, ShirtLeftArm },
				{ DesignPatternInformation.PartType.ShirtRightArm, ShirtRightArm },
				{ DesignPatternInformation.PartType.CapFront, CapFront },
				{ DesignPatternInformation.PartType.CapBack, CapBack },
				{ DesignPatternInformation.PartType.CapBrim, CapBrim },
				{ DesignPatternInformation.PartType.HatTop, HatTop },
				{ DesignPatternInformation.PartType.HatMiddle, HatMiddle },
				{ DesignPatternInformation.PartType.HatBottom, HatBottom }
			};
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while initializing PatternEditor: " + e.ToString()); }
	}

	public void SubPatternChanged(DesignPatternInformation.DesignPatternPart part)
	{
		try
		{
			bool found = false;
			foreach (var kv in PartIcons)
			{
				kv.Value.SetActive(part.Type == kv.Key);
				if (part.Type == kv.Key)
					found = true;
			}
			SubPattern.gameObject.SetActive(found);
			PixelGrid.SubPatternChanged();
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error in SubPatternChanged callback: " + e.ToString()); }
	}

	public void SetCurrentColor(UnityEngine.Color color)
	{
	}

	public void ChangeCurrentColor(UnityEngine.Color color)
	{
		try
		{
			this.ColorPalette.SetSelectedColor(color);
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while changing active color: " + e.ToString()); }
	}

	private Tools.Tool TempTool;
	private bool TempToolSet;
	public ITool CurrentTool;

	public void ChangeTool(ITool tool)
	{
		try
		{
			if (CurrentTool != null)
				CurrentTool.Destroyed();
			CurrentTool = tool;
			if (CurrentTool != null)
				CurrentTool.SetEditor(this);
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while changing tool: " + e.ToString()); }
	}

	// Update is called once per frame
	void Update()
    {
		try
		{
			if (CurrentPattern != null)
			{
				PreviewImage.sprite = CurrentPattern.GetPreviewSprite();

				if (CurrentPattern.Update())
				{
					PreviewImage.sprite = CurrentPattern.GetPreviewSprite();
					Previews.AllPreviews[CurrentPattern.Type].SetTexture(CurrentPattern.GetUpscaledPreview());
				}
				if (Tools.IsToolActive(Tools.Tool.ColorPicker))
				{
					if (Input.GetKey(KeyCode.LeftAlt))
					{
						if (!TempToolSet)
						{
							TempToolSet = true;
							TempTool = Tools.CurrentTool;
							Tools.SwitchTool(Tools.Tool.ColorPicker);
						}
					}
					else
					{
						if (TempToolSet)
						{
							TempToolSet = false;
							Tools.SwitchTool(TempTool);
							TempTool = Tools.Tool.None;
						}
					}
				}
			}

			if (IsShown && ShowPhase < 1f)
				ShowPhase = Mathf.Min(1f, ShowPhase + 1f);
			if (!IsShown && ShowPhase > 0f)
				ShowPhase = Mathf.Max(0f, ShowPhase - Time.deltaTime * 4f);

			var currentColor = ColorPalette.GetSelectedColor();
			MyCanvasGroup.alpha = ShowPhase;
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while updating PatternEditor: " + e.ToString()); }
	}

	public DesignPattern Save()
	{
		try
		{
			var pattern = new DesignPattern
			{
				Type = Type,
				IsPro = Type != DesignPattern.TypeEnum.SimplePattern
			};
			pattern.FromBitmap(this.CurrentPattern.PreviewBitmap);

			return pattern;
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while saving pattern: " + e.ToString()); return null; }
	}

	private void OnDestroy()
	{
		if (CurrentPattern != null)
			CurrentPattern.Dispose();
	}

	private void OnApplicationQuit()
	{
		if (CurrentPattern != null)
			CurrentPattern.Dispose();
	}
}
