using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(CanvasGroup))]
public class Pop : MonoBehaviour
{
	private float PopPhase = 0f;
	private float OldPopPhase = 0f;
	private bool PoppingUp = false;
	private RectTransform MyRectTransform;
	private CanvasGroup Group;

    void OnEnable()
    {
		Group = GetComponent<CanvasGroup>();
		MyRectTransform = GetComponent<RectTransform>();
    }

	void Start()
	{
		MyRectTransform.localScale = new Vector3(0.1f, 0.1f, 1f);
		Group.alpha = 0f;
	}

	public void PopUp()
	{
		Controller.Instance.PlayPopupSound();
		PoppingUp = true;
	}

	public void PopOut()
	{
		PoppingUp = false;
	}

	void Update()
    {
		if (PoppingUp && PopPhase < 1f)
			PopPhase = Mathf.Min(1f, PopPhase + Time.deltaTime * 3f);
		if (!PoppingUp && PopPhase > 0f)
			PopPhase = Mathf.Max(0f, PopPhase - Time.deltaTime * 3f);
		if (OldPopPhase != PopPhase)
		{
			OldPopPhase = PopPhase;
			var scale = EasingFunction.EaseOutBack(0f, 1f, PopPhase);
			MyRectTransform.localScale = new Vector3(scale, scale, 1f);
			Group.alpha = PopPhase;
		}
    }
}
