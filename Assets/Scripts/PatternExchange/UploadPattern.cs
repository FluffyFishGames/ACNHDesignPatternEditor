using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System.Windows.Forms;
using UnityEngine.UI;

public class UploadPattern : MonoBehaviour
{
	public TMP_InputField CreatorInputField;
	public TMPro.TMP_InputField NameInputField;
	private Animator Animator;
	private System.Action<DesignServer.Pattern> Confirm;
	private System.Action Cancel;
	private List<string> Tags;
	public MenuButton ConfirmButton;
	public MenuButton CancelButton;
	public TMPro.TextMeshProUGUI Label;
	
	public TMP_InputField TagInputField;
	public UnityEngine.UI.Button AddTagButton;
	public Transform TagContainer;
	public GameObject NoTagsText;
	public GameObject TagPrefab;
	private DesignPattern Pattern;
	private DesignServer.Pattern UploadedPattern;
	public DesignServer.Client Client;

	void OnEnable()
	{
		Animator = GetComponent<Animator>();
	}

	void Start()
	{
		AddTagButton.onClick.AddListener(() =>
		{
			AddCurrentTag();
		});

		ConfirmButton.OnClick = () => {
			string patternName = this.NameInputField.text;
			string creatorName = this.CreatorInputField.text;
			PlayerPrefs.SetString("CreatorName", this.CreatorInputField.text);
			//string name, List<string> tags
			Controller.Instance.Popup.SetText("Uploading pattern to\r\npattern exchange", true);
			System.Threading.Tasks.Task.Run(() => {
				try
				{
					var msgPattern = new DesignServer.Pattern();
					msgPattern.Type = (byte) Pattern.Type;
					msgPattern.Name = patternName;
					msgPattern.Creator = creatorName;
					msgPattern.Tags = Tags.ToArray();
					msgPattern.Code = "";
					msgPattern.ID = 0;
					var file = ACNHFileFormat.FromPattern(Pattern);
					msgPattern.Bytes = file.ToBytes();
					
					var msg = new DesignServer.Messages.UploadPattern(msgPattern);
					Client.Connection.SendMessage(msg, (response) => {
						if (response is DesignServer.Messages.UploadPatternResponse resp)
						{
							UploadedPattern = resp.Pattern;
						}
					});
				}
				catch (System.Exception e)
				{
					Controller.Instance.Popup.SetText(e.Message, false, () => { return true; });
				}
			});
		};

		CancelButton.OnClick = () => {
			Cancel?.Invoke();
		};
	}

	public void RemoveTag(string tag)
	{
		Tags.Remove(tag);
		UpdateTags();
	}

	void UpdateTags()
	{
		int c = (this.TagContainer.childCount - 1) > this.Tags.Count ? (this.TagContainer.childCount - 1) : this.Tags.Count;
		var components = new List<UploadPatternTag>();
		for (int i = this.TagContainer.childCount - 1; i >= this.Tags.Count + 1; i--)
			Destroy(this.TagContainer.GetChild(i).gameObject);
		for (int i = 1; i <= c; i++)
		{
			if (i >= this.TagContainer.childCount)
			{
				components.Add(GameObject.Instantiate(TagPrefab, this.TagContainer).GetComponent<UploadPatternTag>());
			}
			else
			{
				components.Add(this.TagContainer.GetChild(i).GetComponent<UploadPatternTag>());
			}
		}
		this.NoTagsText.SetActive(this.Tags.Count == 0);
		for (int i = 0; i < components.Count; i++)
		{
			components[i].SetTag(this, this.Tags[i]);
			LayoutRebuilder.ForceRebuildLayoutImmediate(components[i].GetComponent<RectTransform>());
		}
		LayoutRebuilder.ForceRebuildLayoutImmediate(TagContainer.GetComponent<RectTransform>());
	}

	public string GetName()
	{
		return CreatorInputField.text;
	}

	public void SetName(string name)
	{
		CreatorInputField.text = name;
	}

	public void Show(DesignPattern pattern, System.Action<DesignServer.Pattern> confirm, System.Action cancel, string label = null)
	{
		Pattern = pattern;
		CreatorInputField.text = PlayerPrefs.GetString("CreatorName");
		Tags = new List<string>();
		Confirm = confirm;
		Cancel = cancel;
		Animator.SetTrigger("TransitionIn");
		if (Controller.Instance.CurrentOperation is IPatternOperation patternOperation)
		{
			CreatorInputField.text = patternOperation.GetPattern().Name;
		}
		UpdateTags();
		NameInputField.text = pattern.Name;
		CreatorInputField.Select();
		CreatorInputField.ActivateInputField();
	}

	public void Hide()
	{
		Animator.SetTrigger("TransitionOut");
	}

	private void AddCurrentTag()
	{
		if (Tags.Count < 5)
		{
			var tag = TagInputField.text.ToLowerInvariant().Trim().Replace(' ', '-');
			if (tag == "")
				return;
			if (!Tags.Contains(tag))
			{
				Tags.Add(tag);
				this.UpdateTags();
			}
		}
		TagInputField.text = "";
	}

	private bool WasFocused = false;
	private void Update()
	{
		if (UploadedPattern != null)
		{
			Confirm?.Invoke(UploadedPattern);
			Controller.Instance.Popup.Close();
			UploadedPattern = null;
		}
		if (WasFocused && Input.GetKeyDown(KeyCode.Return))
		{
			AddCurrentTag();
			TagInputField.OnPointerClick(new UnityEngine.EventSystems.PointerEventData(null) { button = UnityEngine.EventSystems.PointerEventData.InputButton.Left });
		}
		WasFocused = TagInputField.isFocused;
	}
}
