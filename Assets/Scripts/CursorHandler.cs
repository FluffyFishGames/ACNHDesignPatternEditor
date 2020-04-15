using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class CursorHandler : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
	public CursorManager.CursorType Cursor;
	private bool MouseOver = false;
	private int UniqueID;
	private static int CurrID = 0;

	public void OnPointerEnter(PointerEventData eventData)
	{
		MouseOver = true;
		CursorManager.Instance.AddCursor(this.UniqueID, Cursor);
	}

	public void OnPointerExit(PointerEventData eventData)
	{
		MouseOver = false;
		CursorManager.Instance.RemoveCursor(this.UniqueID);
	}

	void Start()
	{
		CurrID++;
		UniqueID = CurrID;
	}

	void OnDisable()
	{
		if (MouseOver)
		{
			MouseOver = false;
			CursorManager.Instance.RemoveCursor(this.UniqueID);
		}
	}
}
