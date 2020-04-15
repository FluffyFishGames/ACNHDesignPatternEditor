using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

class LayerComponent : MonoBehaviour, IPointerClickHandler
{
	public TMPro.TextMeshProUGUI LabelName;
	public UnityEngine.UI.Image Image;
	public Layers Layers;
	public PatternEditor Editor;
	public EventTrigger DragHandle;
	public GameObject SmartObject;

	private Layer Layer;

	
	public void OnPointerClick(PointerEventData eventData)
	{
		Editor.CurrentPattern.CurrentSubPattern.SelectLayer(this.Layer);
	}

	void Start()
	{
		var mouseDown = new EventTrigger.Entry();
		mouseDown.eventID = EventTriggerType.PointerDown;
		mouseDown.callback.AddListener((eventData) => {
			Layers.DragLayer(this.Layer);
		});
		DragHandle.triggers.Add(mouseDown);
	}

	public void SetLayer(Layer layer)
	{
		SmartObject.SetActive(layer is SmartObjectLayer);
		this.Layer = layer;
		LabelName.text = layer.Name;
		LabelName.color = layer.IsSelected ? new Color(98f / 255f, 80f / 255f, 66f / 255f) : new Color(212f / 255f, 135f / 255f, 155f / 255f);
		Image.sprite = layer.Sprite;
	}
}
