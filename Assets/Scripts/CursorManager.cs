using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class CursorManager : MonoBehaviour
{
	public static CursorManager Instance;
	public Texture2D HandCursor;
	public Vector2 HandPivot;
	public Texture2D ColorPickerCursor;
	public Vector2 ColorPickerPivot;
	public Texture2D MoveCursor;
	public Vector2 MovePivot;
	public Texture2D ResizeDiagonal1Cursor;
	public Vector2 ResizeDiagonal1Pivot;
	public Texture2D ResizeDiagonal2Cursor;
	public Vector2 ResizeDiagonal2Pivot;
	public Texture2D ResizeHCursor;
	public Vector2 ResizeHPivot;
	public Texture2D ResizeVCursor;
	public Vector2 ResizeVPivot;

	public enum CursorType {
		None,
		Hand,
		ColorPicker,
		Move,
		ResizeDiagonal1,
		ResizeDiagonal2,
		ResizeH,
		ResizeV
	}

	void Start()
	{
		Instance = this;
	}

	private void OnEnable()
	{
		Instance = this;
	}

	private CursorType OverrideType;
	private List<(int, CursorType)> ActiveCursors = new List<(int, CursorType)>();
	public void AddCursor(int uniqueID, CursorType type)
	{
		if (uniqueID < 0)
			OverrideType = type;
		else 
			ActiveCursors.Add((uniqueID, type));
	}

	public void RemoveCursor(int uniqueID)
	{
		if (uniqueID < 0)
			OverrideType = CursorType.None;
		else
		{
			for (int i = 0; i < ActiveCursors.Count; i++)
			{
				if (ActiveCursors[i].Item1 == uniqueID)
				{
					ActiveCursors.RemoveAt(i);
					return;
				}
			}
		}
	}

	private CursorType LastType = CursorType.None;
	private void Update()
	{
		CursorType type = CursorType.None;
		if (ActiveCursors.Count > 0)
			type = ActiveCursors[ActiveCursors.Count - 1].Item2;
		if (OverrideType != CursorType.None)
			type = OverrideType;
		if (LastType != type)
		{
			SetCursor(type);
			LastType = type;
		}
	}

	public void SetCursor(CursorType type)
	{
		if (type == CursorType.ColorPicker)
			Cursor.SetCursor(ColorPickerCursor, ColorPickerPivot, CursorMode.Auto);
		else if (type == CursorType.Hand)
			Cursor.SetCursor(HandCursor, HandPivot, CursorMode.Auto);
		else if (type == CursorType.ResizeDiagonal1)
			Cursor.SetCursor(ResizeDiagonal1Cursor, ResizeDiagonal1Pivot, CursorMode.Auto);
		else if (type == CursorType.ResizeDiagonal2)
			Cursor.SetCursor(ResizeDiagonal2Cursor, ResizeDiagonal2Pivot, CursorMode.Auto);
		else if (type == CursorType.ResizeH)
			Cursor.SetCursor(ResizeHCursor, ResizeHPivot, CursorMode.Auto);
		else if (type == CursorType.ResizeV)
			Cursor.SetCursor(ResizeVCursor, ResizeVPivot, CursorMode.Auto);
		else if (type == CursorType.Move)
			Cursor.SetCursor(MoveCursor, MovePivot, CursorMode.Auto);
		else
			Cursor.SetCursor(null, Vector2.zero, CursorMode.Auto);
	}
}
