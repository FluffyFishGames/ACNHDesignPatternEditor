using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.EventSystems;

public class PatternExchangePattern : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
    public UnityEngine.UI.RawImage Image;
    public DesignServer.Pattern Pattern;
    public PatternExchange PatternExchange;
    public TextureBitmap CurrentTexture;

    public void OnPointerClick(PointerEventData eventData)
    {
        this.PatternExchange.ShowDesign(this.Pattern, null);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        this.PatternExchange.ShowTooltip(this);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        this.PatternExchange.HideTooltip();
    }

    public void ShowPattern(DesignServer.Pattern pattern, PatternExchange exchange)
    {
        try
        {
            PatternExchange = exchange;
            Pattern = pattern;
            if (CurrentTexture != null)
            {
                CurrentTexture.Dispose();
                CurrentTexture = null;
            }

            ACNHFileFormat format = new ACNHFileFormat(pattern.Bytes);
            DesignPattern patt = null;
            if (format.IsPro)
            {
                patt = new ProDesignPattern();
                patt.CopyFrom(format);
            }
            else
            {
                patt = new SimpleDesignPattern();
                patt.CopyFrom(format);
            }

            CurrentTexture = patt.GetBitmap();
            CurrentTexture.Apply();
            CurrentTexture.Texture.filterMode = FilterMode.Point;
            CurrentTexture.Texture.wrapMode = TextureWrapMode.Clamp;
            this.Image.texture = CurrentTexture.Texture;
        }
        catch (System.Exception e)
        {

        }
    }

    private void OnDestroy()
    {
        CurrentTexture.Dispose();
        CurrentTexture = null;
    }
    // Start is called before the first frame update
    void Start()
    {
    }
}
