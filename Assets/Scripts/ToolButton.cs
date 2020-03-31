using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ToolButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
	public TMPro.TextMeshProUGUI Label;
	public System.Action OnClick;
	private bool IsMouseOver = false;
	private float HoverPhase = 0f;
	private RectTransform MyTransform;
	private bool IsSelected = false;
	private static Color SelectedColor = new Color(98f / 255f, 80f / 255f, 60f / 255f);
	private static Color NormalColor = new Color(224f / 255f, 167f / 255f, 180f / 255f);

	void OnEnable()
	{
		MyTransform = GetComponent<RectTransform>();
	}

	public void Select()
	{
		IsSelected = true;
	}

	public void Unselect()
	{
		IsSelected = false;
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick?.Invoke();
		Controller.Instance.PlayClickSound();
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		if (!IsSelected)
		{
			IsMouseOver = true;
			if (HoverSoundPause <= 0f)
			{
				Controller.Instance.PlayHoverSound();
			}
		}
	}

	private float HoverSoundPause = 0f;
	public void OnPointerExit(PointerEventData eventData)
	{
		HoverSoundPause = 0.2f;
		IsMouseOver = false;
	}

    // Update is called once per frame
    void Update()
    {
		if (HoverSoundPause > 0f)
			HoverSoundPause -= Time.deltaTime;

		if (IsSelected)
			IsMouseOver = false;
		if (IsMouseOver && HoverPhase < 1f)
			HoverPhase = Mathf.Min(1f, HoverPhase + Time.deltaTime * 3f);
		if (!IsMouseOver && HoverPhase > 0f)
			HoverPhase = Mathf.Max(0f, HoverPhase - Time.deltaTime * 5f);
		var scaleAdd = EasingFunction.EaseOutBack(0f, 0.1f, HoverPhase);
		MyTransform.localScale = new Vector3(1f + scaleAdd, 1f + scaleAdd, 1f);

		if (IsSelected)
			Label.color = SelectedColor;
		else
			Label.color = NormalColor;

	}
}
