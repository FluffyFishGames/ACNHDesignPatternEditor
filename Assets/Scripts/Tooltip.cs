using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Tooltip : MonoBehaviour
{
	private string LastText;
	public string Text;
	private TooltipContent Content;
	private RectTransform ContentTransform;
	private RectTransform MyTransform;
	public float ScreenOffset = 50f;
	private float LastOpenPhase = -1f;
	private float OpenPhase = 0f;
	private bool Opened = false;
	private CanvasGroup Group;

	public void Open()
	{
		if (!Opened)
		{
			Opened = true;
			OpenPhase = 0f;
		}
	}

	public void Close()
	{
		Opened = false;
	}

	void OnEnable()
	{
		Group = GetComponent<CanvasGroup>();
		Content = transform.Find("Content").GetComponent<TooltipContent>();
		ContentTransform = transform.Find("Content").GetComponent<RectTransform>();
		MyTransform = GetComponent<RectTransform>();
	}

    // Update is called once per frame
    void Update()
    {
		
		//Debug.Log(left + "|" + right + "|" + ContentTransform.rect.width + "|" + factor + "|" + center);

		if (LastText != Text)
		{
			Content.Text = Text;
			LastText = Text;
		}

		var canvasPosition = MyTransform.anchoredPosition;
		var left = canvasPosition.x - (ContentTransform.rect.width / 2f);
		var right = canvasPosition.x + (ContentTransform.rect.width / 2f);
		if (left < ScreenOffset)
		{
			ContentTransform.anchoredPosition = new Vector2((ScreenOffset - left), 0f);
		}
		else if (right < Controller.Instance.RectTransform.sizeDelta.x - ScreenOffset)
		{
			ContentTransform.anchoredPosition = new Vector2(0f, 0f);
		}
		else
		{
			ContentTransform.anchoredPosition = new Vector2(((Controller.Instance.RectTransform.sizeDelta.x - ScreenOffset)- right), 0f);
		}

		if (Opened && OpenPhase < 1f)
			OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 3f);
		if (!Opened && OpenPhase > 0f)
			OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 3f);

		if (LastOpenPhase != OpenPhase)
		{
			LastOpenPhase = OpenPhase;
			var scale = EasingFunction.EaseOutBack(0f, 1f, OpenPhase);
			MyTransform.localScale = new Vector3(scale, scale, 1f);
			Group.alpha = OpenPhase;
		}
	}
}
