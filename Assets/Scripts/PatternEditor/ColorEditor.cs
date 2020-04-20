using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ColorEditor : MonoBehaviour
{
    private float ShowPhase = 0f;
    private bool Shown = false;
    public enum ColorMode
    {
        HSV,
        RGB
    };

    public bool IsShown
    {
        get
        {
            return Shown;
        }
    }

    public Image Background;
    public RectTransform StripedBackground;
    public Image Slider1Image;
    public Image Slider2Image;
    public Image Slider3Image;
    public Slider Slider1;
    public Slider Slider2;
    public Slider Slider3;
    public TMPro.TMP_InputField Text1;
    public TMPro.TMP_InputField Text2;
    public TMPro.TMP_InputField Text3;
    public TMPro.TMP_InputField TextRGB;
    public Button HSV;
    public Button RGB;
    public TMPro.TextMeshProUGUI HSVText;
    public TMPro.TextMeshProUGUI RGBText;
    public MenuButton Save;
    public PatternEditor Editor;

    private Color CurrentColor;
    private ColorPaletteButton PaletteButton;
    private RectTransform MyTransform;
    private float StartY = 0;
    private float StripePhase = 0f;
    private float PreviousShowPhase;
    private ColorMode Mode;
    private bool UpdatingInputs = false;

    // Start is called before the first frame update
    void Start()
    {
        MyTransform = GetComponent<RectTransform>();
        StartY = MyTransform.anchoredPosition.y;
        ReparseSliders();

        Slider1.onValueChanged.AddListener((val) => {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(val, Slider2.value, Slider3.value);
            else
                CurrentColor.r = val;
            UpdateInputs();
            UpdateColor();
        });
        Slider2.onValueChanged.AddListener((val) => {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(Slider1.value, val, Slider3.value);
            else
                CurrentColor.g = val;
            UpdateInputs();
            UpdateColor();
        });
        Slider3.onValueChanged.AddListener((val) => {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(Slider1.value, Slider2.value, val);
            else
                CurrentColor.b = val;
            UpdateInputs();
            UpdateColor();
        });
        Text1.onValueChanged.AddListener((val) =>
        {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(((float) int.Parse(val)) / 360f, Slider2.value, Slider3.value);
            else
                CurrentColor.r = ((float) int.Parse(val)) / 255f;
            UpdateInputs();
            UpdateColor();
        });
        Text2.onValueChanged.AddListener((val) =>
        {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(Slider1.value, ((float) int.Parse(val)) / 100f, Slider3.value);
            else
                CurrentColor.g = ((float) int.Parse(val)) / 255f;
            UpdateInputs();
            UpdateColor();
        });
        Text3.onValueChanged.AddListener((val) =>
        {
            if (UpdatingInputs) return;
            if (Mode == ColorMode.HSV)
                CurrentColor = Color.HSVToRGB(Slider1.value, Slider2.value, ((float) int.Parse(val)) / 100f);
            else
                CurrentColor.b = ((float)int.Parse(val)) / 255f;
            UpdateInputs();
            UpdateColor();
        });
        TextRGB.onValueChanged.AddListener((val) => {
            if (UpdatingInputs) return;
            if (!val.StartsWith("#"))
                val = "#" + val;

            var hexCode = val.Substring(1);
            if (hexCode.Length == 6)
            {
                try
                {
                    int color = System.Convert.ToInt32(hexCode, 16);
                    int r = (color & 0xff0000) >> 16;
                    int g = (color & 0xff00) >> 8;
                    int b = (color & 0xff);
                    CurrentColor = new Color(((float) r) / 255f, ((float) g) / 255f, ((float) b) / 255f);
                    UpdateInputs();
                    UpdateColor();
                }
                catch (System.Exception e)
                {

                }
            }
        });
        
        TextRGB.onDeselect.AddListener((val) => {
            if (UpdatingInputs) return;
            UpdateInputs();
            UpdateColor();
        });

        HSV.onClick.AddListener(() => {
            ChangeColorMode(ColorMode.HSV);
        });

        RGB.onClick.AddListener(() => {
            ChangeColorMode(ColorMode.RGB);
        });

        Save.OnClick = () =>
        {
            Editor.OpenColorEditor(null);
            Hide();
        };
    }
    /*
    void UpdateRGBText()
    {
        if (Mode == ColorMode.HSV)
        {
            var col = UnityEngine.Color.HSVToRGB(Slider1.value, Slider2.value, Slider3.value);
            string hex = "#" + ((int) (col.r * 255f)).ToString("X2") + ((int) (col.g * 255f)).ToString("X2") + ((int) (col.b * 255f)).ToString("X2");
            RGBText.text = hex;
        }
        else
        {
            string hex = "#" + ((int) (Slider1.value * 255f)).ToString("X2") + ((int) (Slider2.value * 255f)).ToString("X2") + ((int) (Slider3.value * 255f)).ToString("X2");
            RGBText.text = hex;
        }
    }
    */
    void UpdateColor()
    {
        PaletteButton.SetColor(CurrentColor);
        ReparseSliders();
    }

    public void Show(ColorPaletteButton button)
    {
        PaletteButton = button;
        CurrentColor = PaletteButton.GetColor();
        Shown = true;
        ChangeColorMode(ColorMode.HSV);
        UpdateInputs();
    }

    public void Hide()
    {
        Shown = false;
    }

    public void ChangeColorMode(ColorMode mode)
    {
        Mode = mode;
        if (Mode == ColorMode.HSV)
        {
            HSVText.color = new Color(103f / 255f, 89 / 255f, 78 / 255f);
            RGBText.color = new Color(212f / 255f, 135f / 255f, 155 / 255f);
        }
        else
        {
            RGBText.color = new Color(103f / 255f, 89 / 255f, 78 / 255f);
            HSVText.color = new Color(212f / 255f, 135f / 255f, 155 / 255f);
        }
        UpdateInputs();
        ReparseSliders();
    }

    void UpdateInputs()
    {
        UpdatingInputs = true;
        if (Mode == ColorMode.HSV)
        {
            float h;
            float s;
            float v;
            Color.RGBToHSV(CurrentColor, out h, out s, out v);
            Slider1.value = h;
            Slider2.value = s;
            Slider3.value = v;
            Text1.text = ((int) (h * 360f)) + "";
            Text2.text = ((int) (s * 100f)) + "";
            Text3.text = ((int) (v * 100f)) + "";
            string hex = "#" + ((int) (CurrentColor.r * 255f)).ToString("X2") + ((int) (CurrentColor.g * 255f)).ToString("X2") + ((int) (CurrentColor.b * 255f)).ToString("X2");
            TextRGB.text = hex;
        }
        else if (Mode == ColorMode.RGB)
        {
            Slider1.value = CurrentColor.r;
            Slider2.value = CurrentColor.g;
            Slider3.value = CurrentColor.b;
            Text1.text = ((int) (CurrentColor.r * 255f)) + "";
            Text2.text = ((int) (CurrentColor.g * 255f)) + "";
            Text3.text = ((int) (CurrentColor.b * 255f)) + "";
            string hex = "#" + ((int) (CurrentColor.r * 255f)).ToString("X2") + ((int) (CurrentColor.g * 255f)).ToString("X2") + ((int) (CurrentColor.b * 255f)).ToString("X2");
            TextRGB.text = hex;
        }
    }
    void ReparseSliders()
    {
        if (Mode == ColorMode.HSV)
        {
            if (Slider1Image.sprite != null)
            {
                Destroy(Slider1Image.sprite.texture);
                Destroy(Slider1Image.sprite);
            }
            var hueTexture = new Texture2D(360, 1, TextureFormat.RGB24, false);
            hueTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] hueColors = new Color[360];
            for (int i = 0; i < 360; i++)
                hueColors[i] = UnityEngine.Color.HSVToRGB(((float) i) / 360f, 1f, 1f);
            hueTexture.SetPixels(hueColors);
            hueTexture.Apply();
            Slider1Image.sprite = Sprite.Create(hueTexture, new Rect(0, 0, hueTexture.width, hueTexture.height), new Vector2(0.5f, 0.5f));

            if (Slider2Image.sprite != null)
            {
                Destroy(Slider2Image.sprite.texture);
                Destroy(Slider2Image.sprite);
            }
            var saturationTexture = new Texture2D(100, 1, TextureFormat.RGB24, false);
            saturationTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] saturationColors = new Color[100];
            for (int i = 0; i < 100; i++)
                saturationColors[i] = UnityEngine.Color.HSVToRGB(Slider1.value, ((float) i) / 100f, 1f);
            saturationTexture.SetPixels(saturationColors);
            saturationTexture.Apply();
            Slider2Image.sprite = Sprite.Create(saturationTexture, new Rect(0, 0, saturationTexture.width, saturationTexture.height), new Vector2(0.5f, 0.5f));

            if (Slider3Image.sprite != null)
            {
                Destroy(Slider3Image.sprite.texture);
                Destroy(Slider3Image.sprite);
            }
            var valueTexture = new Texture2D(100, 1, TextureFormat.RGB24, false);
            valueTexture.wrapMode = TextureWrapMode.Clamp; 
            Color[] valueColors = new Color[100];
            for (int i = 0; i < 100; i++)
                valueColors[i] = UnityEngine.Color.HSVToRGB(Slider1.value, Slider2.value, ((float) i) / 100f);
            valueTexture.SetPixels(valueColors);
            valueTexture.Apply();
            Slider3Image.sprite = Sprite.Create(valueTexture, new Rect(0, 0, valueTexture.width, valueTexture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            if (Slider1Image.sprite != null)
            {
                Destroy(Slider1Image.sprite.texture);
                Destroy(Slider1Image.sprite);
            }
            
            var redTexture = new Texture2D(255, 1, TextureFormat.RGB24, false);
            redTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] redColors = new Color[255];
            for (int i = 0; i < 255; i++)
                redColors[i] = new UnityEngine.Color(((float)i)/255f, CurrentColor.g, CurrentColor.b);
            redTexture.SetPixels(redColors);
            redTexture.Apply();
            Slider1Image.sprite = Sprite.Create(redTexture, new Rect(0, 0, redTexture.width, redTexture.height), new Vector2(0.5f, 0.5f));

            if (Slider2Image.sprite != null)
            {
                Destroy(Slider2Image.sprite.texture);
                Destroy(Slider2Image.sprite);
            }
            var greenTexture = new Texture2D(255, 1, TextureFormat.RGB24, false);
            greenTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] greenColors = new Color[255];
            for (int i = 0; i < 255; i++)
                greenColors[i] = new UnityEngine.Color(CurrentColor.r, ((float) i) / 255f, CurrentColor.b);
            greenTexture.SetPixels(greenColors);
            greenTexture.Apply();
            Slider2Image.sprite = Sprite.Create(greenTexture, new Rect(0, 0, greenTexture.width, greenTexture.height), new Vector2(0.5f, 0.5f));

            if (Slider3Image.sprite != null)
            {
                Destroy(Slider3Image.sprite.texture);
                Destroy(Slider3Image.sprite);
            }
            var blueTexture = new Texture2D(255, 1, TextureFormat.RGB24, false);
            blueTexture.wrapMode = TextureWrapMode.Clamp;
            Color[] blueColors = new Color[255];
            for (int i = 0; i < 255; i++)
                blueColors[i] = new UnityEngine.Color(CurrentColor.r, CurrentColor.g, ((float) i) / 255f);
            blueTexture.SetPixels(blueColors);
            blueTexture.Apply();
            Slider3Image.sprite = Sprite.Create(blueTexture, new Rect(0, 0, blueTexture.width, blueTexture.height), new Vector2(0.5f, 0.5f));
        }
    }

    // Update is called once per frame
    void Update()
    {
        UpdatingInputs = false;
        if (Shown && ShowPhase < 1f)
            ShowPhase = Mathf.Min(1f, ShowPhase + Time.deltaTime * 2f);
        if (!Shown && ShowPhase > 0f)
            ShowPhase = Mathf.Max(0f, ShowPhase - Time.deltaTime * 2f);

        StripePhase += Time.deltaTime;
        if (StripePhase > 1f)
            StripePhase -= 1f;
        StripedBackground.anchoredPosition = new Vector2(-30 + 30 * StripePhase, StripedBackground.anchoredPosition.y);

        if (PreviousShowPhase != ShowPhase)
        {
            MyTransform.anchoredPosition = new Vector2(0f, StartY - EasingFunction.EaseOutCubic(0f, MyTransform.sizeDelta.y + StartY, ShowPhase));
            Background.color = new Color(Background.color.r, Background.color.g, Background.color.b, 1f - ShowPhase);
            PreviousShowPhase = ShowPhase;
        }
    }
}
