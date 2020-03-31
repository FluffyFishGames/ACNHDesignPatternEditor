using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ToolButtonGroup : MonoBehaviour
{
	public TMPro.TextMeshProUGUI Title;
	public GameObject ButtonPrefab;
	public Transform ButtonContainer;
	private ToolButton[] Buttons;
	private ToolButton Selected;
	public void SetItems((string, System.Action)[] items)
	{
		if (this.Buttons != null)
			for (int i = 0; i < this.Buttons.Length; i++)
				DestroyImmediate(this.Buttons[i].gameObject);

		Buttons = new ToolButton[items.Length];
		for (int i = 0; i < items.Length; i++)
		{
			var go = GameObject.Instantiate(ButtonPrefab, this.transform);
			Buttons[i] = go.GetComponent<ToolButton>();
			Buttons[i].Label.text = items[i].Item1;
			var callback = items[i].Item2;
			var button = Buttons[i];
			Buttons[i].OnClick = () => { 
				if (Selected != null) 
					Selected.Unselect(); 
				callback(); 
				Selected = button;
				Selected.Select();
			};
		}

		Selected = Buttons[0];
		Buttons[0].Select();
		LayoutRebuilder.ForceRebuildLayoutImmediate(GetComponent<RectTransform>());
	}

	public void Select(int index)
	{
		if (Selected != null)
		{
			Selected.Unselect();
		}
		Buttons[index].Select();
		Selected = Buttons[index];
	}
}
