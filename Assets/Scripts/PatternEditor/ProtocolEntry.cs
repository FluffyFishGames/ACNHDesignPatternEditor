using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine.EventSystems;

public class ProtocolEntry : UnityEngine.MonoBehaviour, UnityEngine.EventSystems.IPointerClickHandler
{
	public int Index;
	public TMPro.TextMeshProUGUI Text;
	public UnityEngine.UI.Image Background;
	public System.Action OnClick;

	public void OnPointerClick(PointerEventData eventData)
	{
		OnClick?.Invoke();
	}

	public void SetHighlighted(bool highlighted)
	{
		Background.color = highlighted ? new UnityEngine.Color(243f / 255f, 199f / 255f, 209f / 255f, 1f) : new UnityEngine.Color(244f / 255f, 183f / 255f, 197f / 255f, 1f);
	}
}
