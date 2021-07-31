using System.Collections;
using System.Collections.Generic;
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
	public IDesignPatternContainer CurrentSavegame;
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
	public UploadPattern UploadPattern;
	public PatternExchange PatternExchange;
	public IOperation CurrentOperation;
	public RectTransform TopLeftTransform;

	private AudioSource[] AudioSources = new AudioSource[16];
	public Animator TransitionAnimator;
	public Animator OnlineTransitionAnimator;
	public TMPro.TextMeshProUGUI OnlineTransitionLabel;
	public PatternEditor PatternEditor;
	public ClothSelector ClothSelector;

	private int CurrentAudioSource = 0;

	private RectTransform TooltipTransform;
	public State CurrentState = State.MainMenu;
	private bool SavegameSaving = false;
	private bool SavegameSaved = false;
	private DesignServer.Client CurrentClient;

	public enum State
	{
		MainMenu,
		PatternSelection,
		PatternEditor,
		PatternExchange,
		NameInput,
		UploadPattern,
		ClothSelector,
		Importer,
		Tutorial,
		None
	}

	public void MoveTooltip(Vector2 position)
	{
		try
		{
			if (this.CurrentState == State.None)
				HideTooltip();
			else 
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
			if (this.CurrentState == State.None)
				this.HideTooltip();
			else
			{
				TooltipTransform.anchoredPosition = position;
				Tooltip.Text = text;
				Tooltip.Open();
			}
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
			/*var client = new DesignServer.Client(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 9801));
			client.Connection.Write(new DesignServer.Messages.ListPatterns(new DesignServer.SearchQuery()
			{
				Phrase = "test"
			}));*/
			Popup.gameObject.SetActive(true);
			MainMenu.gameObject.SetActive(true);
			Tooltip.gameObject.SetActive(true);
			ConfirmationPopup.gameObject.SetActive(true);
			TransitionAnimator.gameObject.SetActive(true);
			NameInput.gameObject.SetActive(false);
			UploadPattern.gameObject.SetActive(false);
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

	private void OnApplicationQuit()
	{
		if (this.CurrentClient != null)
			this.CurrentClient.Close();
	}

	public void Save()
	{
		SavegameSaving = true;
		SavegameSaved = false;
		Thread t = new Thread(() =>
		{
			try
			{
				CurrentSavegame.Save();
				CurrentSavegame.Dispose();
				CurrentSavegame = null;
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

	IEnumerator PlayTransitionIn()
	{
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
		yield return StartCoroutine(PlayTransitionIn());
		
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

	public void SwitchToNameInput(System.Action confirm, System.Action cancel, string label = null)
	{
		StartCoroutine(ShowNameInput(confirm, cancel, label));
	}

	IEnumerator HidePatternEditor()
	{
		if (CurrentState == State.PatternEditor) 
		{
			CurrentState = State.None;
			try { PatternEditor.Hide(); }
			catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString()); }
			yield return new WaitForSeconds(0.2f);
		}
	}

	IEnumerator HideImporter()
	{
		if (CurrentState == State.Importer)
		{
			CurrentState = State.None;
			try { Importer.Hide(); }
			catch (System.Exception e) { Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString()); }
			yield return new WaitForSeconds(0.2f);
		}
	}


	IEnumerator ShowNameInput(System.Action confirm, System.Action cancel, string label = null)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to name input...");
		if (CurrentState == State.PatternEditor) yield return StartCoroutine(HidePatternEditor());
		if (CurrentState == State.Importer) yield return StartCoroutine(HideImporter());
		try
		{
			CurrentState = State.NameInput;
			UploadPattern.gameObject.SetActive(false);
			ClothSelector.gameObject.SetActive(false);
			PatternSelector.gameObject.SetActive(false);
			HideTooltip();
			NameInput.gameObject.SetActive(true);
			NameInput.Show(confirm, cancel, label);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to name input: " + e.ToString());
		}
	}

	public void SwitchToUploadPattern(System.Action<DesignServer.Pattern> confirm, System.Action cancel, DesignPattern pattern, string label = null)
	{
		StartCoroutine(ShowUploadPattern(confirm, cancel, pattern, label));
	}

	IEnumerator ConnectToServer()
	{
		CurrentState = State.None;
		//CurrentClient = new DesignServer.Client(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("127.0.0.1"), 9801));
		CurrentClient = new DesignServer.Client(new System.Net.IPEndPoint(System.Net.IPAddress.Parse("157.90.0.143"), 9801));
		int i = 1;
		float time = 0f;
		OnlineTransitionAnimator.SetTrigger("PlayTransitionIn");
		while (!CurrentClient.IsConnected)
		{
			if (CurrentClient.Error != null)
			{
				OnlineTransitionLabel.text = CurrentClient.Error;
				CurrentClient = null;
				yield return new WaitForSeconds(2f);
				break;
			}
			string label = "Connecting" + new string('.', i);
			OnlineTransitionLabel.text = label;
			i++;
			if (i > 3) i = 1;
			yield return new WaitForSeconds(0.2f);
			time += 0.2f;
		}
		if (CurrentClient != null)
			OnlineTransitionLabel.text = "Connected!";
		var left = 1.5f - time;
		if (left > 0f)
			yield return new WaitForSeconds(left);

		ClothSelector.gameObject.SetActive(false);
		PatternSelector.gameObject.SetActive(false);
		OnlineTransitionAnimator.SetTrigger("PlayTransitionOut");
	}

	IEnumerator HideUploadPattern()
	{
		if (CurrentState == State.UploadPattern)
		{
			UploadPattern.Hide();
			yield return new WaitForSeconds(0.2f);
			UploadPattern.gameObject.SetActive(false);
			CurrentState = State.None;
		}
		yield return new WaitForEndOfFrame();
	}

	IEnumerator ShowUploadPattern(System.Action<DesignServer.Pattern> confirm, System.Action cancel, DesignPattern pattern, string label = null)
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

		yield return StartCoroutine(ConnectToServer());
		if (CurrentClient == null) { cancel?.Invoke(); }
		else
		{
			yield return new WaitForSeconds(0.5f);
			try
			{
				CurrentState = State.UploadPattern;
				UploadPattern.gameObject.SetActive(true);
				UploadPattern.Client = CurrentClient;
				UploadPattern.Show(pattern, confirm, cancel, label);
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to upload pattern: " + e.ToString());
			}
		}
	}

	public void SwitchToPatternExchange(DesignServer.Pattern pattern, System.Action confirm)
	{
		StartCoroutine(ShowPatternExchange(pattern, confirm));
	}

	IEnumerator ShowPatternExchange(DesignServer.Pattern pattern, System.Action confirm)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to pattern exchange...");
		try
		{
			HideTooltip();
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern exchange: " + e.ToString());
		}
		yield return StartCoroutine(HideUploadPattern());
		try
		{
			CurrentState = State.PatternExchange;
			ClothSelector.gameObject.SetActive(false);
			PatternSelector.gameObject.SetActive(false);
			HideTooltip();
			PatternExchange.gameObject.SetActive(true);
			PatternExchange.ShowDesign(pattern, confirm);
		}
		catch (System.Exception e)
		{
			Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern exchange: " + e.ToString());
		}
	}


	public void SwitchToPatternExchange(bool proDesigns, System.Action<DesignServer.Pattern> confirm, System.Action cancel)
	{
		StartCoroutine(ShowPatternExchange(proDesigns, confirm, cancel));
	}

	IEnumerator ShowPatternExchange(bool proDesigns, System.Action<DesignServer.Pattern> confirm, System.Action cancel)
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to pattern exchange...");

		yield return StartCoroutine(ConnectToServer());
		if (CurrentClient == null)
		{
			cancel?.Invoke();
		}
		else
		{
			try
			{
				CurrentState = State.PatternExchange;
				ClothSelector.gameObject.SetActive(false);
				PatternSelector.gameObject.SetActive(false);
				PatternExchange.gameObject.SetActive(false);
				HideTooltip();
				PatternExchange.gameObject.SetActive(true);
				PatternExchange.Client = CurrentClient;
				PatternExchange.ShowItems(proDesigns, confirm, cancel);
			}
			catch (System.Exception e)
			{
				Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern exchange: " + e.ToString());
			}
		}
	}

	public void SwitchToPatternMenu()
	{
		StartCoroutine(ShowPatternSelector());
	}

	IEnumerator ShowPatternSelector()
	{
		Logger.Log(Logger.Level.INFO, "[Controller] Switching to pattern selector...");
		if (CurrentClient != null)
		{
			CurrentClient.Close();
			CurrentClient = null;
		}

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
			if (CurrentState == State.UploadPattern)
			{
				yield return StartCoroutine(HideUploadPattern());
			}
			if (CurrentState == State.PatternExchange)
			{
				try
				{
					PatternExchange.Hide();
				}
				catch (System.Exception e)
				{
					Logger.Log(Logger.Level.ERROR, "[Controller] Error while switching to pattern selector: " + e.ToString());
				}
				yield return new WaitForSeconds(1.5f);
			}
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
				UploadPattern.gameObject.SetActive(false);
				ClothSelector.gameObject.SetActive(false);
				PatternExchange.gameObject.SetActive(false);
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
