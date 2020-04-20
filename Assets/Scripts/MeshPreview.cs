using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MeshPreview : MonoBehaviour
{
	public Material StandOverviewMaterial;
	public Material StandPreviewMaterial;
	public Material[] Materials;
	public MeshRenderer ClothStand;
	public SkinnedMeshRenderer[] ClothRenderers;
	public Transform ObjectContainer;
	public Camera Camera;
	public bool Rerender = false;
	private float AngleX = 0f;
	private float AngleY = 0f;

	public void SetTexture(Texture2D texture)
	{
		for (int i = 0; i < Materials.Length; i++)
			Materials[i].SetTexture("_MainTex", texture);
		Camera.Render();
	}

	public void ResetPosition()
	{
		AngleX = 0f;
		AngleY = 0f;
		ObjectContainer.rotation = Quaternion.AngleAxis(AngleX, new Vector3(0f, 1f, 0f));
	}

	public void Move(float deltaX, float deltaY)
	{
		AngleX += deltaX;
		AngleY += deltaY;
		if (AngleY < -45f) AngleY = -45f;
		if (AngleY > 0f) AngleY = 0f;
		var rotation = Quaternion.AngleAxis(AngleY, new Vector3(1f, 0f, 0f)) * Quaternion.AngleAxis(AngleX, new Vector3(0f, 1f, 0f));
		ObjectContainer.rotation = rotation;
	}

	public void Render()
	{
		Camera.Render();
	}

	// Start is called before the first frame update
	void Start()
	{
		//		Camera.enabled = false;
		//Camera.Render();
	}

	// Update is called once per frame
	void Update()
	{
		if (Rerender)
			Camera.Render();
	}
}
