using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class NameInput : MonoBehaviour
{
	public TMP_InputField InputField;
	private Animator Animator;
	private System.Action Confirm;
	private System.Action Cancel;
	public MenuButton ConfirmButton;
	public MenuButton CancelButton;
	public TMPro.TextMeshProUGUI Label;

	void OnEnable()
	{
		Animator = GetComponent<Animator>();
	}

	void Start()
	{
		ConfirmButton.OnClick = () => {
			Confirm?.Invoke();
		};

		CancelButton.OnClick = () => {
			Cancel?.Invoke();
		};
		InputField.onValueChanged.AddListener((value) =>
		{
/*			if (Controller.Instance.CurrentOperation is IChangeNameOperation changeNameOption)
			{
				changeNameOption.SetName(value);
			}*/
		});
	}

	public string GetName()
	{
		return InputField.text;
	}

	public void SetName(string name)
	{
		InputField.text = name;
	}

	public void Show(System.Action confirm, System.Action cancel, string label = null)
	{
		if (label == null)
			Label.text = "Please enter a name for your design!";
		else
			Label.text = label;
		Confirm = confirm;
		Cancel = cancel;
		Animator.SetTrigger("TransitionIn");
		if (Controller.Instance.CurrentOperation is IPatternOperation patternOperation)
		{
			InputField.text = patternOperation.GetPattern().Name;
		}
		InputField.Select();
		InputField.ActivateInputField();
	}

	public void Hide()
	{
		Animator.SetTrigger("TransitionOut");
	}
}
