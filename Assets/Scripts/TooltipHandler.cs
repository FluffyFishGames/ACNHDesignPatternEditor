using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TooltipHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public string Tooltip;
	public Vector2 Offset;
	private bool TooltipShown = false;

	public void OnPointerEnter(PointerEventData eventData)
	{
		TooltipShown = true;
		var rectTransform = this.GetComponent<RectTransform>();
		var pos = Controller.Instance.TopLeftTransform.InverseTransformPoint(rectTransform.TransformPoint(Offset));
		Controller.Instance.ShowTooltip(Tooltip, pos);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		TooltipShown = false;
		Controller.Instance.HideTooltip();
	}

	void Update()
	{
		if (TooltipShown)
		{
			var rectTransform = this.GetComponent<RectTransform>();
			var pos = Controller.Instance.TopLeftTransform.InverseTransformPoint(rectTransform.TransformPoint(Offset));
			Controller.Instance.ShowTooltip(Tooltip, pos);
		}
	}

	void OnDisable()
	{
		if (TooltipShown)
		{
			TooltipShown = false;
			Controller.Instance.HideTooltip();
		}
	}
}
