using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.EventSystems;

[ExecuteInEditMode]
public class ActionMenuButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
{
	public RectTransform HoverTransform;
	public RectTransform TextTransform;
	public TextMeshProUGUI TextComponent;
	public string Text = "";
	private string LastText = "";
	private float LastPhase = 0f;
	private float Phase = 0f;
	private bool IsMouseOver;
	public AnimationCurve PositionCurve;
	public AnimationCurve ScaleCurve;
	public System.Action OnClick;
	public ActionMenu Menu;
	public int Index;

	// Start is called before the first frame update
	void Start()
    {
		HoverTransform.gameObject.SetActive(false);
	}


	void RecalculateText()
	{
		TextComponent.text = Text;
		Vector3[] vertices;
		TextComponent.ForceMeshUpdate();

		TMP_TextInfo textInfo = TextComponent.textInfo;
		int characterCount = textInfo.characterCount;

		if (characterCount == 0)
			return;

		float width = 3f;
		float phasePerCharacter = 1f / ((float) characterCount);
		float phase = Phase * (1f + width * phasePerCharacter) - (phasePerCharacter * width / 2);
		for (int i = 0; i < characterCount; i++)
		{
			float charPhase = Mathf.Clamp01(((phase - (((float)i) - width / 2) * phasePerCharacter)) / (phasePerCharacter * width));
			if (!textInfo.characterInfo[i].isVisible)
				continue;

			var scale = ScaleCurve.Evaluate(charPhase);
			var pos = PositionCurve.Evaluate(charPhase) * 10f;

			int vertexIndex = textInfo.characterInfo[i].vertexIndex;
			int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
			vertices = textInfo.meshInfo[materialIndex].vertices;
			
			vertices[vertexIndex + 0] = new Vector3(vertices[vertexIndex + 0].x, (vertices[vertexIndex + 0].y + pos) * scale, vertices[vertexIndex + 0].z);
			vertices[vertexIndex + 1] = new Vector3(vertices[vertexIndex + 1].x, (vertices[vertexIndex + 1].y + pos) * scale, vertices[vertexIndex + 1].z);
			vertices[vertexIndex + 2] = new Vector3(vertices[vertexIndex + 2].x, (vertices[vertexIndex + 2].y + pos) * scale, vertices[vertexIndex + 2].z);
			vertices[vertexIndex + 3] = new Vector3(vertices[vertexIndex + 3].x, (vertices[vertexIndex + 3].y + pos) * scale, vertices[vertexIndex + 3].z);
		}

		TextComponent.UpdateVertexData();
	}

	void Update()
	{
		if (IsMouseOver && Phase < 1f)
			Phase = Mathf.Min(1f, Phase + Time.deltaTime * 0.5f);

		if (LastPhase != Phase || LastText != Text || TextComponent.havePropertiesChanged)
		{
			LastPhase = Phase;
			LastText = Text;
			RecalculateText();
			return;
		}
	}

	public void OnPointerEnter(PointerEventData eventData)
	{
		Controller.Instance.PlayHoverSound();

		IsMouseOver = true;
		Phase = 0f;
		HoverTransform.gameObject.SetActive(true);
		HoverTransform.sizeDelta = new Vector2(TextTransform.sizeDelta.x + 15f, HoverTransform.sizeDelta.y);
		Menu.MouseOver(this);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		IsMouseOver = false;
		Phase = 0f;
		HoverTransform.gameObject.SetActive(false);
		Menu.MouseOut(this);
	}

	public void OnPointerClick(PointerEventData eventData)
	{
		Controller.Instance.PlayClickSound();

		OnClick?.Invoke();
	}
}
