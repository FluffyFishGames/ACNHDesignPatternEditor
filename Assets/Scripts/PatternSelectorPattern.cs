using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using MyHorizons.Data;

public class PatternSelectorPattern : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public DesignPattern Pattern;
	public PatternSelector PatternSelector;
	private RectTransform TransparencyTransform;
	private RectTransform TransparencyBackgroundTransform;
	private RectTransform ImageTransform;
	private Image ImageImage;
	private Image BorderImage;
	private RectTransform MyTransform;
	private EventTrigger Events;
	private bool IsMouseOver;
	private float HoverPhase = 0f;
	private string Name;
	private bool IsSelected;
	private RectTransform SelectionImageTransform;
	private RectTransform SelectionBorderTransform;
	private float SelectionPhase = 0f;
	private bool IsHighlighted = false;
	private float HoverAudioPause = 0f;
	private bool Initialized = false;
	private Sprite Preview;
	public TooltipHandler TooltipHandler;

	// Start is called before the first frame update
	void OnEnable()
    {
		if (!Initialized)
		{
			Initialize();
		}
	}

	void Initialize()
	{
		Initialized = true;
		MyTransform = GetComponent<RectTransform>();
		TransparencyTransform = transform.Find("Transparency").GetComponent<RectTransform>();
		TransparencyBackgroundTransform = transform.Find("TransparencyBackground").GetComponent<RectTransform>();
		ImageTransform = transform.Find("Image").GetComponent<RectTransform>();
		ImageImage = transform.Find("Image").GetComponent<Image>();
		BorderImage = transform.Find("Border").GetComponent<Image>();
		BorderImage.color = new Color(BorderImage.color.r, BorderImage.color.g, BorderImage.color.b, 0f);
		SelectionBorderTransform = transform.Find("SelectionBorder").GetComponent<RectTransform>();
		SelectionImageTransform = SelectionBorderTransform.transform.Find("SelectionImage").GetComponent<RectTransform>();
		SelectionBorderTransform.gameObject.SetActive(false);
	}

	public void SetPattern(DesignPattern pattern)
	{
		try
		{
			if (this.Pattern != null)
			{
				GameObject.DestroyImmediate(Preview.texture);
				GameObject.DestroyImmediate(Preview);
			}
			this.Pattern = pattern;
			this.Name = pattern.Name;
			TooltipHandler.Tooltip = this.Name;
			Preview = pattern.GetPreview();
			ImageImage.sprite = Preview;
		}
		catch (System.Exception e)
		{
			System.IO.File.AppendAllText("error.log", "\r\n" + e.ToString());
		}
	}

	public void Highlight()
	{
		IsHighlighted = true;
		SelectionBorderTransform.gameObject.SetActive(true);
	}

	public void Unhighlight()
	{
		IsHighlighted = false;
		SelectionBorderTransform.gameObject.SetActive(false);
	}

	public void Select()
	{
		IsSelected = true;
	}

	public void Unselect()
	{
		IsSelected = false;
	}

	// Update is called once per frame
	void Update()
    {
		if (HoverAudioPause > 0f)
			HoverAudioPause -= Time.deltaTime;

		SelectionPhase += Time.deltaTime;
		if (SelectionPhase > 1f)
			SelectionPhase -= 1f;

		SelectionImageTransform.anchoredPosition = new Vector2(-20f + 20f * SelectionPhase, 0f);

		if (IsSelected || IsHighlighted)
			HoverPhase = 1f;
		else
		{
			if (IsMouseOver && HoverPhase < 1f)
				HoverPhase = Mathf.Min(1f, HoverPhase + Time.deltaTime * 5f);
			if (!IsMouseOver && HoverPhase > 0f)
				HoverPhase = Mathf.Max(0f, HoverPhase - Time.deltaTime * 3f);
		}
		var scaleAdd = EasingFunction.EaseOutBack(0f, 0.2f, HoverPhase);
		MyTransform.localScale = new Vector3(1f + scaleAdd, 1f + scaleAdd, 1f);
		ImageTransform.localScale = new Vector3(1f - scaleAdd / 2f, 1f - scaleAdd / 2f, 1f);
		SelectionBorderTransform.localScale = new Vector3(1f - scaleAdd / 2f, 1f - scaleAdd / 2f, 1f);
		TransparencyTransform.localScale = new Vector3(1f - scaleAdd / 2f, 1f - scaleAdd / 2f, 1f);
		TransparencyBackgroundTransform.localScale = new Vector3(1f - scaleAdd / 2f, 1f - scaleAdd / 2f, 1f);
		BorderImage.color = new Color(BorderImage.color.r, BorderImage.color.g, BorderImage.color.b, HoverPhase);

		if (IsMouseOver)
		{
			var pos = Controller.Instance.RectTransform.InverseTransformPoint(MyTransform.position);
			pos.x += Controller.Instance.RectTransform.rect.width / 2f;
			pos.y -= Controller.Instance.RectTransform.rect.height / 2f - MyTransform.rect.height / 1.5f;
			Controller.Instance.MoveTooltip(pos);
		}
		/*
		StripsPhase += Time.deltaTime * 2f;
		if (StripsPhase > 1f)
			StripsPhase -= 1f;
		Strips.anchoredPosition = new Vector2(-60 + 60 * StripsPhase, 0f);
		if (IsMouseOver)
		{
			Background.color = new Color(90f / 255f, 240f / 255f, 212f / 255f);
			Hover.SetActive(true);
		}
		else
		{
			Background.color = new Color(31f / 255f, 217f / 255f, 181f / 255f);
			Hover.SetActive(false);
		}*/
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Controller.Instance.PlayHoverSound();

		IsMouseOver = true;
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		HoverAudioPause = 0.2f;
		Controller.Instance.HideTooltip();
		IsMouseOver = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		if (HoverAudioPause <= 0f)
			Controller.Instance.PlayClickSound();
		
		PatternSelector.SelectPattern(this);
	}
}
