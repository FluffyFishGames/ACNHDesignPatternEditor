using MyHorizons.Data;
using MyHorizons.Data.Save;
using System.Collections;
using System.Threading;
using UnityEngine;

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
		try
		{
			TooltipTransform.anchoredPosition = position;
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while moving tooltip: " + e.ToString());
		}
	}

	public void HideTooltip()
	{
		try
		{
			Tooltip.Close();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while closing tooltip: " + e.ToString());
		}
	}

	public void ShowTooltip(string text, Vector2 position)
	{
		try
		{
			TooltipTransform.anchoredPosition = position;
			Tooltip.Text = text;
			Tooltip.Open();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while showing tooltip: " + e.ToString());
		}
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
		try
		{
			PlaySound(this.ClickSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while playing sound: " + e.ToString());
		}
	}

	public void PlayHoverSound()
	{
		try
		{
			PlaySound(this.HoverSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while playing sound: " + e.ToString());
		}
	}

	public void PlayPopupSound()
	{
		try
		{
			PlaySound(this.PopupSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while playing sound: " + e.ToString());
		}
	}

	public void PlayPopoutSound()
	{
		try
		{
			PlaySound(this.PopoutSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while playing sound: " + e.ToString());
		}
	}

	void Start()
	{
		try
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
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while initializing: " + e.ToString());
		}
	}

	public void Save()
	{
		SavegameSaving = true;
		SavegameSaved = false;
		Thread t = new Thread(() =>
		{
			try
			{
				CurrentSavegame.Save(null);
				SavegameSaved = true;
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while saving: " + e.ToString());
			}
		});
		t.Start();
		StartCoroutine(DoSave());
	}

	IEnumerator DoSave()
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Saving...");
		try
		{
			PatternSelector.Close();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while saving: " + e.ToString());
		}
		yield return new WaitForSeconds(0.5f);
		try
		{
			Controller.Instance.Popup.SetText("<align=\"center\">Saving <#FF6666>savegame<#FFFFFF><s1>...<s10>\r\n\r\nPlease wait.", true);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while saving: " + e.ToString());
		}
		yield return new WaitForSeconds(3f);
		while (SavegameSaving && !SavegameSaved)
		{
			yield return new WaitForEndOfFrame();
		}
		try
		{
			SavegameSaving = false;
			Controller.Instance.Popup.Close();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while saving: " + e.ToString());
		}
		yield return new WaitForSeconds(0.3f);
		try
		{
			Controller.Instance.SwitchToMainMenu();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while saving: " + e.ToString());
		}
	}

	public void StartOperation(IOperation operation)
	{
		CurrentOperation = operation;
		CurrentOperation.Start();
	}

	public void SwitchToImporter(TextureBitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<TextureBitmap> confirm, System.Action cancel)
	{
		StartCoroutine(ShowImporter(bitmap, rect, resultSize, confirm, cancel));
	}

	IEnumerator ShowImporter(TextureBitmap bitmap, (int, int, int, int) rect, (int, int) resultSize, System.Action<TextureBitmap> confirm, System.Action cancel)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to importer...");
		if (Popup.IsOpened)
		{
			try
			{
				Popup.Close();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to importer: " + e.ToString());
			}
			yield return new WaitForSeconds(0.5f);
		}
		try
		{
			HideTooltip();
			ClothSelector.gameObject.SetActive(false);
			Importer.Show(bitmap, rect, resultSize, confirm, cancel);
			CurrentState = State.Importer;
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to importer: " + e.ToString());
		}
	}

	public void SwitchToPatternEditor(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		StartCoroutine(ShowPatternEditor(pattern, confirm, cancel));
	}

	IEnumerator ShowPatternEditor(DesignPattern pattern, System.Action confirm, System.Action cancel)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to pattern editor...");
		if (Popup.IsOpened)
		{
			try
			{
				Popup.Close();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern editor: " + e.ToString());
			}
			yield return new WaitForSeconds(0.5f);
		}
		try
		{
			TransitionAnimator.SetTrigger("PlayTransitionIn");
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern editor: " + e.ToString());
		}
		yield return new WaitForSeconds(0.2f);
		try
		{
			PlaySound(AppOpenSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern editor: " + e.ToString());
		}
		yield return new WaitForSeconds(0.3f);
		try
		{
			if (CurrentState == State.ClothSelector)
				ClothSelector.gameObject.SetActive(false);
			if (CurrentState == State.PatternSelection)
				PatternSelector.gameObject.SetActive(false);
			CurrentState = State.PatternEditor;
			PatternEditor.gameObject.SetActive(true);
			PatternEditor.Show(pattern, confirm, cancel);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern editor: " + e.ToString());
		}
	}

	public void SwitchToClothSelector(System.Action<DesignPattern.TypeEnum> confirm, System.Action cancel)
	{
		StartCoroutine(ShowClothSelector(confirm, cancel));
	}

	IEnumerator ShowClothSelector(System.Action<DesignPattern.TypeEnum> confirm, System.Action cancel)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to cloth selector...");
		if (Popup.IsOpened)
		{
			try
			{
				Popup.Close();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to cloth selector: " + e.ToString());
			}
			yield return new WaitForSeconds(0.5f);
		}
		try
		{
			TransitionAnimator.SetTrigger("PlayTransitionIn");
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to cloth selector: " + e.ToString());
		}
		yield return new WaitForSeconds(0.2f);
		try
		{
			PlaySound(AppOpenSound, 0f);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to cloth selector: " + e.ToString());
		}
		yield return new WaitForSeconds(0.3f);
		try
		{
			if (CurrentState == State.PatternSelection)
				PatternSelector.gameObject.SetActive(false);
			PatternEditor.gameObject.SetActive(false);
			CurrentState = State.ClothSelector;
			ClothSelector.gameObject.SetActive(true);
			ClothSelector.Show(confirm, cancel);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to cloth selector: " + e.ToString());
		}
	}

	public void SwitchToNameInput(System.Action confirm, System.Action cancel)
	{
		StartCoroutine(ShowNameInput(confirm, cancel));
	}

	IEnumerator ShowNameInput(System.Action confirm, System.Action cancel)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to name input...");
		try
		{
			HideTooltip();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString());
		}
		if (CurrentState == State.PatternEditor)
		{
			try
			{
				PatternEditor.Hide();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString());
			}
			yield return new WaitForSeconds(0.2f);
		}
		if (CurrentState == State.Importer)
		{
			try
			{
				Importer.Hide();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString());
			}
			yield return new WaitForSeconds(0.2f);
		}
		try
		{
			CurrentState = State.NameInput;
			ClothSelector.gameObject.SetActive(false);
			PatternSelector.gameObject.SetActive(false);
			HideTooltip();
			NameInput.gameObject.SetActive(true);
			NameInput.Show(confirm, cancel);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString());
		}
	}

	public void SwitchToPatternMenu()
	{
		StartCoroutine(ShowPatternSelector());
	}

	IEnumerator ShowPatternSelector()
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to pattern selector...");
		if (CurrentState == State.MainMenu)
		{
			try
			{
				PatternSelector.gameObject.SetActive(true);
				PatternSelector.Open();
				CurrentState = State.PatternSelection;
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern selector: " + e.ToString());
			}
		}
		else
		{
			if (CurrentState == State.NameInput)
			{
				try
				{
					NameInput.Hide();
				}
				catch (System.Exception e)
				{
					Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern selector: " + e.ToString());
				}
				yield return new WaitForSeconds(0.2f);
			}
			try
			{
				CurrentState = State.PatternSelection;
				TransitionAnimator.SetTrigger("PlayTransitionOut");
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern selector: " + e.ToString());
			}
			yield return new WaitForSeconds(0.25f);
			try
			{
				NameInput.gameObject.SetActive(false);
				ClothSelector.gameObject.SetActive(false);
				Importer.gameObject.SetActive(false);
				PatternEditor.Hide();
				PatternEditor.gameObject.SetActive(false);
				PatternSelector.gameObject.SetActive(true);
				PatternSelector.Open();
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern selector: " + e.ToString());
			}
		}
	}

	public void SwitchToMainMenu()
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to main menu...");
		try
		{
			if (CurrentState == State.Tutorial)
				Tutorial.gameObject.SetActive(false);
			if (CurrentState == State.PatternSelection)
				PatternSelector.Close();
			CurrentState = State.MainMenu;
			MainMenu.Open();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to main menu: " + e.ToString());
		}
	}

	public void SwitchToTutorial()
	{
		StartCoroutine(ShowTutorial());
	}

	IEnumerator ShowTutorial()
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to tutorial...");
		if (CurrentState == State.MainMenu)
		{
			try
			{
				MainMenu.Close();
				CurrentState = State.Tutorial;
			} 
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to main menu: " + e.ToString());
			}
			yield return new WaitForSeconds(1f);
		}
		yield return new WaitForSeconds(1f);
		try
		{
			Tutorial.gameObject.SetActive(true);
			Tutorial.StartTutorial();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to main menu: " + e.ToString());
		}
	}

	// Update is called once per frame
	void Update()
	{
		try
		{
			if (CurrentOperation != null && CurrentOperation.IsFinished())
			{
				if (CurrentState != State.PatternSelection)
					SwitchToPatternMenu();
				CurrentOperation = null;
			}
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while updating: " + e.ToString());
		}
    }
}
