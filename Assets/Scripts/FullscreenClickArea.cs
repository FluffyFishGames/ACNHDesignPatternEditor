using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class FullscreenClickArea : MonoBehaviour, IPointerClickHandler
{
	private System.Action Callback;
	private bool Permanent = false;

	public void OnPointerClick(PointerEventData eventData)
	{
		if (!Permanent)
		{
			this.gameObject.SetActive(false);
			Callback?.Invoke();
		}
	}

	public void Show(System.Action callback)
	{
		this.gameObject.SetActive(true);
		Callback = callback;
	}

	public void ShowPermanent()
	{
		this.gameObject.SetActive(true);
		Permanent = true;
	}

	public void Hide()
	{
		Permanent = false;
		this.gameObject.SetActive(false);
	}
}
