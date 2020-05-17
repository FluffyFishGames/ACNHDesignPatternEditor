using DesignServer.Messages;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using xBRZNet;

public class PatternExchange : MonoBehaviour
{
    public RectTransform Panel;
    public CanvasGroup Background;
    public Previews Previews;

    [Header("Items")]
    public CanvasGroup ItemsPanel;
    public RectTransform Loading;
    public UnityEngine.UI.GridLayoutGroup ItemsContainer;
    public UnityEngine.UI.Button PreviousPage;
    public UnityEngine.UI.Button NextPage;
    public TMPro.TMP_InputField SearchInput;
    public UnityEngine.UI.Image DesignsIcon;
    public UnityEngine.UI.Image ProDesignsIcon;
    public RectTransform DesignsTransform;
    public RectTransform ProDesignsTransform;
    public GameObject DesignsTooltip;
    public GameObject ProDesignsTooltip;
    public TMPro.TextMeshProUGUI PagesText;
    public GameObject PatternPrefab;
    public GameObject PagesContainer;
    public Transform TagsContainer;
    public TMPro.TextMeshProUGUI NameText;
    public TMPro.TextMeshProUGUI CreatorText;
    public GameObject TagPrefab;
    public CanvasGroup Tooltip;
    public RectTransform TooltipItems;
    public RectTransform TooltipBackground;
    public RawImage TooltipImage;
    public MenuButton CancelButton;
    public Pop CancelPop;
    

    [Header("Design")]
    public CanvasGroup DesignPanel;
    public UnityEngine.UI.RawImage DesignPreview;
    public UnityEngine.UI.RawImage PatternPreview;
    public TMPro.TextMeshProUGUI DesignType;
    public TMPro.TextMeshProUGUI DesignCode;
    public TMPro.TextMeshProUGUI CreatorName;
    public Tooltip DesignName;
    public Pop BackPop;
    public Pop ImportPop;
    public Pop ContinuePop;
    public MenuButton BackButton;
    public MenuButton ImportButton;
    public MenuButton ContinueButton;
    public DesignServer.Client Client;
    private float OpenPhase = 0f;
    private bool Opened = false;
    private bool FullOpen = true;
    private const float OpenTime = 1.5f;
    private const float Phase1 = 0.3f;
    private const float Phase2 = 0.7f;
    private CanvasGroup CurrentPanel;
    private int Page;
    private int Pages;

    private System.Action<DesignServer.Pattern> ConfirmImport;
    private System.Action Confirm;
    private System.Action Cancel;

    private DesignServer.Pattern Pattern;
    private DesignPattern DesignPattern;
    private TextureBitmap CurrentPreview;
    private TextureBitmap CurrentUpscaledPreview;
    private bool ProDesigns = false;
    private bool IsLoading = false;
    private DesignServer.Messages.ListPatternsResponse Response;
    private DesignServer.Pattern SelectedPattern;

    public void Hide()
    {
        Opened = false;
        FullOpen = true;
        ContinuePop.PopOut();
        BackPop.PopOut();
        ImportPop.PopOut();
        Cancel = null;
        ConfirmImport = null;
        Confirm = null;

        if (CurrentPreview != null)
        {
            CurrentPreview.Dispose();
            CurrentPreview = null;
        }
        if (CurrentUpscaledPreview != null)
        {
            CurrentUpscaledPreview.Dispose();
            CurrentUpscaledPreview = null;
        }
    }

    private bool UpdateTooltipLayout = false;
    public void ShowTooltip(PatternExchangePattern pattern)
    {
        Tooltip.alpha = 1f;
        Tooltip.GetComponent<RectTransform>().anchoredPosition = Controller.Instance.TopLeftTransform.InverseTransformPoint(pattern.GetComponent<RectTransform>().TransformPoint(new Vector3(0f, 0f)));
        NameText.text = pattern.Pattern.Name;
        CreatorText.text = pattern.Pattern.Creator;
        TooltipImage.texture = pattern.CurrentTexture.Texture;
        for (int i = TagsContainer.childCount - 1; i >= 0; i--)
            Destroy(TagsContainer.GetChild(i).gameObject);
        for (int i = 0; i < pattern.Pattern.Tags.Length; i++)
        {
            var newTag = GameObject.Instantiate(TagPrefab, TagsContainer).GetComponent<PatternExchangeTag>();
            newTag.SetTag(pattern.Pattern.Tags[i]);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(newTag.GetComponent<RectTransform>());
        }
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipItems);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipBackground);
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(Tooltip.GetComponent<RectTransform>());
        UpdateTooltipLayout = true;
    }

    public void HideTooltip()
    {
        Tooltip.alpha = 0f;
    }
    void SearchPatterns()
    {
        if (IsLoading) return;
        IsLoading = true;
        var searchPhrase = this.SearchInput.text.Trim();
        var message = new DesignServer.Messages.ListPatterns();
        var query = new DesignServer.SearchQuery();
        if (searchPhrase.ToLowerInvariant().StartsWith("de-"))
            query.Code = searchPhrase.Substring(3).ToUpperInvariant();
        else if (searchPhrase != "")
            query.Phrase = searchPhrase;

        query.Page = this.Page;
        query.ProDesigns = ProDesigns;
        
        message.Query = query;
        Client.Connection.SendMessage(message, (resp) => {
            if (resp is ListPatternsResponse response)
            {
                IsLoading = false;
                Response = response;
            }
        });
    }

    public void ShowItems(bool proDesigns, System.Action<DesignServer.Pattern> confirm, System.Action cancel)
    {
        this.ProDesigns = proDesigns;
        if (!this.Opened)
        {
            this.PagesContainer.SetActive(false);
            this.Page = 0;
            this.SearchInput.text = "";
        }
        if (ProDesigns)
        {
            this.ProDesignsTooltip.SetActive(true);
            this.DesignsTooltip.SetActive(false);
            this.ProDesignsIcon.color = new Color(237f / 255f, 123f / 255f, 133f / 255f);
            this.DesignsIcon.color = new Color(185f / 255f, 173f / 255f, 151f / 255f);
        }
        else
        {
            this.ProDesignsTooltip.SetActive(false);
            this.DesignsTooltip.SetActive(true);
            this.ProDesignsIcon.color = new Color(185f / 255f, 173f / 255f, 151f / 255f);
            this.DesignsIcon.color = new Color(237f / 255f, 123f / 255f, 133f / 255f);
        }
        SearchPatterns();
        this.ConfirmImport = confirm;
        this.Cancel = cancel;
        StartCoroutine(ShowItemsCoroutine());
    }

    public void ShowDesign(DesignServer.Pattern pattern, System.Action confirm)
    {
        if (confirm == null && Cancel == null && Confirm == null && ConfirmImport == null)
            return;
        // we are coming from items
        if (confirm == null)
        {
            SelectedPattern = pattern;
            CancelPop.PopOut();
        }
        if (CurrentPreview != null)
        {
            CurrentPreview.Dispose();
            CurrentPreview = null;
        }
        if (CurrentUpscaledPreview != null)
        {
            CurrentUpscaledPreview.Dispose();
            CurrentUpscaledPreview = null;
        }

        this.Pattern = pattern;
        this.Confirm = confirm;
        this.DesignName.Open();
        this.DesignName.Text = Pattern.Name;
        this.CreatorName.text = Pattern.Creator;
        var t = (DesignPattern.TypeEnum) Pattern.Type;
        try
        {
            var acnhFileFormat = new ACNHFileFormat(pattern.Bytes);
            if (acnhFileFormat.IsPro)
            {
                DesignPattern = new ProDesignPattern();
                DesignPattern.CopyFrom(acnhFileFormat);
            }
            else
            {
                DesignPattern = new SimpleDesignPattern();
                DesignPattern.CopyFrom(acnhFileFormat);
            }
            CurrentPreview = DesignPattern.GetBitmap();
            CurrentPreview.Apply();
            CurrentPreview.Texture.filterMode = FilterMode.Point;
            CurrentPreview.Texture.wrapMode = TextureWrapMode.Clamp;

            CurrentUpscaledPreview = new TextureBitmap(CurrentPreview.Width * 4, CurrentPreview.Height * 4);

            int[] src = CurrentPreview.ConvertToInt();
            int[] target = new int[CurrentUpscaledPreview.Width * CurrentUpscaledPreview.Height];
            var scaler = new xBRZScaler();
            scaler.ScaleImage(4, src, target, CurrentPreview.Width, CurrentPreview.Height, new xBRZNet.ScalerCfg(), 0, int.MaxValue);
            CurrentUpscaledPreview.FromInt(target);
            CurrentUpscaledPreview.Apply();

            Previews.AllPreviews[DesignPattern.Type].SetTexture(CurrentUpscaledPreview.Texture);
            Previews.AllPreviews[DesignPattern.Type].Render();
            PatternPreview.texture = CurrentPreview.Texture;
            DesignPreview.texture = Previews.AllPreviews[DesignPattern.Type].Camera.targetTexture;

            this.DesignType.text = DesignPatternInformation.Types[t].Name;
            this.DesignCode.text = "DE-" + pattern.Code;
        }
        catch (System.Exception e)
        {

        }
        StartCoroutine(ShowDesignCoroutine());
    }

    IEnumerator ShowDesignCoroutine()
    {
        if (Opened)
        {
            FullOpen = false;
            yield return new WaitForSeconds((1f - Phase1) * OpenTime);
        }
        ItemsPanel.gameObject.SetActive(false);
        DesignPanel.gameObject.SetActive(true);
        CurrentPanel = DesignPanel;
        Opened = true;
        FullOpen = true;
        yield return new WaitForSeconds(0.5f);
        if (ConfirmImport == null)
        {
            ImportButton.gameObject.SetActive(false);
            BackButton.gameObject.SetActive(false);
            ContinueButton.gameObject.SetActive(true);
            ContinuePop.PopUp();
        }
        else
        {
            ImportButton.gameObject.SetActive(true);
            BackButton.gameObject.SetActive(true);
            ContinueButton.gameObject.SetActive(false);
            ImportPop.PopUp();
            BackPop.PopUp();
        }
    }

    IEnumerator ShowItemsCoroutine()
    {
        Tooltip.alpha = 0f;
        if (Opened)
        {
            FullOpen = false;
            yield return new WaitForSeconds((1f - Phase1) * OpenTime);
        }
        ItemsPanel.gameObject.SetActive(true);
        DesignPanel.gameObject.SetActive(false);
        CurrentPanel = ItemsPanel;
        Opened = true;
        FullOpen = true;
        yield return new WaitForSeconds(0.1f);
        CancelPop.PopUp();
    }

    // Start is called before the first frame update
    void Start()
    {
        Background.alpha = 0f;
        Panel.localScale = new Vector3(1f, 0f, 1f);
        ItemsPanel.alpha = 0f;
        DesignPanel.alpha = 0f;
        ContinueButton.OnClick = () =>
        {
            Confirm?.Invoke();
        };
        CancelButton.OnClick = () =>
        {
            Cancel?.Invoke();
        };
        ImportButton.OnClick = () =>
        {
            ConfirmImport?.Invoke(SelectedPattern);
        };

        NextPage.onClick.AddListener(() => {
            if (this.Page < this.Pages - 1)
            {
                this.Page++;
                this.SearchPatterns();
            }
        });
        PreviousPage.onClick.AddListener(() => {
            if (this.Page > 0)
            {
                this.Page--;
                this.SearchPatterns();
            }
        });
        BackButton.OnClick = () =>
        {
            StartCoroutine(ShowItemsCoroutine());
        };
        SearchInput.onValueChanged.AddListener((value) => {
            SearchTimer = 0.5f;
        });
    }

    private float SearchTimer = 0f;

    // Update is called once per frame
    void Update()
    {
        if (SearchTimer > 0f)
        {
            SearchTimer -= Time.deltaTime;
            if (SearchTimer <= 0f)
            {
                this.SearchPatterns();
            }
        }
        /*if (UpdateTooltipLayout)
        {*/
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipItems);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(TooltipBackground);
            UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(Tooltip.GetComponent<RectTransform>());
/*            UpdateTooltipLayout = false;
        }*/

        if (this.Loading.gameObject.activeSelf != this.IsLoading)
        {
            this.Loading.gameObject.SetActive(this.IsLoading);
            this.ItemsContainer.gameObject.SetActive(!this.IsLoading);
        }

        if (Response != null)
        {
            Debug.Log("Response received");
            this.Pages = Response.Results.Pages;
            this.PagesText.text = (this.Page + 1) + " / " + Response.Results.Pages;
            this.PagesContainer.SetActive(true);
            this.NextPage.gameObject.SetActive(this.Page < this.Pages - 1);
            this.PreviousPage.gameObject.SetActive(this.Page > 0);

            for (int i = ItemsContainer.transform.childCount - 1; i >= 0; i--)
                Destroy(ItemsContainer.transform.GetChild(i).gameObject);
            for (int i = 0; i < Response.Results.Patterns.Count; i++)
            {
                var item = GameObject.Instantiate(PatternPrefab, ItemsContainer.transform).GetComponent<PatternExchangePattern>();
                item.ShowPattern(Response.Results.Patterns[i], this);
            }
            Response = null;
        }
        if (CurrentPanel != null)
        {
            if (Opened && (FullOpen && OpenPhase < 1f) || (!FullOpen && OpenPhase < 0.3f))
                OpenPhase = Mathf.Min(FullOpen ? 1f : 0.3f, OpenPhase + Time.deltaTime / OpenTime);
            if ((Opened && !FullOpen && OpenPhase > 0.3f) || (!Opened && OpenPhase > 0f))
                OpenPhase = Mathf.Max(!Opened ? 0 : 0.3f, OpenPhase - Time.deltaTime / OpenTime);

            var phase1 = Mathf.Clamp(OpenPhase, 0f, Phase1) / (Phase1);
            var phase2 = (Mathf.Clamp(OpenPhase, Phase1, Phase2) - Phase1) / (Phase2 - Phase1);
            var phase3 = (Mathf.Clamp(OpenPhase, Phase2, 1f) - Phase2) / (1f - Phase2);

            phase1 = EasingFunction.EaseOutQuad(0f, 1f, phase1);
            phase2 = EasingFunction.EaseOutQuad(0f, 1f, phase2);
            phase3 = EasingFunction.EaseOutQuad(0f, 1f, phase3);

            Background.alpha = phase1;
            Panel.localScale = new Vector3(1f, phase2, 1f);
            CurrentPanel.alpha = phase3;
        }
    }
}
