using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class _3DPreview : MonoBehaviour, IPointerDownHandler
{
	public Transform Meshes;
	public PatternEditor Editor;
	private bool IsMouseDown = false;
	private float LastX = 0;
	private float LastY = 0;
	private float Angle = 0f;

	public void OnPointerDown(PointerEventData eventData)
	{
		IsMouseDown = true;
		LastX = eventData.position.x;
		LastY = eventData.position.x;
	}

	// Start is called before the first frame update
	void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
		if (IsMouseDown)
		{
			var mouseX = Input.mousePosition.x;
			var mouseY = Input.mousePosition.y;
			var deltaX = LastX - mouseX;
			var deltaY = LastY - mouseY;
			Editor.MovePreview(deltaX, -deltaY);
			LastX = mouseX;
			LastY = mouseY;
			if (!Input.GetMouseButton(0))
				IsMouseDown = false;
		}
    }
}
