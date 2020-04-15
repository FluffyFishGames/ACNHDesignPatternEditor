using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FormatPopup : MonoBehaviour
{
	public Pop CancelPop;
	public GameObject ACNH;
	public GameObject ACNL;
	public GameObject QR;
	public GameObject Image;
	public MenuButton ACNHButton;
	public MenuButton ACNLButton;
	public MenuButton QRButton;
	public MenuButton ImageButton;
	public MenuButton CancelButton;

	public CanvasGroup BackgroundCanvasGroup;
	public TMPro.TextMeshProUGUI Text;

	public Pop PopupPop;
	private System.Action<Format> Callback;
	private System.Action CancelCallback;

	private bool IsOpen = false;
	private float OpenPhase = 0f;

	public enum Format
	{
		ACNH,
		ACNL,
		QR,
		Image
	}

	void OnEnable()
	{
	}

	void Start()
	{
		BackgroundCanvasGroup.alpha = 0f;
		CancelButton.OnClick = () => {
			CancelCallback?.Invoke();
			Hide();
		};
		ACNLButton.OnClick = () =>
		{
			Callback?.Invoke(Format.ACNL);
			Hide();
		};
		ACNHButton.OnClick = () =>
		{
			Callback?.Invoke(Format.ACNH);
			Hide();
		};
		QRButton.OnClick = () =>
		{
			Callback?.Invoke(Format.QR);
			Hide();
		};
		ImageButton.OnClick = () =>
		{
			Callback?.Invoke(Format.Image);
			Hide();
		};
	}

	public void Hide()
	{
		if (IsOpen)
		{
			Controller.Instance.PlayPopoutSound();
			IsOpen = false;
			StartCoroutine(Close());
		}
	}


	public void Show(string text, System.Action<Format> callback, System.Action cancel, bool showACNH, bool showACNL, bool showQR, bool showImage)
	{
		if (!IsOpen)
		{
			IsOpen = true;
			CancelCallback = cancel;
			Callback = callback;
			ACNH.SetActive(showACNH);
			ACNL.SetActive(showACNL);
			QR.SetActive(showQR);
			Image.SetActive(showImage);
			Text.text = text;

			gameObject.SetActive(true);
			StartCoroutine(Open());
		}
	}

	IEnumerator Open()
	{
		PopupPop.PopUp();
		yield return new WaitForSeconds(0.1f);

		CancelPop.PopUp();
	}

	IEnumerator Close()
	{
		CancelPop.PopOut();
		yield return new WaitForSeconds(0.1f);
		PopupPop.PopOut();
		yield return new WaitForSeconds(0.4f);
		gameObject.SetActive(false);
	}

	// Update is called once per frame
	void Update()
	{
		if (IsOpen && OpenPhase < 1f)
			OpenPhase = Mathf.Min(1f, OpenPhase + Time.deltaTime * 2f);
		if (!IsOpen && OpenPhase > 0f)
			OpenPhase = Mathf.Max(0f, OpenPhase - Time.deltaTime * 2f);
		BackgroundCanvasGroup.alpha = OpenPhase;
	}
}
