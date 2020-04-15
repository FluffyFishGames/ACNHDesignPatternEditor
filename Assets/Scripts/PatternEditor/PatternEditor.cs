using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using MyHorizons.Data;
using UnityEngine.EventSystems;
using SFB;

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
	private int CurrentSubPattern = 0;
	private int Width;
	private int Height;
	private Dictionary<DesignPatternInformation.PartType, GameObject> PartIcons;
	
	private bool Initialized = false;
	private float[] BrushShape;

	public PixelGrid PixelGrid;

	public void SetSize(int width, int height)
	{
		Width = width;
		Height = height;

		PixelSize = 18;
		var maxSize = 670;
		if (maxSize < PixelSize * width)
			PixelSize = maxSize / width;
		if (maxSize < PixelSize * height)
			PixelSize = maxSize / height;

		PixelGrid.SetSize(width, height, PixelSize);
	}

	public void Hide()
	{
		if (IsShown)
		{
			IsShown = false;
			CurrentPattern.Dispose();
		}
	}

	public void OnImageUpdated()
	{
		PixelGrid.UpdateImage();
	}

	public void OnLayerSelected(int num, Layer layer)
	{
		if (layer is SmartObjectLayer smartObjectLayer)
		{
			//TransformTool.Transform(smartObjectLayer);
			Tools.SwitchToolset(Tools.Toolset.SmartObjectLayer);
			//Tools.SwitchTool(Tools.Tool.Transform);
		}
		else
		{
			Tools.SwitchToolset(Tools.Toolset.RasterLayer);
			//TransformTool.Hide();
		}
	}

	public void MovePreview(float delta)
	{
		Previews.AllPreviews[CurrentPattern.Type].Move(delta);
		Previews.AllPreviews[CurrentPattern.Type].Render();
	}

	public void BrushPreviewUpdated()
	{
		Tools.BrushPreviewUpdated();
		PixelGrid.BrushPreviewUpdated();
	}

	private DesignPattern.TypeEnum Type;

	public void SetType(DesignPattern.TypeEnum type)
	{
		this.Type = type;
	}

	public void Show(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		this.CurrentBrush = new Brush() { Editor = this };
		if (pattern != null)
		{
			this.CurrentPattern = new Pattern(this, pattern);
			this.CurrentPattern.Load();

			IsShown = true;
			ConfirmAction = confirm;
			CancelAction = cancel;

			Type = pattern.Type;
		}

		Preview.texture = Previews.AllPreviews[Type].Camera.targetTexture;
		Previews.AllPreviews[Type].ResetPosition();
		Previews.AllPreviews[Type].Render();

		Tools.PatternChanged();
		Tools.BrushUpdated();
		Tools.SwitchTool(Tools.Tool.None);
		Tools.SwitchToolset(Tools.Toolset.RasterLayer);
		PixelGrid.PatternLoaded();
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
		ColorPalette.EditColor(color);
		if (color != null)
			ColorEditor.Show(color);
	}

	public void LayersChanged()
	{
		Layers.UpdateLayers();
	}

	void Initialize()
	{
		Initialized = true;
		Loading.gameObject.SetActive(false);
		MyCanvasGroup = GetComponent<CanvasGroup>();

		CancelButton.OnClick += () => {
			//Debug.Log("CANCEL " + CancelAction);
			CancelAction?.Invoke();
		};

		SaveButton.OnClick += () => {
			//Debug.Log("CANCEL " + ConfirmAction);
			ConfirmAction?.Invoke();
		};

		SubPattern.onClick.AddListener(() => {
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

	
	void DeleteSelectedLayer()
	{

	}

	public void SubPatternChanged(DesignPatternInformation.DesignPatternPart part)
	{
		foreach (var kv in PartIcons)
			kv.Value.SetActive(part.Type == kv.Key);
		PixelGrid.SubPatternChanged();
	}

	public void SetCurrentColor(UnityEngine.Color color)
	{
	}

	public void ChangeCurrentColor(UnityEngine.Color color)
	{
		this.ColorPalette.SetSelectedColor(color);
	}

	private int lastBrushX = -1;
	private int lastBrushY = -1;

	private Tools.Tool TempTool;
	private bool TempToolSet;
	public ITool CurrentTool;

	public void ChangeTool(ITool tool)
	{
		if (CurrentTool != null)
			CurrentTool.Destroyed();
		CurrentTool = tool;
		if (CurrentTool != null)
			CurrentTool.SetEditor(this);
	}

	// Update is called once per frame
	void Update()
    {
		if (CurrentPattern != null)
		{
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

	public DesignPattern Save()
	{
		var pattern = new DesignPattern();
		pattern.Type = Type;
		pattern.IsPro = Type != DesignPattern.TypeEnum.SimplePattern;
		pattern.FromTexture(this.CurrentPattern.GetPreviewSprite().texture);
		return pattern;
	}
}
