using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

[RequireComponent(typeof(EventTrigger))]
public class MenuButton : MonoBehaviour
{
	private EventTrigger Events;
	private RectTransform MyRectTransform;
	private bool IsMouseOver = false;
	private float HoverPhase = 0f;
	private RectTransform Strips;
	private GameObject Hover;
	private Image Background;
	private float StripsPhase = 0f;
	public delegate void ClickDelegate();
	public ClickDelegate OnClick;

	// Start is called before the first frame update
	void Start()
	{
		MyRectTransform = GetComponent<RectTransform>();
		Events = GetComponent<EventTrigger>();
		Hover = transform.Find("Hover").gameObject;
		Strips = Hover.transform.Find("Strips").GetComponent<RectTransform>();
		Background = transform.Find("Background").GetComponent<Image>();
		Hover.SetActive(false);

		var mouseOver = new EventTrigger.Entry();
		mouseOver.eventID = EventTriggerType.PointerEnter;
		mouseOver.callback.AddListener((eventData) => {
			Controller.Instance.PlayHoverSound();
			IsMouseOver = true;
		});
		Events.triggers.Add(mouseOver);

		var mouseOut = new EventTrigger.Entry();
		mouseOut.eventID = EventTriggerType.PointerExit;
		mouseOut.callback.AddListener((eventData) => {
			IsMouseOver = false;
		});
		Events.triggers.Add(mouseOut);

		var click = new EventTrigger.Entry();
		click.eventID = EventTriggerType.PointerClick;
		click.callback.AddListener((eventData) => {
			this.Click();
		});
		Events.triggers.Add(click);
	}

	private void Click()
	{
		Controller.Instance.PlayClickSound();
		OnClick?.Invoke();
	}

    // Update is called once per frame
    void Update()
    {
		if (IsMouseOver && HoverPhase < 1f)
			HoverPhase = Mathf.Min(1f, HoverPhase + Time.deltaTime * 3f);
		if (!IsMouseOver && HoverPhase > 0f)
			HoverPhase = Mathf.Max(0f, HoverPhase - Time.deltaTime * 3f);
		var scaleAdd = EasingFunction.EaseInOutBack(0f, 0.1f, HoverPhase);
		MyRectTransform.localScale = new Vector3(1f + scaleAdd, 1f + scaleAdd, 1f);

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
		}
	}
}
