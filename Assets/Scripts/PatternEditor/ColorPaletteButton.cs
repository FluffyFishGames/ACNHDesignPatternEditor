using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class ColorPaletteButton : MonoBehaviour
{
	[HideInInspector]
	public ColorPalette Palette;
	[HideInInspector]
	public int Index;
	[HideInInspector]
	public PatternEditor Editor;

	public Button EditButton;

	private Image EditButtonImage;
	private EventTrigger Events;
	private RectTransform Container;
	private RectTransform Selected;
	private Image Color;
	private bool IsSelected;
	private float Phase;
	private const float InAnimationTime = 0.1f;
	private const float OutAnimationTime = 0.25f;
	private void Start()
	{
		EditButton.onClick.AddListener(() => {
			Editor.OpenColorEditor(this);
		});
		EditButtonImage = EditButton.GetComponent<Image>();
	}
	void OnEnable()
    {
		Events = GetComponent<EventTrigger>();

		var click = new EventTrigger.Entry();
		click.eventID = EventTriggerType.PointerClick;
		click.callback.AddListener((eventData) => {
			this.Palette.ChangeColor(this.Index);
		});
		Events.triggers.Add(click);

		Selected = this.transform.Find("Selected").GetComponent<RectTransform>();
		Container = this.transform.Find("Container").GetComponent<RectTransform>();
		Color = Container.Find("Color").GetComponent<Image>();
		
		Selected.localScale = new Vector3(0f, 1f, 1f);
		Container.localScale = new Vector3(0.85f, 0.85f, 1f);
	}

	public void HighlightEdit()
	{
		EditButtonImage.color = new Color(1f, 1f, 1f);
	}

	public void UnhighlightEdit()
	{
		EditButtonImage.color = new Color(212f / 255f, 135f / 255f, 155f / 255f);
	}

	public void Select()
	{
		IsSelected = true;
	}

	public void Deselect()
	{
		IsSelected = false;
	}

	public void SetColor(Color color)
	{
		Color.color = color;
	}

	public Color GetColor()
	{
		return Color.color;
	}

	// Update is called once per frame
	void Update()
    {
		var oldPhase = Phase;
		if (IsSelected && Phase < 1f)
			Phase = Mathf.Min(1f, Phase + (Time.deltaTime / InAnimationTime));
		if (!IsSelected && Phase > 0f)
			Phase = Mathf.Max(0f, Phase - (Time.deltaTime / OutAnimationTime));

		if (oldPhase != Phase)
		{
			Container.localScale = new Vector3(0.85f + Phase * 0.15f, 0.85f + Phase * 0.15f, 1f);
			Selected.localScale = new Vector3(Phase, 1f, 1f);
		}
	}
}
