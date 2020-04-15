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
	private float Angle = 0f;

	public void SetTexture(Texture2D texture)
	{
		for (int i = 0; i< Materials.Length; i++)
			Materials[i].SetTexture("_MainTex", texture);
		Camera.Render();
	}

	public void ResetPosition()
	{
		Angle = 0f;
		ObjectContainer.rotation = Quaternion.AngleAxis(Angle, new Vector3(0f, 1f, 0f));
	}

	public void Move(float delta)
	{
		Angle += delta;
		ObjectContainer.rotation = Quaternion.AngleAxis(Angle, new Vector3(0f, 1f, 0f));
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
