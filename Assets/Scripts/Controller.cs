using MyHorizons.Data;
using MyHorizons.Data.Save;
using MyHorizons.Encryption;
using SFB;
using System.Collections;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using UnityEngine;
using ZXing;

public class Controller : MonoBehaviour
{
	public QRCode QRCode;
	public HowToExport Tutorial;
	public DesignPattern CurrentPattern;
	public static Controller Instance;
	public Popup Popup;
	public MainMenu MainMenu;
	public MainSaveFile CurrentSavegame;
	public PatternSelector PatternSelector;
	public Importer Importer;
	public RectTransform RectTransform;
	public Tooltip Tooltip;
	public ConfirmationPopup ConfirmationPopup;
	public FormatPopup FormatPopup;
	public AudioClip HoverSound;
	public AudioClip ClickSound;
	public AudioClip PopupSound;
	public AudioClip PopoutSound;
	public AudioClip AppOpenSound;
	public NameInput NameInput;
	public IOperation CurrentOperation;
	public RectTransform TopLeftTransform;

	private AudioSource[] AudioSources = new AudioSource[16];
	public Animator TransitionAnimator;
	public PatternEditor PatternEditor;
	public ClothSelector ClothSelector;

	private int CurrentAudioSource = 0;

	private RectTransform TooltipTransform;
	public State CurrentState = State.MainMenu;
	private bool SavegameSaving = false;
	private bool SavegameSaved = false;

	public enum State
	{
		MainMenu,
		PatternSelection,
		PatternEditor,
		NameInput,
		ClothSelector,
		Importer,
		Tutorial
	}

	public void MoveTooltip(Vector2 position)
	{
		TooltipTransform.anchoredPosition = position;
	}

	public void HideTooltip()
	{
		Tooltip.Close();
	}

	public void ShowTooltip(string text, Vector2 position)
	{
		TooltipTransform.anchoredPosition = position;
		Tooltip.Text = text;
		Tooltip.Open();
	}

	void OnEnable()
	{
		Application.targetFrameRate = 60;
		TooltipTransform = Tooltip.GetComponent<RectTransform>();
		RectTransform = GetComponent<RectTransform>();
		Instance = this;
	}

	public void PlaySound(AudioClip clip, float offset = 0f)
	{
		AudioSources[CurrentAudioSource].clip = clip;
		AudioSources[CurrentAudioSource].time = offset;
		AudioSources[CurrentAudioSource].Play();

		CurrentAudioSource++;
		if (CurrentAudioSource >= this.AudioSources.Length)
			CurrentAudioSource = 0;
	}
	public void PlayClickSound()
	{
		PlaySound(this.ClickSound, 0f);
	}

	public void PlayHoverSound()
	{
		PlaySound(this.HoverSound, 0f);
	}

	public void PlayPopupSound()
	{
		PlaySound(this.PopupSound, 0f);
	}

	public void PlayPopoutSound()
	{
		PlaySound(this.PopoutSound, 0f);
	}

	void Start()
	{
		Popup.gameObject.SetActive(true);
		MainMenu.gameObject.SetActive(true);
		Tooltip.gameObject.SetActive(true);
		ConfirmationPopup.gameObject.SetActive(true);
		TransitionAnimator.gameObject.SetActive(true);
		NameInput.gameObject.SetActive(false);
		PatternEditor.gameObject.SetActive(false);
		PatternSelector.gameObject.SetActive(false);
		Importer.gameObject.SetActive(false);
		MainMenu.Open();
		for (int i = 0; i < AudioSources.Length; i++)
		{
			AudioSources[i] = gameObject.AddComponent<AudioSource>();
			AudioSources[i].playOnAwake = false;
			AudioSources[i].loop = false;
			AudioSources[i].spatialBlend = 0f;
		}
    }

	public void Save()
	{
		SavegameSaving = true;
		SavegameSaved = false;
		Thread t = new Thread(() =>
		{
			CurrentSavegame.Save(null);
			SavegameSaved = true;
		});
		t.Start();
		StartCoroutine(DoSave());
	}

	IEnumerator DoSave()
	{
		PatternSelector.Close();
		yield return new WaitForSeconds(0.5f);
		Controller.Instance.Popup.SetText("<align=\"center\">Saving <#FF6666>savegame<#FFFFFF><s1>...<s10>\r\n\r\nPlease wait.", true);
		yield return new WaitForSeconds(3f);
		while (SavegameSaving && !SavegameSaved)
		{
			yield return new WaitForEndOfFrame();
		}
		SavegameSaving = false;
		Controller.Instance.Popup.Close();
		yield return new WaitForSeconds(0.3f);
		Controller.Instance.SwitchToMainMenu();
	}

	public void StartOperation(IOperation operation)
	{
		CurrentOperation = operation;
		CurrentOperation.Start();
	}

	public void SwitchToImporter(System.Drawing.Bitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<System.Drawing.Bitmap> confirm, System.Action cancel)
	{
		StartCoroutine(ShowImporter(bitmap, rect, resultSize, confirm, cancel));
	}

	IEnumerator ShowImporter(System.Drawing.Bitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<System.Drawing.Bitmap> confirm, System.Action cancel)
	{
		if (Popup.IsOpened)
		{
			Popup.Close();
			yield return new WaitForSeconds(0.5f);
		}
		HideTooltip();
		ClothSelector.gameObject.SetActive(false);
		Importer.Show(bitmap, rect, resultSize, confirm, cancel);
		CurrentState = State.Importer;
	}

	public void SwitchToPatternEditor(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		StartCoroutine(ShowPatternEditor(pattern, confirm, cancel));
	}

	IEnumerator ShowPatternEditor(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		if (Popup.IsOpened)
		{
			Popup.Close();
			yield return new WaitForSeconds(0.5f);
		}
		HideTooltip();
		TransitionAnimator.SetTrigger("PlayTransitionIn");
		yield return new WaitForSeconds(0.2f);
		HideTooltip();
		PlaySound(AppOpenSound, 0f);
		yield return new WaitForSeconds(0.3f);
		if (CurrentState == State.ClothSelector)
			ClothSelector.gameObject.SetActive(false);
		if (CurrentState == State.PatternSelection)
			PatternSelector.gameObject.SetActive(false);
		CurrentState = State.PatternEditor;
		PatternEditor.gameObject.SetActive(true);
		PatternEditor.Show(pattern, confirm, cancel);
	}

	public void SwitchToClothSelector(System.Action<DesignPattern.TypeEnum> confirm, System.Action cancel)
	{
		StartCoroutine(ShowClothSelector(confirm, cancel));
	}

	IEnumerator ShowClothSelector(System.Action<DesignPattern.TypeEnum> confirm, System.Action cancel)
	{
		if (Popup.IsOpened)
		{
			Popup.Close();
			yield return new WaitForSeconds(0.5f);
		}
		HideTooltip();
		TransitionAnimator.SetTrigger("PlayTransitionIn");
		yield return new WaitForSeconds(0.2f);
		HideTooltip();
		PlaySound(AppOpenSound, 0f);
		yield return new WaitForSeconds(0.3f);
		if (CurrentState == State.PatternSelection)
			PatternSelector.gameObject.SetActive(false);
		PatternEditor.gameObject.SetActive(false);
		CurrentState = State.ClothSelector;
		ClothSelector.gameObject.SetActive(true);
		ClothSelector.Show(confirm, cancel);
	}

	public void SwitchToNameInput(System.Action confirm, System.Action cancel)
	{
		StartCoroutine(ShowNameInput(confirm, cancel));
	}

	IEnumerator ShowNameInput(System.Action confirm, System.Action cancel)
	{
		HideTooltip();
		if (CurrentState == State.PatternEditor)
		{
			PatternEditor.Hide();
			yield return new WaitForSeconds(0.2f);
		}
		if (CurrentState == State.Importer)
		{
			Importer.Hide();
			yield return new WaitForSeconds(0.2f);
		}

		CurrentState = State.NameInput;
		ClothSelector.gameObject.SetActive(false);
		PatternSelector.gameObject.SetActive(false);
		HideTooltip();
		NameInput.gameObject.SetActive(true);
		NameInput.Show(confirm, cancel);
	}

	public void SwitchToPatternMenu()
	{
		StartCoroutine(ShowPatternSelector());
	}

	IEnumerator ShowPatternSelector()
	{
		if (CurrentState == State.MainMenu)
		{
			PatternSelector.gameObject.SetActive(true);
			PatternSelector.Open();
			CurrentState = State.PatternSelection;
		}
		else
		{
			if (CurrentState == State.NameInput)
			{
				NameInput.Hide();
				yield return new WaitForSeconds(0.2f);
			}
			CurrentState = State.PatternSelection;
			TransitionAnimator.SetTrigger("PlayTransitionOut");
			yield return new WaitForSeconds(0.25f);
			NameInput.gameObject.SetActive(false);
			ClothSelector.gameObject.SetActive(false);
			Importer.gameObject.SetActive(false);
			PatternEditor.Hide();
			PatternEditor.gameObject.SetActive(false);
			PatternSelector.gameObject.SetActive(true);
			PatternSelector.Open();
		}
	}

	public void SwitchToMainMenu()
	{
		if (CurrentState == State.Tutorial)
			Tutorial.gameObject.SetActive(false);
		if (CurrentState == State.PatternSelection)
			PatternSelector.Close();
		CurrentState = State.MainMenu;
		MainMenu.Open();
	}

	public void SwitchToTutorial()
	{
		StartCoroutine(ShowTutorial());
	}

	IEnumerator ShowTutorial()
	{
		if (CurrentState == State.MainMenu)
		{
			MainMenu.Close();
			CurrentState = State.Tutorial;
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		Tutorial.gameObject.SetActive(true);
		Tutorial.StartTutorial();		
 	}

	// Update is called once per frame
	void Update()
	{
		if (CurrentOperation != null && CurrentOperation.IsFinished())
		{
			if (CurrentState != State.PatternSelection)
				SwitchToPatternMenu();
			CurrentOperation = null;
		}
    }
}
