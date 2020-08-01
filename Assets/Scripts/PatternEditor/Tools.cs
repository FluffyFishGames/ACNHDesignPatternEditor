using SFB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Tools : MonoBehaviour
{
	public int BucketFillTolerance;
	public PatternEditor Editor;

	public enum Toolset
	{
		RasterLayer,
		SmartObjectLayer
	}

	public enum Tool
	{
		None,
		Brush,
		ColorPicker,
		BucketFill,
		Rect,
		LineRect,
		LineCircle,
		FullRect,
		FullCircle,
		Line,
		//Shape,
		//Quantize,
		//Import,
		//Select,
		Transform,
		Eraser,
		CropNone,
		CropFit,
		CropCover,
		SamplingBox,
		SamplingLanczos8,
		SamplingMitchell,
		SamplingCosine,
		SamplingCubicConvolution
	};

	public enum Tab
	{
		Tools,
		Quantization,
		Protocol,
		File
	};

	[Header("Tabs")]
	public Tab CurrentTab;
	public Image ToolsTab;
	public EventTrigger ToolsTabHandler;
	public Image ToolsTabIcon;
	public Image QuantizationTab;
	public EventTrigger QuantizationTabHandler;
	public Image QuantizationTabIcon;
	public Image ProtocolTab;
	public EventTrigger ProtocolTabHandler;
	public Image ProtocolTabIcon;
	public Image FileTab;
	public EventTrigger FileTabHandler;
	public Image FileTabIcon;

	[Header("File")]
	public MenuButton SaveProjectButton;
	public MenuButton LoadProjectButton;
	public MenuButton ChangeTypeButton;
	public MenuButton ClearButton;

	[Header("Protocol")]
	public Transform ProtocolContainer;
	public GameObject ProtocolEntryPrefab;

	[Header("General")]
	public Tool CurrentTool;
	public Toolset CurrentToolset;
	public GameObject ToolButtonGroupPrefab;
	public GameObject ToolsPanel;
	public GameObject ProtocolPanel;
	public GameObject QuantizationPanel;
	public GameObject FilePanel;

	[Header("Brush")]
	public EventTrigger Brush;
	public Pop BrushOptions;
	public Slider BrushSize;
	public Slider BrushHardness;
	public Image BrushPreview;
	public RectTransform BrushContainer;

	/*public RectTransform Back;
	public Button BackButton;*/

	[Header("Eraser")]
	public EventTrigger Eraser;

	[Header("Color Picker")]
	public EventTrigger ColorPicker;

	[Header("Bucket Fill")]
	public EventTrigger BucketFill;
	public Pop BucketFillOptions;
	public Slider BucketFillToleranceSlider;
	public TMPro.TextMeshProUGUI BucketFillToleranceText;

	[Header("Rect")]
	public EventTrigger Rect;

	public Pop RectOptions;
	public EventTrigger LineRect;
	public EventTrigger LineCircle;
	public EventTrigger FullRect;
	public EventTrigger FullCircle;

	[Header("Line")]
	public EventTrigger Line;

	[Header("Shape")]
	public EventTrigger Shape;

	[Header("Quantize")]
	public EventTrigger Quantize;
	public RectTransform QuantizeOptions;
	public GameObject QuantizeTools;
	public EventTrigger Select;

	[Header("Transform")]
	public EventTrigger Transform;
	public Pop TransformOptions;
	public EventTrigger CropNone;
	public EventTrigger CropCover;
	public EventTrigger CropFit;
	public EventTrigger SamplingBox;
	public EventTrigger SamplingLanczos8;
	public EventTrigger SamplingMitchell;
	public EventTrigger SamplingCosine;
	public EventTrigger SamplingCubicConvolution;

	public TransformToolContainer TransformTool;

	private ToolButtonGroup QuantizationGroup;
	private ToolButtonGroup ColorCacheGroup;
//	private ToolButtonGroup ResamplingGroup;
//	private ToolButtonGroup CropGroup;

	private bool Initialized = false;

	private Dictionary<Tool, Image> ToolImages;
	private readonly List<Tool> ActiveTools = new List<Tool>();
	private History CurrentHistory;

	public void PatternChanged()
	{
		SwitchTab(Tab.Tools);
		ChangeTypeButton.gameObject.SetActive(Editor.CurrentPattern.Type != DesignPattern.TypeEnum.SimplePattern);
		LayoutRebuilder.ForceRebuildLayoutImmediate(ChangeTypeButton.transform.parent.GetComponent<RectTransform>());
	}

	public void SwitchTab(Tab tab)
	{
		var lightColor            = new UnityEngine.Color(243f / 255f, 183f / 255f, 196f / 255f);
		var darkColor             = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f);
		ToolsTab.color            = tab == Tab.Tools ? lightColor : darkColor;
		ToolsTabIcon.color        = tab == Tab.Tools ? darkColor : lightColor;
		QuantizationTab.color     = tab == Tab.Quantization ? lightColor : darkColor;
		QuantizationTabIcon.color = tab == Tab.Quantization ? darkColor : lightColor;
		ProtocolTab.color         = tab == Tab.Protocol ? lightColor : darkColor;
		ProtocolTabIcon.color     = tab == Tab.Protocol ? darkColor : lightColor;
		FileTab.color             = tab == Tab.File ? lightColor : darkColor;
		FileTabIcon.color         = tab == Tab.File ? darkColor : lightColor;
		ToolsPanel.SetActive(tab == Tab.Tools);
		QuantizationPanel.SetActive(tab == Tab.Quantization);
		ProtocolPanel.SetActive(tab == Tab.Protocol);
		FilePanel.SetActive(tab == Tab.File);

		CurrentTab = tab;

		if (tab != Tab.Tools)
			SwitchTool(Tool.None);
		ForceRebuildLayout();
		//243, 183, 196
		//212, 135, 155
	}

	public bool IsToolActive(Tool tool)
	{
		return ActiveTools.Contains(tool);
	}

	public void HistoryChanged(History history)
	{
		CurrentHistory = history;
		for (int i = ProtocolContainer.childCount - 1; i >= history.Events.Count; i--)
		{
			Destroy(ProtocolContainer.GetChild(i).gameObject);
		}

		for (int i = ProtocolContainer.childCount; i < history.Events.Count; i++)
			GameObject.Instantiate(ProtocolEntryPrefab, ProtocolContainer);

		for (int i = 0; i < history.Events.Count; i++)
		{
			var entry = ProtocolContainer.GetChild(i).GetComponent<ProtocolEntry>();
			entry.SetHighlighted(history.CurrentEvent == i);
			entry.Text.text = history.Events[i].Name;
			int num = i;
			entry.OnClick = () =>
			{
				history.RestoreTo(num);
			};
		}

		LayoutRebuilder.ForceRebuildLayoutImmediate(ProtocolContainer.GetComponent<RectTransform>());
	}

	public void SwitchTool(Tool tool)
	{
		if (tool == Tool.CropNone)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				TextureBitmap.CropMode old = sml.GetCrop();
				if (old != TextureBitmap.CropMode.Scale)
				{
					sml.ChangeCrop(TextureBitmap.CropMode.Scale);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed crop", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), old, sml.GetResampling(), TextureBitmap.CropMode.Scale, sml.GetResampling()));
				}
			}
			return;
		}
		if (tool == Tool.CropFit)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				TextureBitmap.CropMode old = sml.GetCrop();
				if (old != TextureBitmap.CropMode.Fit)
				{
					sml.ChangeCrop(TextureBitmap.CropMode.Fit);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed crop", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), old, sml.GetResampling(), TextureBitmap.CropMode.Fit, sml.GetResampling()));
				}
			}
			return;
		}
		if (tool == Tool.CropCover)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				TextureBitmap.CropMode old = sml.GetCrop();
				if (old != TextureBitmap.CropMode.Cover)
				{
					sml.ChangeCrop(TextureBitmap.CropMode.Cover);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed crop", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), old, sml.GetResampling(), TextureBitmap.CropMode.Cover, sml.GetResampling()));
				}
			}
			return;
		}
		if (tool == Tool.SamplingBox)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				ResamplingFilters old = sml.GetResampling();
				if (old != ResamplingFilters.Box)
				{
					sml.ChangeResampling(ResamplingFilters.Box);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed sampling", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), sml.GetCrop(), old, sml.GetCrop(), ResamplingFilters.Box));
				}
			}
			return;
		}
		if (tool == Tool.SamplingLanczos8)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				ResamplingFilters old = sml.GetResampling();
				if (old != ResamplingFilters.Lanczos8)
				{
					sml.ChangeResampling(ResamplingFilters.Lanczos8);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed sampling", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), sml.GetCrop(), old, sml.GetCrop(), ResamplingFilters.Lanczos8));
				}
			}
			return;
		}
		if (tool == Tool.SamplingMitchell)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				ResamplingFilters old = sml.GetResampling();
				if (old != ResamplingFilters.Mitchell)
				{
					sml.ChangeResampling(ResamplingFilters.Mitchell);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed sampling", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), sml.GetCrop(), old, sml.GetCrop(), ResamplingFilters.Mitchell));
				}
			}
			return;
		}
		if (tool == Tool.SamplingCosine)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				ResamplingFilters old = sml.GetResampling();
				if (old != ResamplingFilters.Cosine)
				{
					sml.ChangeResampling(ResamplingFilters.Cosine);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed sampling", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), sml.GetCrop(), old, sml.GetCrop(), ResamplingFilters.Cosine));
				}
			}
			return;
		}
		if (tool == Tool.SamplingCubicConvolution)
		{
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer sml)
			{
				ResamplingFilters old = sml.GetResampling();
				if (old != ResamplingFilters.CubicConvolution)
				{
					sml.ChangeResampling(ResamplingFilters.CubicConvolution);
					sml.UpdateColors();
					sml.UpdateTexture();
					UpdateTransformSettings(sml);
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.TransformChangeCropResampling("Changed sampling", Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(sml), sml.GetCrop(), old, sml.GetCrop(), ResamplingFilters.CubicConvolution));
				}
			}
			return;
		}

		if (CurrentTool != Tool.None)
		{
			ToolImages[CurrentTool].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f, ActiveTools.Contains(CurrentTool) ? 1f : 0.5f);
			if ((CurrentTool == Tool.LineRect || CurrentTool == Tool.LineCircle || CurrentTool == Tool.FullRect || CurrentTool == Tool.FullCircle) && 
				(tool != Tool.LineRect && tool != Tool.LineCircle && tool != Tool.FullRect && tool != Tool.FullCircle))
				ToolImages[Tool.Rect].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f, ActiveTools.Contains(Tool.Rect) ? 1f : 0.5f);
		}
		if (tool == Tool.Rect)
		{
			tool = Tool.LineRect;
			ToolImages[Tool.Rect].color       = new UnityEngine.Color(98f / 255f, 80f / 255f, 66f / 255f);
			ToolImages[Tool.LineRect].color   = new UnityEngine.Color(98f / 255f, 80f / 255f, 66f / 255f);
			ToolImages[Tool.LineCircle].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f);
			ToolImages[Tool.FullRect].color   = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f);
			ToolImages[Tool.FullCircle].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f);
		}
		CurrentTool = tool;
		if (ToolImages.ContainsKey(CurrentTool))
			ToolImages[CurrentTool].color = new UnityEngine.Color(98f / 255f, 80f / 255f, 66f / 255f);

		if (CurrentTool == Tool.Brush || CurrentTool == Tool.Eraser)
		{
			if (CurrentTool == Tool.Brush) Editor.ChangeTool(new BrushTool());
			if (CurrentTool == Tool.Eraser) Editor.ChangeTool(new EraseTool());

			BrushContainer.SetParent(BrushOptions.transform);
			BrushContainer.anchoredPosition = new Vector2(0f, 0f);
			BrushContainer.localScale = new Vector3(1f, 1f, 1f);

			BrushOptions.PopUp();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			TransformOptions.PopOut();

			Editor.CurrentBrush.Size = (int) BrushSize.value;
			Editor.CurrentBrush.Hardness = (int) BrushHardness.value;
			Editor.CurrentBrush.RecalculateBrush();
		}

		if (CurrentTool == Tool.ColorPicker)
		{
			Editor.ChangeTool(new ColorPickerTool());

			BrushOptions.PopOut();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			TransformOptions.PopOut();

			Editor.CurrentBrush.Size = 1;
			Editor.CurrentBrush.RecalculateBrush();
		}

		if (CurrentTool == Tool.BucketFill)
		{
			Editor.ChangeTool(new BucketFillTool());

			BrushOptions.PopOut();
			BucketFillOptions.PopUp();
			RectOptions.PopOut();
			TransformOptions.PopOut();

			Editor.CurrentBrush.Size = 1;
			Editor.CurrentBrush.RecalculateBrush();
		}

		if (CurrentTool == Tool.LineRect || CurrentTool == Tool.LineCircle || CurrentTool == Tool.FullRect || CurrentTool == Tool.FullCircle)
		{
			Editor.ChangeTool(new RectTool());

			BrushContainer.SetParent(RectOptions.transform);
			BrushContainer.anchoredPosition = new Vector2(0f, 0f);
			BrushContainer.localScale = new Vector3(1f, 1f, 1f);

			BrushOptions.PopOut();
			BucketFillOptions.PopOut();
			RectOptions.PopUp();
			TransformOptions.PopOut();

			if (CurrentTool == Tool.FullRect || CurrentTool == Tool.FullCircle)
				Editor.CurrentBrush.Size = 1;
			else
			{
				Editor.CurrentBrush.Size = (int) BrushSize.value;
				Editor.CurrentBrush.Hardness = (int) BrushHardness.value;
			}

			Editor.CurrentBrush.RecalculateBrush();
		}

		if (CurrentTool == Tool.Line)
		{
			Editor.ChangeTool(new LineTool());

			BrushContainer.SetParent(BrushOptions.transform);
			BrushContainer.anchoredPosition = new Vector2(0f, 0f);
			BrushContainer.localScale = new Vector3(1f, 1f, 1f);

			BrushOptions.PopUp();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			TransformOptions.PopOut();

			Editor.CurrentBrush.Size = (int) BrushSize.value;
			Editor.CurrentBrush.Hardness = (int) BrushHardness.value;
			Editor.CurrentBrush.RecalculateBrush();
		}

		if (CurrentTool == Tool.Transform)
		{
			Editor.ChangeTool(new TransformTool());

			BrushOptions.PopOut();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			TransformOptions.PopUp();
		}

		if (CurrentTool == Tool.None)
		{
			Editor.ChangeTool(null);

			BrushOptions.PopOut();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			TransformOptions.PopOut();
		}
		/*

		AddEvent(Line, new EventTrigger[] { Brush, Eraser, ColorPicker, BucketFill, Rect, Line, Shape, Quantize, Import, Select, Transform }, (evtData) => {
			CurrentTool = Tool.Line;
			BrushContainer.SetParent(BrushOptions.transform);
			BrushContainer.anchoredPosition = new Vector2(0f, 0f);
BrushContainer.localScale = new Vector3(1f, 1f, 1f);

BrushOptions.PopUp();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			RecalculateBrush();
		}, "Line");

		AddEvent(Quantize, new EventTrigger[] { Brush, Eraser, ColorPicker, BucketFill, Rect, Line, Shape, Quantize, Import, Select, Transform }, (evtData) => {
			CurrentTool = Tool.Quantize;
			CurrentTools = QuantizeOptions;
			QuantizeOptions.gameObject.SetActive(true);
			Back.gameObject.SetActive(true);

			ShowTools = true;

			BrushOptions.PopOut();
			BucketFillOptions.PopOut();
			RectOptions.PopOut();
			ForceRebuildLayout();*/
	}

	void UpdateTransformSettings(SmartObjectLayer layer)
	{
		CropNone.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		CropFit.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		CropCover.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		SamplingBox.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		SamplingLanczos8.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		SamplingMitchell.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		SamplingCosine.GetComponent<CanvasRenderer>().SetAlpha(0.5f);
		SamplingCubicConvolution.GetComponent<CanvasRenderer>().SetAlpha(0.5f);

		if (layer.Crop == TextureBitmap.CropMode.Scale)
			CropNone.GetComponent<CanvasRenderer>().SetAlpha(1f);
		else if (layer.Crop == TextureBitmap.CropMode.Fit)
			CropFit.GetComponent<CanvasRenderer>().SetAlpha(1f);
		else if (layer.Crop == TextureBitmap.CropMode.Cover)
			CropCover.GetComponent<CanvasRenderer>().SetAlpha(1f);
		if (layer.Resampler == ResamplingFilters.Box)
			SamplingBox.GetComponent<CanvasRenderer>().SetAlpha(1f);
		if (layer.Resampler == ResamplingFilters.Lanczos8)
			SamplingLanczos8.GetComponent<CanvasRenderer>().SetAlpha(1f);
		if (layer.Resampler == ResamplingFilters.Mitchell)
			SamplingMitchell.GetComponent<CanvasRenderer>().SetAlpha(1f);
		if (layer.Resampler == ResamplingFilters.CubicConvolution)
			SamplingCubicConvolution.GetComponent<CanvasRenderer>().SetAlpha(1f);
		if (layer.Resampler == ResamplingFilters.Cosine)
			SamplingCosine.GetComponent<CanvasRenderer>().SetAlpha(1f);
	}

	public void SwitchToolset(Toolset toolset)
	{
		CurrentToolset = toolset;
		if (CurrentToolset == Toolset.RasterLayer)
		{
			ActivateTool(Tool.Brush);
			ActivateTool(Tool.BucketFill);
			ActivateTool(Tool.ColorPicker);
			ActivateTool(Tool.Eraser);
			ActivateTool(Tool.LineRect);
			ActivateTool(Tool.LineCircle);
			ActivateTool(Tool.FullRect);
			ActivateTool(Tool.FullCircle);
			//ActivateTool(Tool.Import);
			ActivateTool(Tool.Line);
			//ActivateTool(Tool.Quantize);
			ActivateTool(Tool.Rect);
			//ActivateTool(Tool.Select);
			//ActivateTool(Tool.Shape);
			//DeactivateTool(Tool.Transform);
			DeactivateTool(Tool.Transform);
			if (CurrentTool == Tool.Transform)
				SwitchTool(Tool.Brush);
		}
		else if (CurrentToolset == Toolset.SmartObjectLayer)
		{
			DeactivateTool(Tool.Brush);
			DeactivateTool(Tool.BucketFill);
			DeactivateTool(Tool.ColorPicker);
			DeactivateTool(Tool.Eraser);
			DeactivateTool(Tool.LineRect);
			DeactivateTool(Tool.LineCircle);
			DeactivateTool(Tool.FullRect);
			DeactivateTool(Tool.FullCircle);
			//DeactivateTool(Tool.Import);
			DeactivateTool(Tool.Line);
			//DeactivateTool(Tool.Quantize);
			DeactivateTool(Tool.Rect);
			//DeactivateTool(Tool.Select);
			//DeactivateTool(Tool.Shape);
			ActivateTool(Tool.Transform);
			ActivateTool(Tool.CropNone);
			ActivateTool(Tool.CropFit);
			ActivateTool(Tool.CropCover);
			ActivateTool(Tool.SamplingMitchell);
			ActivateTool(Tool.SamplingLanczos8);
			ActivateTool(Tool.SamplingBox);
			ActivateTool(Tool.SamplingCubicConvolution);
			ActivateTool(Tool.SamplingCosine);
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer layer)
				UpdateTransformSettings(layer);
			SwitchTool(Tool.Transform);
		}
	}

	void ActivateTool(Tool tool)
	{
		if (!ActiveTools.Contains(tool))
		{
			ActiveTools.Add(tool);
		}
		if (tool != Tool.SamplingMitchell && tool != Tool.SamplingLanczos8 && tool != Tool.SamplingBox && tool != Tool.CropNone && tool != Tool.CropFit && tool != Tool.CropCover)
			ToolImages[tool].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f, 1f);
	}

	void DeactivateTool(Tool tool)
	{
		if (ActiveTools.Contains(tool))
		{
			ActiveTools.Remove(tool);
		}
		if (tool != Tool.SamplingMitchell && tool != Tool.SamplingLanczos8 && tool != Tool.SamplingBox && tool != Tool.CropNone && tool != Tool.CropFit && tool != Tool.CropCover)
			ToolImages[tool].color = new UnityEngine.Color(212f / 255f, 135f / 255f, 155f / 255f, 0.1f);
	}

	void AddEvent(EventTrigger trigger, Tool tool)
	{
		var onClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
		onClick.callback.AddListener((evtData) => {
			if (ActiveTools.Contains(tool))
			{
				SwitchTool(tool);
			}
		});
		trigger.triggers.Add(onClick);
	}

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}

	void ForceRebuildLayout()
	{
		/*LayoutRebuilder.ForceRebuildLayoutImmediate(CropGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(ResamplingGroup.GetComponent<RectTransform>());*/
		LayoutRebuilder.ForceRebuildLayoutImmediate(QuantizationGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(ColorCacheGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(QuantizeTools.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(QuantizationPanel.GetComponent<RectTransform>());
	}

	public void BrushUpdated()
	{
		this.BrushHardness.value = Editor.CurrentBrush.Hardness;
		this.BrushSize.value = Editor.CurrentBrush.Size;
	}

	public void BrushPreviewUpdated()
	{
		this.BrushPreview.sprite = Editor.CurrentBrush.BrushSprite;
		this.BrushPreview.GetComponent<RectTransform>().sizeDelta = new Vector2(Editor.CurrentBrush.BrushSprite.texture.width * 4, Editor.CurrentBrush.BrushSprite.texture.height * 4);
	}

	public void Unloaded()
	{
		for (int i = ProtocolContainer.childCount - 1; i >= 0; i--)
		{
			Destroy(ProtocolContainer.GetChild(i).gameObject);
		}
	}

	public void Undo()
	{
		if (CurrentHistory != null && CurrentHistory.CurrentEvent > 0)
		{
			CurrentHistory.RestoreTo(CurrentHistory.CurrentEvent - 1);
		}
	}

	public void Redo()
	{
		if (CurrentHistory != null && CurrentHistory.CurrentEvent < CurrentHistory.Events.Count - 1)
		{
			CurrentHistory.RestoreTo(CurrentHistory.CurrentEvent + 1);
		}
	}


	private void Initialize()
	{
		if (Initialized) return;
		Initialized = true;

		ToolImages = new Dictionary<Tool, Image>()
		{
			{ Tool.Brush,       Brush.GetComponent<Image>() },
			{ Tool.BucketFill,  BucketFill.GetComponent<Image>() },
			{ Tool.LineCircle,  LineCircle.GetComponent<Image>() },
			{ Tool.ColorPicker, ColorPicker.GetComponent<Image>() },
			{ Tool.Eraser,      Eraser.GetComponent<Image>() },
			{ Tool.FullCircle,  FullCircle.GetComponent<Image>() },
			{ Tool.FullRect,    FullRect.GetComponent<Image>() },
			//{ Tool.Import,      Import.GetComponent<Image>() },
			{ Tool.Line,        Line.GetComponent<Image>() },
			//{ Tool.Quantize,    Quantize.GetComponent<Image>() },
			{ Tool.Rect,        Rect.GetComponent<Image>() },
			{ Tool.LineRect,    LineRect.GetComponent<Image>() },
			//{ Tool.Select,      Select.GetComponent<Image>() },
			//{ Tool.Shape,       Shape.GetComponent<Image>() },
			{ Tool.Transform,   Transform.GetComponent<Image>() },
			{ Tool.CropNone,    CropNone.GetComponent<Image>() },
			{ Tool.CropFit,     CropFit.GetComponent<Image>() },
			{ Tool.CropCover,   CropCover.GetComponent<Image>() },
			{ Tool.SamplingBox, SamplingBox.GetComponent<Image>() },
			{ Tool.SamplingLanczos8, SamplingLanczos8.GetComponent<Image>() },
			{ Tool.SamplingMitchell, SamplingMitchell.GetComponent<Image>() },
			{ Tool.SamplingCosine, SamplingCosine.GetComponent<Image>() },
			{ Tool.SamplingCubicConvolution, SamplingCubicConvolution.GetComponent<Image>() },
		};


		var quantization = GameObject.Instantiate(ToolButtonGroupPrefab, QuantizeTools.transform);
		QuantizationGroup = quantization.GetComponent<ToolButtonGroup>();
		QuantizationGroup.Title.text = "Quantization algorithm";
		QuantizationGroup.SetItems(new (string, System.Action)[] {
			("Xiaolin Wu",   () => { Editor.CurrentPattern.ChangeQuantizer(0); }),
			("HSL Distinct", () => { Editor.CurrentPattern.ChangeQuantizer(1); }),
			("Popularity",   () => { Editor.CurrentPattern.ChangeQuantizer(2); }),
			("Median Cut",   () => { Editor.CurrentPattern.ChangeQuantizer(3); }),
			("Octree",       () => { Editor.CurrentPattern.ChangeQuantizer(4); })
		});

		var colorCache = GameObject.Instantiate(ToolButtonGroupPrefab, QuantizeTools.transform);
		ColorCacheGroup = colorCache.GetComponent<ToolButtonGroup>();
		ColorCacheGroup.Title.text = "Color cache";
		ColorCacheGroup.SetItems(new (string, System.Action)[] {
			("Euclidean distance", () => { Editor.CurrentPattern.ChangeColorCache(0); }),
			("Locally-sensitive",  () => { Editor.CurrentPattern.ChangeColorCache(1); }),
			("Octree search",      () => { Editor.CurrentPattern.ChangeColorCache(2); })
		});

		ForceRebuildLayout();

		AddEvent(Brush,       Tool.Brush);
		AddEvent(Eraser,      Tool.Eraser);
		AddEvent(ColorPicker, Tool.ColorPicker);
		AddEvent(BucketFill,  Tool.BucketFill);
		AddEvent(Rect,        Tool.Rect);
		AddEvent(LineRect,    Tool.LineRect);
		AddEvent(LineCircle,  Tool.LineCircle);
		AddEvent(FullRect,    Tool.FullRect);
		AddEvent(FullCircle,  Tool.FullCircle);
		AddEvent(Line,        Tool.Line);
		AddEvent(Transform,   Tool.Transform);
		AddEvent(CropNone,    Tool.CropNone);
		AddEvent(CropFit,     Tool.CropFit);
		AddEvent(CropCover,   Tool.CropCover);
		AddEvent(SamplingBox, Tool.SamplingBox);
		AddEvent(SamplingLanczos8, Tool.SamplingLanczos8);
		AddEvent(SamplingMitchell, Tool.SamplingMitchell);
		AddEvent(SamplingCosine, Tool.SamplingCosine);
		AddEvent(SamplingCubicConvolution, Tool.SamplingCubicConvolution);

		BrushHardness.onValueChanged.AddListener((val) => {
			Editor.CurrentBrush.Hardness = val;
			Editor.CurrentBrush.RecalculateBrush();
		});
		BrushSize.onValueChanged.AddListener((val) => {
			Editor.CurrentBrush.Size = (int) val;
			Editor.CurrentBrush.RecalculateBrush();
		});
		BucketFillToleranceSlider.onValueChanged.AddListener((val) => {
			BucketFillTolerance = (int) val;
			BucketFillToleranceText.text = BucketFillToleranceSlider.value + "";
		});
		BucketFillToleranceText.text = BucketFillTolerance + "";

		var onClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
		onClick.callback.AddListener((evtData) => {
			SwitchTab(Tab.Tools);
		});
		ToolsTabHandler.triggers.Add(onClick);

		onClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
		onClick.callback.AddListener((evtData) => {
			SwitchTab(Tab.Quantization);
		});
		QuantizationTabHandler.triggers.Add(onClick);

		onClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
		onClick.callback.AddListener((evtData) => {
			SwitchTab(Tab.Protocol);
		});
		ProtocolTabHandler.triggers.Add(onClick);

		onClick = new EventTrigger.Entry() { eventID = EventTriggerType.PointerClick };
		onClick.callback.AddListener((evtData) => {
			SwitchTab(Tab.File);
		});
		FileTabHandler.triggers.Add(onClick);

		ChangeTypeButton.OnClick = () => {
			Controller.Instance.Popup.SetText("By changing the type of a design all layers will be merged.", false, () =>
			{
				Controller.Instance.SwitchToClothSelector(
					(type) =>
					{
						Editor.CurrentPattern.SetType(type);
						Controller.Instance.SwitchToPatternEditor(null, null, null);
					},
					() =>
					{
						Controller.Instance.SwitchToPatternEditor(null, null, null);
					}
				);
				return true;
			});
		};

		ClearButton.OnClick = () =>
		{
			Controller.Instance.ConfirmationPopup.Show("<align=\"center\"><#827157>Are you sure you want to clear the current design? This step is irreverseable.", () => {
				Editor.CurrentPattern.Clear();
			}, () => {
			});
		};

		SaveProjectButton.OnClick = () =>
		{
			var path = StandaloneFileBrowser.SaveFilePanel("Save project", "", "project.acnhp", new ExtensionFilter[] { new ExtensionFilter("ACNH project", new string[] { "acnhp" }) });
			if (path != null && path.Length > 0)
			{
				try
				{
					if (path.EndsWith(".acnhp"))
					{
						System.IO.File.WriteAllBytes(path, Editor.CurrentPattern.ToBytes());
					}
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogException(e);
					Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
				}
			}
		};

		LoadProjectButton.OnClick = () =>
		{
			var path = StandaloneFileBrowser.OpenFilePanel("Load project", "", new ExtensionFilter[] { new ExtensionFilter("ACNH project", new string[] { "acnhp" }) }, false);
			if (path != null && path.Length > 0)
			{
				try
				{
					if (path[0].EndsWith(".acnhp"))
					{
						Editor.OpenProject(System.IO.File.ReadAllBytes(path[0]));
					}
				}
				catch (System.ArgumentException e)
				{
					UnityEngine.Debug.LogException(e);
					if (e.ParamName == "project")
						Controller.Instance.Popup.SetText(e.Message, false, () => { return true; });
					else 
						Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
				}
				catch (System.Exception e)
				{
					UnityEngine.Debug.LogException(e);
					Controller.Instance.Popup.SetText("Unknown error occured.", false, () => { return true; });
				}
			}
		};
	}
}