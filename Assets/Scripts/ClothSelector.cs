using MyHorizons.Data;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ClothSelector : MonoBehaviour
{
    public EventTrigger TopsTrigger;
    public EventTrigger DressesTrigger;
    public EventTrigger HatsTrigger;
    public EventTrigger LegacyTrigger;
    public Image TopsIcon;
    public Image DressesIcon;
    public Image HatsIcon;
    public Image LegacyIcon;
    public GameObject TopsTooltip;
    public GameObject DressesTooltip;
    public GameObject HatsTooltip;
    public GameObject LegacyTooltip;
    public GameObject Tops;
    public GameObject Dresses;
    public GameObject Hats;
    public GameObject Legacy;
    public Previews Previews;
    public MenuButton CancelButton;

    private System.Action<DesignPattern.TypeEnum> ConfirmCallback;
    private System.Action CancelCallback;

    public enum Menu
    {
        None,
        Tops,
        Dresses,
        Hats,
        Legacy
    }
    public Menu CurrentMenu;

    void Start()
    {
        var click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((eventData) => {
            SwitchToTops();
        });
        TopsTrigger.triggers.Add(click);

        click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((eventData) => {
            SwitchToDresses();
        });
        DressesTrigger.triggers.Add(click);

        click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((eventData) => {
            SwitchToHats();
        });
        HatsTrigger.triggers.Add(click);

        click = new EventTrigger.Entry();
        click.eventID = EventTriggerType.PointerClick;
        click.callback.AddListener((eventData) => {
            SwitchToLegacy();
        });
        LegacyTrigger.triggers.Add(click);

        CancelButton.OnClick = () => {
            CancelCallback?.Invoke();
        };

        SwitchToTops();
    }

    public void SelectCloth(DesignPattern.TypeEnum type)
    {
        ConfirmCallback?.Invoke(type);
    }

    public void SetTexture(Texture2D texture)
    {
        foreach (var preview in Previews.AllPreviews)
        {
            preview.Value.SetTexture(texture);
        }
    }

    public void Show(System.Action<DesignPattern.TypeEnum> confirm, System.Action cancel)
    {
        ConfirmCallback = confirm;
        CancelCallback = cancel;
    }

    void SwitchToTops()
    {
        if (CurrentMenu != Menu.Tops)
        {
            TopsTooltip.SetActive(true);
            DressesTooltip.SetActive(false);
            HatsTooltip.SetActive(false);
            LegacyTooltip.SetActive(false);
            Tops.SetActive(true);
            Dresses.SetActive(false);
            Hats.SetActive(false);
            Legacy.SetActive(false);
            TopsIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
            DressesIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            HatsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            LegacyIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            TopsIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
            DressesIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            HatsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            LegacyIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            CurrentMenu = Menu.Tops;
        }
    }

    void SwitchToDresses()
    {
        if (CurrentMenu != Menu.Dresses)
        {
            TopsTooltip.SetActive(false);
            DressesTooltip.SetActive(true);
            HatsTooltip.SetActive(false);
            LegacyTooltip.SetActive(false);
            Tops.SetActive(false);
            Dresses.SetActive(true);
            Hats.SetActive(false);
            Legacy.SetActive(false);
            TopsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            DressesIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f); 
            HatsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            LegacyIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            TopsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            DressesIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
            HatsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            LegacyIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            CurrentMenu = Menu.Dresses;
        }
    }

    void SwitchToHats()
    {
        if (CurrentMenu != Menu.Hats)
        {
            TopsTooltip.SetActive(false);
            DressesTooltip.SetActive(false);
            HatsTooltip.SetActive(true);
            LegacyTooltip.SetActive(false);
            Tops.SetActive(false);
            Dresses.SetActive(false);
            Hats.SetActive(true);
            Legacy.SetActive(false);
            TopsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            DressesIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            HatsIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
            LegacyIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            TopsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            DressesIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            HatsIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
            LegacyIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            CurrentMenu = Menu.Hats;
        }
    }

    void SwitchToLegacy()
    {
        if (CurrentMenu != Menu.Legacy)
        {
            TopsTooltip.SetActive(false);
            DressesTooltip.SetActive(false);
            HatsTooltip.SetActive(false);
            LegacyTooltip.SetActive(true);
            Tops.SetActive(false);
            Dresses.SetActive(false);
            Hats.SetActive(false);
            Legacy.SetActive(true);
            TopsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            DressesIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            HatsIcon.color = new Color(185f / 255f, 182f / 255f, 162f / 255f);
            LegacyIcon.color = new Color(228f / 255f, 107f / 255f, 137f / 255f);
            TopsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            DressesIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            HatsIcon.rectTransform.sizeDelta = new Vector2(50f, 50f);
            LegacyIcon.rectTransform.sizeDelta = new Vector2(75f, 75f);
            CurrentMenu = Menu.Legacy;
        }
    }
    // Update is called once per frame
    void Update()
    {
        
    }
}
