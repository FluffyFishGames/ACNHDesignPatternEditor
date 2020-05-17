using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TooltipContent : Graphic
{
	[SerializeField]
	Texture m_Texture;

	private string _LastText;
	[SerializeField]
	private string _Text;

	public string Text
	{
		get
		{
			return _Text;
		}
		set
		{
			if (_Text != value)
			{
				_Text = value;
				this.RecalculateText();
			}
		}
	}

	// make it such that unity will trigger our ui element to redraw whenever we change the texture in the inspector
	public Texture texture
	{
		get
		{
			return m_Texture;
		}
		set
		{
			if (m_Texture == value)
				return;

			m_Texture = value;
			SetVerticesDirty();
			SetMaterialDirty();
		}
	}
	public override Texture mainTexture
	{
		get
		{
			return m_Texture == null ? s_WhiteTexture : m_Texture;
		}
	}

	private CanvasRenderer Renderer;
	private RectTransform RectTransform;
	private Mesh Mesh;
	private TMPro.TextMeshProUGUI TextComponent;

	public float Padding = 35f;
	public float Resolution = 10f;
	public float SideWidth = 20f;
	[Range(0f, 1f)]
	public float SideUV = 0.49f;
	public float Angle = 20f;

	protected override void OnPopulateMesh(VertexHelper vh)
	{
		RectTransform = GetComponent<RectTransform>();
		Renderer = GetComponent<CanvasRenderer>();
		var rect = RectTransform.rect;
		int width = (int) (rect.width / Resolution);
		vh.Clear();
		var per = 1f / ((float) width - 1);
		var x = 0f;
		float centerUVWidth = (1f - SideUV * 2);
		float sideWidth = (SideWidth) / rect.width;
		if (SideWidth * 2 > rect.width)
			sideWidth = 0.5f;
		float centerWidth = (1f - sideWidth);
		float angle = Angle * 100 / rect.width;
		for (int i = 0; i < width; i++)
		{
			var quat = Quaternion.AngleAxis((x - 0.5f) * angle, new Vector3(0f, 0f, -1f));
			float uvX = x;
			if (uvX < sideWidth)
				uvX = uvX / sideWidth * SideUV;
			else if (uvX < 1 - sideWidth)
				uvX = SideUV + ((uvX - sideWidth) / centerWidth) * centerUVWidth;
			else
				uvX = (1f - SideUV) + ((uvX - (1f - sideWidth))) / sideWidth * SideUV;
			vh.AddVert(quat * new Vector3(rect.xMin + x * rect.width, rect.yMin + 0f, 0f), this.color, new Vector2(uvX, 0f));
			vh.AddVert(quat * new Vector3(rect.xMin + x * rect.width, rect.yMin + rect.height, 0f), this.color, new Vector2(uvX, 1f));
			x += per;
			if (i < width - 1)
			{
				vh.AddTriangle(i * 2 + 1, i * 2 + 2, i * 2 + 0);
				vh.AddTriangle(i * 2 + 2, i * 2 + 1, i * 2 + 3);
			}
		}
	}

	protected override void OnRectTransformDimensionsChange()
	{
		base.OnRectTransformDimensionsChange();
		SetVerticesDirty();
		SetMaterialDirty();
	}

	void RecalculateText()
	{
		if (TextComponent == null) TextComponent = this.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
		if (RectTransform == null) RectTransform = GetComponent<RectTransform>();
		TextComponent.text = Text;
		Vector3[] vertices;
		TextComponent.ForceMeshUpdate();
		float textWidth = TextComponent.preferredWidth + Padding * 2;
		this.RectTransform.sizeDelta = new Vector3(textWidth, this.RectTransform.sizeDelta.y, 0f);
		TextComponent.ForceMeshUpdate();

		TMP_TextInfo textInfo = TextComponent.textInfo;
		int characterCount = textInfo.characterCount;

		if (characterCount == 0)
			return;

		float boundsMinX = TextComponent.bounds.min.x - Padding;
		float boundsMaxX = TextComponent.bounds.max.x + Padding;

		float fullWidth = textWidth + Padding * 2;
		float angle = Angle * 100 / textWidth;

		for (int i = 0; i < characterCount; i++)
		{
			if (!textInfo.characterInfo[i].isVisible)
				continue;

			int vertexIndex = textInfo.characterInfo[i].vertexIndex;
			int materialIndex = textInfo.characterInfo[i].materialReferenceIndex;
			vertices = textInfo.meshInfo[materialIndex].vertices;

			Vector3 charMidBaselinePos = new Vector2((vertices[vertexIndex + 0].x + vertices[vertexIndex + 2].x) / 2, textInfo.characterInfo[i].baseLine);

			float zeroToOnePos = (charMidBaselinePos.x - boundsMinX) / (boundsMaxX - boundsMinX);

			var quat = Quaternion.AngleAxis((zeroToOnePos - 0.5f) * angle, new Vector3(0f, 0f, -1f));
			vertices[vertexIndex + 0] = quat * vertices[vertexIndex + 0];
			vertices[vertexIndex + 1] = quat * vertices[vertexIndex + 1];
			vertices[vertexIndex + 2] = quat * vertices[vertexIndex + 2];
			vertices[vertexIndex + 3] = quat * vertices[vertexIndex + 3];
		}

		TextComponent.UpdateVertexData();
		SetVerticesDirty();
		SetMaterialDirty();
	}

	void Update()
	{
		if (TextComponent == null) TextComponent = this.transform.Find("Text").GetComponent<TMPro.TextMeshProUGUI>();
		if (_LastText == _Text && !TextComponent.havePropertiesChanged)// && !ParametersHaveChanged())
		{
			return;
		}
		_LastText = _Text;

		RecalculateText();
	}
}
