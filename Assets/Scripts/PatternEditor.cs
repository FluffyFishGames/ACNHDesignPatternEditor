using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;
using SimplePaletteQuantizer.Quantizers;
using SimplePaletteQuantizer.Quantizers.XiaolinWu;
using SimplePaletteQuantizer.Quantizers.DistinctSelection;
using SimplePaletteQuantizer.Quantizers.Popularity;
using SimplePaletteQuantizer.Quantizers.MedianCut;
using SimplePaletteQuantizer.Quantizers.Octree;
using SimplePaletteQuantizer.ColorCaches;
using SimplePaletteQuantizer.ColorCaches.EuclideanDistance;
using SimplePaletteQuantizer.ColorCaches.LocalitySensitiveHash;
using SimplePaletteQuantizer.ColorCaches.Octree;
using Graphics = System.Drawing.Graphics;
using SimplePaletteQuantizer.Helpers;
using System.Threading;
using MyHorizons.Data;

public class PatternEditor : MonoBehaviour
{
	public LoadingAnimation Loading;
	public ColorPalette ColorPalette;
	public GameObject ToolButtonGroupPrefab;
	public RectTransform RightPanel;
	public GameObject ImportTools;
	public MenuButton SaveButton;
	public MenuButton CancelButton;
	public Transform PixelContainer;
	public GameObject PixelPrefab;
	public Image PreviewImage;
	public GameObject QRCode;
	public TMPro.TextMeshProUGUI NameValue;
	public TMPro.TextMeshProUGUI UsernameValue;
	public TMPro.TextMeshProUGUI TownValue;

	private ToolButtonGroup QuantizationGroup;
	private ToolButtonGroup ColorCacheGroup;
	private ToolButtonGroup ResamplingGroup;
	private ToolButtonGroup CropGroup;
	private DesignPattern CurrentPattern;
	private Pixel[] Pixels;
	private CanvasGroup MyCanvasGroup;
	private int Crop = 0;
	private int Resampling = 0;
	private int Quantization = 0;
	private int ColorCache = 0;
	private bool IsShown = false;
	private float ShowPhase = 0f;
	private System.Action CancelAction;
	private System.Action ConfirmAction;

	private static List<ICrop> Crops = new List<ICrop>()
	{
		null,
		new Fit(),
		new Cover()
	};

	private static List<ISampling> Samplers = new List<ISampling>()
	{
		new NearestNeighbourSampling(),
		new BillinearSampling(),
		new BicubicSampling()
	};

	private static List<IColorQuantizer> Quantizers = new List<IColorQuantizer>() {
		new WuColorQuantizer(),
		new DistinctSelectionQuantizer(),
		new PopularityQuantizer(),
		new MedianCutQuantizer(),
		new OctreeQuantizer()
	};
	
	private static List<IColorCache> ColorCaches = new List<IColorCache>()
	{
		new EuclideanDistanceColorCache(),
		new LshColorCache(),
		new OctreeColorCache()
	};

	public void Hide()
	{
		IsShown = false;
	}

	public void Show(System.Action confirm, System.Action cancel)
	{
		IsShown = true;
		ConfirmAction = confirm;
		CancelAction = cancel;

		if (Controller.Instance.CurrentOperation is ImportOperation importOperation)
		{
			if (importOperation.IsQRCode)
			{
				ImportTools.SetActive(false);
				QRCode.SetActive(true);
				NameValue.text = importOperation.GetName();
				UsernameValue.text = importOperation.Username;
				TownValue.text = importOperation.Town;
				ForceRebuildLayout();
			}
			else
			{
				QuantizationGroup.gameObject.SetActive(true);
				ColorCacheGroup.gameObject.SetActive(true);
				ResamplingGroup.gameObject.SetActive(importOperation.ImageWidth != 32 || importOperation.ImageHeight != 32);
				CropGroup.gameObject.SetActive(importOperation.ImageWidth != importOperation.ImageHeight);
				ImportTools.gameObject.SetActive(true);
				QRCode.SetActive(false);

				CropGroup.Select(0);
				ResamplingGroup.Select(0);
				QuantizationGroup.Select(0);
				ColorCacheGroup.Select(0);

				Crop = 0;
				Resampling = 0;
				Quantization = 0;
				ColorCache = 0;

				ForceRebuildLayout();
				RegeneratePattern();
			}
		}
	}

	void RegeneratePattern()
	{
		if (Controller.Instance.CurrentOperation is ImportOperation importOperation)
		{
			if (Quantization == 4)
				ColorCacheGroup.gameObject.SetActive(false);
			else
				ColorCacheGroup.gameObject.SetActive(true);
			ForceRebuildLayout();

			Loading.gameObject.SetActive(true);
			importOperation.ParsePattern(Crops[Crop], Samplers[Resampling], Quantizers[Quantization], ColorCaches[ColorCache]);
		}
	}

	private bool Initialized = false;
	void OnEnable()
	{
		MyCanvasGroup = GetComponent<CanvasGroup>();
		if (!Initialized)
			Initialize();
	}

	void ForceRebuildLayout()
	{
		LayoutRebuilder.ForceRebuildLayoutImmediate(CropGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(ResamplingGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(QuantizationGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(ColorCacheGroup.GetComponent<RectTransform>());
		LayoutRebuilder.ForceRebuildLayoutImmediate(ImportTools.GetComponent<RectTransform>());
	}
	void Start()
	{
		if (!Initialized)
			Initialize();
	}

	void ChangeCrop(int val)
	{
		if (Controller.Instance.CurrentOperation is ImportOperation importOperation && !importOperation.IsParsing) { Crop = val; RegeneratePattern(); }
	}

	void ChangeResampling(int val)
	{
		if (Controller.Instance.CurrentOperation is ImportOperation importOperation && !importOperation.IsParsing) { Resampling = val; RegeneratePattern(); }
	}

	void ChangeQuantization(int val)
	{
		if (Controller.Instance.CurrentOperation is ImportOperation importOperation && !importOperation.IsParsing) { Quantization = val; RegeneratePattern(); }
	}

	void ChangeColorCache(int val)
	{
		if (Controller.Instance.CurrentOperation is ImportOperation importOperation && !importOperation.IsParsing) { ColorCache = val; RegeneratePattern(); }
	}

	void Initialize()
	{
		Initialized = true;
		Loading.gameObject.SetActive(false);

		CancelButton.OnClick += () => {
			CancelAction?.Invoke();
		};

		SaveButton.OnClick += () => {
			ConfirmAction?.Invoke();
		};

		var crop = GameObject.Instantiate(ToolButtonGroupPrefab, ImportTools.transform);
		CropGroup = crop.GetComponent<ToolButtonGroup>();
		CropGroup.Title.text = "Crop";
		CropGroup.SetItems(new (string, System.Action)[] {
			("None", () => { ChangeCrop(0); }),
			("Fit", () => { ChangeCrop(1); }),
			("Cover", () => { ChangeCrop(2); })
		});

		var resampling = GameObject.Instantiate(ToolButtonGroupPrefab, ImportTools.transform);
		ResamplingGroup = resampling.GetComponent<ToolButtonGroup>();
		ResamplingGroup.Title.text = "Resampling";
		ResamplingGroup.SetItems(new (string, System.Action)[] {
			("Nearest Neighbour", () => { ChangeResampling(0); }),
			("Bilinear", () => { ChangeResampling(1); }),
			("Bicubic", () => { ChangeResampling(2); })
		});

		var quantization = GameObject.Instantiate(ToolButtonGroupPrefab, ImportTools.transform);
		QuantizationGroup = quantization.GetComponent<ToolButtonGroup>();
		QuantizationGroup.Title.text = "Quantization algorithm";
		QuantizationGroup.SetItems(new (string, System.Action)[] {
			("Xiaolin Wu", () => { ChangeQuantization(0); }),
			("HSL Distinct", () => { ChangeQuantization(1); }),
			("Popularity", () => { ChangeQuantization(2); }),
			("Median Cut", () => { ChangeQuantization(3); }),
			("Octree", () => { ChangeQuantization(4); })
		});

		var colorCache = GameObject.Instantiate(ToolButtonGroupPrefab, ImportTools.transform);
		ColorCacheGroup = colorCache.GetComponent<ToolButtonGroup>();
		ColorCacheGroup.Title.text = "Color cache";
		ColorCacheGroup.SetItems(new (string, System.Action)[] {
			("Euclidean distance", () => { ChangeColorCache(0); }),
			("Locally-sensitive", () => { ChangeColorCache(1); }),
			("Octree search", () => { ChangeColorCache(2); })
		});

		ForceRebuildLayout();

		Pixels = new Pixel[32 * 32];
		for (int i = 0; i < 32 * 32; i++)
		{
			var go = GameObject.Instantiate(PixelPrefab, PixelContainer);
			Pixels[i] = go.GetComponent<Pixel>();
		}

		ImportTools.gameObject.SetActive(false);
	}
	
	// Update is called once per frame
	void Update()
    {
		if (IsShown && ShowPhase < 1f)
			ShowPhase = Mathf.Min(1f, ShowPhase + 1f);
		if (!IsShown && ShowPhase > 0f)
			ShowPhase = Mathf.Max(0f, ShowPhase - Time.deltaTime * 4f);

		MyCanvasGroup.alpha = ShowPhase;

		if (Controller.Instance.CurrentOperation is ImportOperation importOperation)
		{
			if (importOperation.IsReady)
			{
				Loading.gameObject.SetActive(false);
				ColorPalette.SetPattern(importOperation.GetResultPattern());
				var colors = importOperation.GetPixels();
				for (var i = 0; i < Pixels.Length; i++)
					Pixels[i].SetColor(colors[i]);

				PreviewImage.sprite = importOperation.GetPreview();
			}
		}
    }
}
