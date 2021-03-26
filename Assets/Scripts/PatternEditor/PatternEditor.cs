using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Image = UnityEngine.UI.Image;

public class PatternEditor : MonoBehaviour
{
	public bool IsPro;
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
	public GameObject FanFront;
	public GameObject FanBack;
	public GameObject FlagFront;
	public GameObject FlagBack;
	public GameObject FlagPole;

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
				if (CurrentPattern != null)
				{
					CurrentPattern.Dispose();
					CurrentPattern = null;
				}
				if (Secret != null)
				{
					Secret.Dispose();
					Secret = null;
				}
				if (CurrentBrush != null)
				{
					CurrentBrush.Dispose();
					CurrentBrush = null;
				}
				Layers.Unloaded();
				Tools.Unloaded();
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
			if (this.CurrentBrush != null)
			{
				this.CurrentBrush.Dispose();
				this.CurrentBrush = null;
			}

			this.CurrentBrush = new Brush() { Editor = this };
			if (pattern != null)
			{
				Logger.Log(Logger.Level.DEBUG, "[PatternEditor] Adding pattern to editor.");
				IsPro = pattern is ProDesignPattern;
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

	public void OpenProject(byte[] data)
	{
		try
		{
			var newPattern = new Pattern(this, data);
			if (this.CurrentPattern != null)
				this.CurrentPattern.Dispose();
			this.CurrentPattern = newPattern;
			this.CurrentPattern.ProjectLoaded();
		}
		catch (System.Exception e)
		{
			throw e;
		}
		this.Show(null, null, null);
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
			};

			SaveButton.OnClick += () =>
			{
				//Debug.Log("CANCEL " + ConfirmAction);
				ConfirmAction?.Invoke();
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
				{ DesignPatternInformation.PartType.HatBottom, HatBottom },
				{ DesignPatternInformation.PartType.FanFront, FanFront },
				{ DesignPatternInformation.PartType.FanBack, FanBack },
				{ DesignPatternInformation.PartType.FlagFront, FlagFront },
				{ DesignPatternInformation.PartType.FlagBack, FlagBack },
				{ DesignPatternInformation.PartType.FlagPole, FlagPole }
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
		if (this.Secret != null) return;
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

	private KeyCode[] SecretCombo = new KeyCode[] { KeyCode.UpArrow, KeyCode.UpArrow, KeyCode.DownArrow, KeyCode.DownArrow, KeyCode.LeftArrow, KeyCode.RightArrow, KeyCode.LeftArrow, KeyCode.RightArrow };
	private int CurrentSecret = 0;
	private Snake Secret;
	public class Snake
	{
		private int Width;
		private int Height;
		private float SnakeSpeed = 0.5f;
		private List<TextureBitmap.Point> Points = new List<TextureBitmap.Point>();
		private bool Extend = false;
		private float Phase = 0f;
		private Direction CurrentDirection;
		private Direction LastDirection;
		private TextureBitmap.Point FoodPosition;
		private TextureBitmap Playground;
		private TextureBitmap Original;
		private TextureBitmap.Color CurrentSnakeColor;
		private TextureBitmap.Color CurrentFoodColor;
		public bool Dead = false;

		public Snake(TextureBitmap playground)
		{
			CurrentSnakeColor = new TextureBitmap.Color(0, 0, 0, 255);

			Original = playground.Clone();
			Playground = playground;
			Width = playground.Width;
			Height = playground.Height;
			int startX = Width / 2;
			int startY = Height / 2;
			for (int i = 0; i < 3; i++)
				Points.Add(new TextureBitmap.Point(startX, startY + 3 - i));
			SpawnFood();
			CurrentDirection = Direction.UP;
			LastDirection = Direction.UP;
		}

		public enum Direction
		{
			UP,
			LEFT,
			RIGHT,
			DOWN
		};

		public void Update()
		{
			if (Dead)
				return;
			if (Input.GetKeyDown(KeyCode.UpArrow) && LastDirection != Direction.DOWN)
				CurrentDirection = Direction.UP;
			if (Input.GetKeyDown(KeyCode.DownArrow) && LastDirection != Direction.UP)
				CurrentDirection = Direction.DOWN;
			if (Input.GetKeyDown(KeyCode.LeftArrow) && LastDirection != Direction.RIGHT)
				CurrentDirection = Direction.LEFT;
			if (Input.GetKeyDown(KeyCode.RightArrow) && LastDirection != Direction.LEFT)
				CurrentDirection = Direction.RIGHT;
			Phase += Time.deltaTime;
			if (Phase > SnakeSpeed)
			{
				Phase -= SnakeSpeed;
				Move();
				Render(true);
			}
			else Render(false);
		}
		
		private void Render(bool renderEffects = false)
		{
			Playground.AlphaComposite(Original, new TextureBitmap.Color(255, 255, 255, 10));
			if (renderEffects)
				Playground.SetPixel(FoodPosition.X, FoodPosition.Y, new TextureBitmap.Color(255, 255, 255, 255));
			else
				Playground.SetPixel(FoodPosition.X, FoodPosition.Y, CurrentFoodColor);

			for (int i = 0; i < Points.Count; i++)
			{
				if (renderEffects)
				{
					Original.SetPixel(Points[i].X, Points[i].Y, Original.GetPixel(Points[i].X, Points[i].Y).AlphaComposite(new TextureBitmap.Color(CurrentSnakeColor.R, CurrentSnakeColor.G, CurrentSnakeColor.B, 20)));
				}
				Playground.SetPixel(Points[i].X, Points[i].Y, CurrentSnakeColor);
			}
			Playground.Apply();
		}

		private void Move()
		{
			int start = Points.Count - 1;
			for (int i = start; i >= 0; i--)
			{
				if (i == 0)
				{
					Points[i].X += CurrentDirection == Direction.LEFT ? -1 : CurrentDirection == Direction.RIGHT ? 1 : 0;
					Points[i].Y -= CurrentDirection == Direction.UP ? -1 : CurrentDirection == Direction.DOWN ? 1 : 0;
					if (Points[i].X < 0 || Points[i].X >= Width || Points[i].Y < 0 || Points[i].Y >= Height)
						Dead = true;
					for (int j = 1; j < Points.Count; j++)
					{
						if (Points[i].X == Points[j].X && Points[i].Y == Points[j].Y)
							Dead = true;
					}
					if (Points[i].X == FoodPosition.X && Points[i].Y == FoodPosition.Y)
						Eat();
				}
				else
				{
					if (Extend && i == Points.Count - 1)
					{
						Points.Add(new TextureBitmap.Point(Points[i].X, Points[i].Y));
						Extend = false;
					}
					Points[i].X = Points[i - 1].X;
					Points[i].Y = Points[i - 1].Y;
				}
			}
			LastDirection = CurrentDirection;
		}

		private void Eat()
		{
			SnakeSpeed *= 0.95f;
			if (SnakeSpeed < 0.1f)
				SnakeSpeed = 0.1f;
			CurrentSnakeColor = CurrentFoodColor;
			Extend = true;
			int size = Random.Range(2, 10);
			int opacity = Random.Range(100, 200);
			for (int x = -size; x <= size; x++)
			{
				for (int y = -size; y <= size; y++)
				{
					int fx = FoodPosition.X + x;
					int fy = FoodPosition.Y + y;
					var dist = Vector2.Distance(new Vector2(fx, fy), new Vector2(FoodPosition.X, FoodPosition.Y)) / size;
					if (fx < 0) fx += Width;
					if (fy < 0) fy += Height;
					if (fx >= Width) fx -= Width;
					if (fy >= Height) fy -= Height;
					Original.SetPixel(fx, fy, Original.GetPixel(fx, fy).AlphaComposite(new TextureBitmap.Color(CurrentSnakeColor.R, CurrentSnakeColor.G, CurrentSnakeColor.B, (byte) (Mathf.Clamp01(1 - dist) * opacity))));
				}
			}
			SpawnFood();
		}
		
		public void Dispose()
		{
			Original.Dispose();
		}

		void SpawnFood()
		{
			List<TextureBitmap.Point> freeSpots = new List<TextureBitmap.Point>();
			for (int x = 0; x < Width; x++)
			{
				for (int y = 0; y < Height; y++)
				{
					bool free = true;
					for (int i = 0; i < Points.Count; i++)
					{
						if (x == Points[i].X && y == Points[i].Y)
						{
							free = false;
							break;
						}
					}
					if (free)
						freeSpots.Add(new TextureBitmap.Point(x, y));
				}
			}

			CurrentFoodColor = new TextureBitmap.Color((byte) Random.Range(0, 255), (byte) Random.Range(0, 255), (byte) Random.Range(0, 255), 255);
			FoodPosition = freeSpots[Random.Range(0, freeSpots.Count)];
		}
	}
	// Update is called once per frame
	void Update()
    {
		if (Secret != null)
		{
			if (!Secret.Dead)
			{
				Secret.Update();
				CurrentPattern.CurrentSubPattern.UpdateImage();
				CurrentPattern.RegeneratePreview();
			}
		}
		if (Secret == null && IsShown && Input.anyKeyDown)
		{
			if (Input.GetKey(SecretCombo[CurrentSecret]))
			{
				CurrentSecret++;
				if (CurrentSecret == SecretCombo.Length)
				{
					CurrentSecret = 0;
					var layer = new RasterLayer(CurrentPattern.CurrentSubPattern, "Snake");
					layer.Bitmap.CopyFrom(this.CurrentPattern.CurrentSubPattern.Bitmap);
					CurrentPattern.CurrentSubPattern.Layers.Add(layer);
					LayersChanged();
					Secret = new Snake(layer.Bitmap);
				}
			}
			else
			{
				CurrentSecret = 0;
			}
		}
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
				if (Tools.IsToolActive(Tools.Tool.Brush) && Input.GetKeyDown(KeyCode.B))
					Tools.SwitchTool(Tools.Tool.Brush);
				if (Tools.IsToolActive(Tools.Tool.BucketFill) && Input.GetKeyDown(KeyCode.F))
					Tools.SwitchTool(Tools.Tool.BucketFill);
				if (Tools.IsToolActive(Tools.Tool.ColorPicker) && Input.GetKeyDown(KeyCode.C))
					Tools.SwitchTool(Tools.Tool.ColorPicker);
				if (Tools.IsToolActive(Tools.Tool.Eraser) && Input.GetKeyDown(KeyCode.E))
					Tools.SwitchTool(Tools.Tool.Eraser);
				if (Tools.IsToolActive(Tools.Tool.Line) && Input.GetKeyDown(KeyCode.L))
					Tools.SwitchTool(Tools.Tool.Line);
				if (Tools.IsToolActive(Tools.Tool.Rect) && Input.GetKeyDown(KeyCode.S))
					Tools.SwitchTool(Tools.Tool.Rect);
				if (Tools.IsToolActive(Tools.Tool.Transform) && Input.GetKeyDown(KeyCode.T))
					Tools.SwitchTool(Tools.Tool.Transform);
				if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
					Input.GetKeyDown(KeyCode.Z))
					Tools.Undo();
				if ((Input.GetKey(KeyCode.LeftControl) || Input.GetKey(KeyCode.RightControl)) &&
					Input.GetKeyDown(KeyCode.Y))
					Tools.Redo();

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
			if (this.CurrentPattern.Type == DesignPattern.TypeEnum.SimplePattern)
			{
				var pattern = new SimpleDesignPattern
				{
					Type = Type/*,
					IsPro = Type != DesignPattern.TypeEnum.SimplePattern*/
				};
				pattern.FromBitmap(this.CurrentPattern.PreviewBitmap);
				return pattern;
			}
			else
			{
				var pattern = new ProDesignPattern
				{
					Type = Type/*,
					IsPro = Type != DesignPattern.TypeEnum.SimplePattern*/
				};
				pattern.FromBitmap(this.CurrentPattern.PreviewBitmap);
				return pattern;
			}
		}
		catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[PatternEditor] Error while saving pattern: " + e.ToString()); return null; }
	}

	private void OnDestroy()
	{
		if (CurrentPattern != null)
			CurrentPattern.Dispose();
		if (CurrentBrush != null)
			CurrentBrush.Dispose();
		if (Secret != null)
			Secret.Dispose();

	}

	private void OnApplicationQuit()
	{
		if (CurrentPattern != null)
			CurrentPattern.Dispose();
		if (CurrentBrush != null)
			CurrentBrush.Dispose();
		if (Secret != null)
			Secret.Dispose();
	}
}
