using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ActionMenu : MonoBehaviour
{
	public GameObject ButtonPrefab;
	public GameObject Hand;
	private RectTransform HandTransform;
	private CanvasRenderer HandRenderer;

	private Pop Pop;
	private GameObject[] Buttons;
	private float StartY = 0;
	private RectTransform MyRectTransform;
	private bool Opened = false;
	private Coroutine CurrentRoutine;

	public bool IsOpened
	{
		get
		{
			return Opened;
		}
	}
	
	void OnEnable()
	{
		MyRectTransform = GetComponent<RectTransform>();
		Pop = GetComponent<Pop>();
		HandRenderer = Hand.GetComponent<CanvasRenderer>();
		HandTransform = Hand.GetComponent<RectTransform>();
		HandRenderer.SetAlpha(0f);
	}

	void Start()
	{
		StartY = MyRectTransform.anchoredPosition.y + MyRectTransform.rect.height / 2f;
	}

	public void ShowActions((string, System.Action)[] actions)
	{
		if (CurrentRoutine != null)
			StopCoroutine(CurrentRoutine);

		if (!Opened)
		{
			CreateButtons(actions);
		}

		CurrentRoutine = StartCoroutine(Open(actions));
	}

	public void Close()
	{
		if (Opened)
		{
			Controller.Instance.PlayPopoutSound();
			Opened = false;
			Pop.PopOut();
		}
	}

	void CreateButtons((string, System.Action)[] actions)
	{
		if (Buttons != null)
			for (int i = 0; i < Buttons.Length; i++)
				GameObject.DestroyImmediate(Buttons[i]);

		Buttons = new GameObject[actions.Length];
		for (int i = 0; i < actions.Length; i++)
		{
			Buttons[i] = GameObject.Instantiate(ButtonPrefab, this.transform);
			var button = Buttons[i].GetComponent<ActionMenuButton>();
			button.Menu = this;
			button.Index = i;
			button.Text = actions[i].Item1;
			button.OnClick = actions[i].Item2;
		}
	}

	public void MouseOver(ActionMenuButton button)
	{
		HandTransform.anchoredPosition = new Vector2(0f, button.GetComponent<RectTransform>().anchoredPosition.y);
		HandRenderer.SetAlpha(1f);
	}

	public void MouseOut(ActionMenuButton button)
	{
		HandTransform.anchoredPosition = new Vector2(0f, button.GetComponent<RectTransform>().anchoredPosition.y);
		HandRenderer.SetAlpha(0f);
	}

	void Update()
	{
		MyRectTransform.anchoredPosition = new Vector2(MyRectTransform.anchoredPosition.x, StartY - MyRectTransform.rect.height / 2f);
	}

	IEnumerator Open((string, System.Action)[] actions)
	{
		if (Opened)
		{
			Pop.PopOut();
			Controller.Instance.PlayPopoutSound();
			yield return new WaitForSeconds(0.4f);
			CreateButtons(actions);
		}
		Controller.Instance.PlayPopupSound();
		Opened = true;
		Pop.PopUp();
		yield return new WaitForSeconds(0.2f);
		for (int i = 0; i < Buttons.Length; i++)
		{
			yield return new WaitForSeconds(0.1f);
			Buttons[i].GetComponent<Pop>().PopUp();
		}
	}

}
