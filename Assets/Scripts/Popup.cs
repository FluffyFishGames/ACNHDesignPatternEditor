using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Popup : MonoBehaviour
{
	public AudioClip TextSoundClip;
	public FullscreenClickArea FullScreenClickArea;
	private RectTransform Loading;
	private TMPro.TextMeshProUGUI Text;
	private List<(string, float)> CurrentChars = new List<(string, float)>();
	private int CurrentChar = 0;
	private const float StandardSpeed = 10f;
	private const float MaxTime = 0.25f;
	private float TimeLeft = 0f;
	private string PreviousDebugText = "";
	private bool ShowLoading = false;
	private float LoadingPhase = 0f;
	[TextArea]
	public string DebugText = "";
	public bool IsOpened
	{
		get { return Opened; }
	}
	private bool Opened = false;
	private float OpeningPhase = 0f;
	private RectTransform MyRectTransform;
	private CanvasRenderer MyRenderer;
	private bool SpeedUp = false;
	private System.Func<bool> Callback;
	private GameObject Arrow;

	private AudioSource[] AudioSources = new AudioSource[4];
	private int CurrentAudioSource = 0;
	void Start()
	{
		for (int i = 0; i < AudioSources.Length; i++)
		{
			AudioSources[i] = gameObject.AddComponent<AudioSource>();
			AudioSources[i].playOnAwake = false;
			AudioSources[i].clip = TextSoundClip;
			AudioSources[i].loop = false;
			AudioSources[i].spatialBlend = 0f;
		}
	}

	void PlaySound(float offset)
	{
		AudioSources[CurrentAudioSource].time = offset;
		AudioSources[CurrentAudioSource].Play();
		CurrentAudioSource++;
		if (CurrentAudioSource >= AudioSources.Length)
			CurrentAudioSource = 0;
	}
	// Start is called before the first frame update
	void OnEnable()
    {
		MyRenderer = this.GetComponent<CanvasRenderer>();
		MyRectTransform = this.GetComponent<RectTransform>();
		MyRectTransform.localScale = new Vector3(0.01f, 0.01f, 0f);
		Text = transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
		Text.text = "";
		Loading = transform.Find("Loading").GetComponent<RectTransform>();
		Loading.gameObject.SetActive(false);
		Arrow = transform.Find("Arrow").gameObject;
		Arrow.SetActive(false);
    }

	public void Open()
	{
		Controller.Instance.PlayPopupSound();

		Opened = true;
	}

	public void Close()
	{
		Controller.Instance.PlayPopoutSound();

		Opened = false;
	}

	public void SetText(string text, bool showLoading = false, System.Func<bool> callback = null)
	{
		if (!this.Opened)
			Open();

		Arrow.SetActive(false);
		Text.text = " ";
		Callback = callback;
		SpeedUp = false;
		Text.ForceMeshUpdate();
		Loading.gameObject.SetActive(false);
		ShowLoading = showLoading;
		LoadingPhase = 0f;
		CurrentChars.Clear();
		CurrentChar = 0;
		var currentSpeed = StandardSpeed;
		for (int i = 0; i < text.Length; i++)
		{
			var c = text[i];
			if (c == '<')
			{
				var end = text.IndexOf(">", i);
				if (end > -1)
				{
					var op = text.Substring(i, (end + 1) - i);
					i = end;
					if (op.ToLowerInvariant().StartsWith("<s"))
					{
						var speed = float.Parse(op.Substring(2, op.Length - 3), System.Globalization.NumberStyles.Number);
						if (speed == 0)
							currentSpeed = StandardSpeed;
						else
							currentSpeed = speed;
						continue;
					}
					CurrentChars.Add((op, 0f));
					continue;
				}
				else CurrentChars.Add((c + "", currentSpeed));
			}
			else
			{
				CurrentChars.Add((c + "", currentSpeed));
			}
		}
		if (Callback != null)
			FullScreenClickArea.Show(Click);
	}

	void Click()
	{
		if (OpeningPhase >= 1f)
		{ 
			if (CurrentChar >= CurrentChars.Count)
			{
				if (Callback != null)
				{
					if (Callback())
						Close();
				}
				else Close();
			}
			else
			{
				SpeedUp = true;
				FullScreenClickArea.Show(Click);
			}
		}
		else FullScreenClickArea.Show(Click);
	}

	private float AudioCooldown = 0f;

    // Update is called once per frame
    void Update()
    {
		if (PreviousDebugText != DebugText)
		{
			PreviousDebugText = DebugText;
			SetText(DebugText, true);
		}
		if (Opened && OpeningPhase < 1f)
			OpeningPhase = Mathf.Min(1f, OpeningPhase + Time.deltaTime * 2f);
		if (!Opened && OpeningPhase > 0f)
		{
			OpeningPhase = Mathf.Max(0f, OpeningPhase - Time.deltaTime * 3f);
			if (OpeningPhase <= 0f)
			{
				Text.text = " ";
				Text.ForceMeshUpdate();
			}
		}

		if (AudioCooldown > 0f)
			AudioCooldown -= Time.deltaTime;

		if (OpeningPhase >= 1f)
		{
			if (TimeLeft > 0f)
				TimeLeft -= Time.deltaTime * (SpeedUp ? 3f : 1f);
			while (TimeLeft <= 0f && CurrentChar <= CurrentChars.Count)
			{
				if (AudioCooldown <= 0f)
				{
					PlaySound(-AudioCooldown);
					AudioCooldown = 0.05f;
				}

				string text = "";
				for (int i = 0; i < CurrentChar; i++)
					text += CurrentChars[i].Item1;
				Text.text = text;
				Text.ForceMeshUpdate();
				if (CurrentChar < CurrentChars.Count)
				{
					if (CurrentChars[CurrentChar].Item2 > 0f)
					{
						float time = MaxTime / CurrentChars[CurrentChar].Item2;
						TimeLeft += time;
					}
				}
				CurrentChar++;
			}

			if (Callback != null && CurrentChar >= CurrentChars.Count && !Arrow.activeSelf)
				Arrow.SetActive(true);
			if (ShowLoading && CurrentChar >= CurrentChars.Count && LoadingPhase < 1f)
			{
				Loading.gameObject.SetActive(true);
				LoadingPhase = Mathf.Min(1f, LoadingPhase + Time.deltaTime * 2);

				var loadingScale = EasingFunction.EaseOutBack(0f, 0.7f, LoadingPhase);
				Loading.localScale = new Vector3(loadingScale, loadingScale, 1f);
				if (LoadingPhase == 1f)
					Opened = false;
			}
		}
		MyRenderer.SetAlpha(Mathf.Min(1f, OpeningPhase * 1.2f));
		float scale = OpeningPhase == 0f ? 0.001f : EasingFunction.EaseOutBack(0.5f, 1f, OpeningPhase);
		MyRectTransform.localScale = new Vector3(scale, scale, 1f);
	}
}
