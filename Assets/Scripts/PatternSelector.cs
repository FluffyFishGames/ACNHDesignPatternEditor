using MyHorizons.Data;
using SFB;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PatternSelector : MonoBehaviour
{
	public RectTransform PanelLeft;
	public Transform Patterns;
	public GameObject PatternPrefab;
	public ActionMenu ActionMenu;
	public GameObject Save;
	public GameObject Cancel;
	public GameObject MainButtons;
	public GameObject CloneSwapButtons;
	private Pop SavePop;
	private Pop CancelPop;
	private MenuButton SaveButton;
	private MenuButton CancelButton;
	public MenuButton CancelCloneSwapButton;

	private bool IsOpened = false;
	private float OpenPhase = 0f;
	private float LastOpenPhase = -1f;
	private PatternSelectorPattern Selected = null;
	private PatternSelectorPattern[] PatternObjects = new PatternSelectorPattern[50];

	// Start is called before the first frame update
	void OnEnable()
    {
		SavePop = Save.GetComponent<Pop>();
		CancelPop = Cancel.GetComponent<Pop>();
		SaveButton = Save.transform.GetChild(0).GetComponent<MenuButton>();
		CancelButton = Cancel.transform.GetChild(0).GetComponent<MenuButton>();
	}

	void Start()
	{
		CloneSwapButtons.SetActive(false);
		SaveButton.OnClick += () =>
		{
			Controller.Instance.Save();
		};
		CancelButton.OnClick += () =>
		{
			Controller.Instance.SwitchToMainMenu();
		};
		CancelCloneSwapButton.OnClick = () =>
		{
			Controller.Instance.CurrentOperation.Abort();
		};
	}

	public void Close()
	{
		IsOpened = false;
		StartCoroutine(DoClose());
	}

	public void Open()
	{
		for (var i = Patterns.childCount - 1; i >= 0; i--)
			DestroyImmediate(Patterns.GetChild(i).gameObject);

		for (var i = 0; i < Controller.Instance.CurrentSavegame.DesignPatterns.Length; i++)
		{
			var newObj = GameObject.Instantiate(PatternPrefab, Patterns);
			PatternObjects[i] = newObj.GetComponent<PatternSelectorPattern>();
			PatternObjects[i].PatternSelector = this;
			PatternObjects[i].SetPattern(Controller.Instance.CurrentSavegame.DesignPatterns[i]);
		}

		if (!IsOpened)
		{
			IsOpened = true;
			StartCoroutine(DoOpen());
		}
	}

	IEnumerator DoClose()
	{
		Controller.Instance.PlayPopoutSound();
		ActionMenu.Close();
		CancelPop.PopOut();
		yield return new WaitForSeconds(0.1f);
		SavePop.PopOut();
	}

	IEnumerator DoOpen()
	{
		SavePop.PopUp();
		yield return new WaitForSeconds(0.1f);
		CancelPop.PopUp();
	}

	public void SelectPattern(PatternSelectorPattern pattern)
	{
		if (Controller.Instance.CurrentOperation != null && Controller.Instance.CurrentOperation is IPatternSelectorOperation patternSelectorOperation)
		{
			patternSelectorOperation.SelectPattern(pattern.Pattern);
		}
		else
		{
			if (Selected != null)
				Selected.Unselect();
			Selected = pattern;
			pattern.Select();
			ActionMenu.ShowActions(
				new (string, System.Action)[]
				{
					/*("Edit design", () => { }),*/
					("Delete design", () => {
						Controller.Instance.StartOperation(new DeleteOperation(Selected.Pattern));
					}),
					("Clone design", () => {
						Controller.Instance.StartOperation(new CloneOperation(Selected.Pattern));
					}),
					("Swap design", () => {
						Controller.Instance.StartOperation(new SwapOperation(Selected.Pattern));
					}),
					("Import design", () => {
						Controller.Instance.StartOperation(new ImportOperation(Selected.Pattern));
					}),
					("Export design", () => {
						Controller.Instance.StartOperation(new ExportOperation(Selected.Pattern));
					})
				}
			);
		}
	}

	private bool OperationRunning = false;

    // Update is called once per frame
    void Update()
    {
		if (Controller.Instance.CurrentOperation != null)
		{
			OperationRunning = true;
			ActionMenu.Close();
			if (Controller.Instance.CurrentOperation is IPatternSelectorOperation && Controller.Instance.CurrentOperation is IPatternOperation patternOperation)
			{
				var pattern = patternOperation.GetPattern();
				for (int i = 0; i < PatternObjects.Length; i++)
				{
					if (PatternObjects[i].Pattern == pattern)
						PatternObjects[i].Highlight();
					else
						PatternObjects[i].Unhighlight();
				}
				MainButtons.SetActive(false);
				CloneSwapButtons.SetActive(true);
			}
		}
		else
		{
			if (OperationRunning)
			{
				MainButtons.SetActive(true);
				CloneSwapButtons.SetActive(false);
				for (int i = 0; i < PatternObjects.Length; i++)
				{
					PatternObjects[i].SetPattern(PatternObjects[i].Pattern);
					PatternObjects[i].Unhighlight();
					PatternObjects[i].Unselect();
				}
				OperationRunning = false;
			}
		}
		if (IsOpened && OpenPhase < 1f)
			OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 3f);
		if (!IsOpened && OpenPhase > 0f)
			OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 3f);
		if (OpenPhase != LastOpenPhase)
		{
			LastOpenPhase = OpenPhase;
			float x = EasingFunction.EaseOutBack(-750f, 50f, OpenPhase);
			PanelLeft.anchoredPosition = new Vector2(x, PanelLeft.anchoredPosition.y);
		}
	}
}
