using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.EventSystems;

public class TransformToolContainer : MonoBehaviour
{
	public PatternEditor Editor;
	public EventTrigger TransformCenter;
	public EventTrigger TransformTopLeft;
	public EventTrigger TransformTopRight;
	public EventTrigger TransformBottomLeft;
	public EventTrigger TransformBottomRight;
	public EventTrigger TransformTop;
	public EventTrigger TransformBottom;
	public EventTrigger TransformLeft;
	public EventTrigger TransformRight;
	public GameObject TransformContainer;
	private bool Dragging = false;
	private int DragStartX;
	private int DragStartY;
	private ITransformable Target;

	private DragMode CurrentDragMode;
	private enum DragMode
	{
		TOP_LEFT,
		TOP_RIGHT,
		BOTTOM_LEFT,
		BOTTOM_RIGHT,
		TOP,
		BOTTOM,
		LEFT,
		RIGHT,
		MOVE
	}

	private bool Initialized = false;

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}

	public void Show(ITransformable target)
	{
		gameObject.SetActive(true);
		Target = target;
	}

	public void Hide()
	{
		gameObject.SetActive(false);
	}

	private int StartObjectX;
	private int StartObjectY;
	private int StartObjectWidth;
	private int StartObjectHeight;

	private void Initialize()
	{
		if (Initialized) return;
		Initialized = true;

		var pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				Vector2 vec;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Editor.PixelGrid.RectTransform, Input.mousePosition, Camera.main, out vec))
				{
					StartObjectX = Target.ObjectX;
					StartObjectY = Target.ObjectY;
					StartObjectWidth = Target.ObjectWidth;
					StartObjectHeight = Target.ObjectHeight;

					Vector2 pos = (vec + new Vector2(Editor.PixelGrid.RectTransform.sizeDelta.x / 2f, -Editor.PixelGrid.RectTransform.sizeDelta.y / 2f)) / Editor.PixelSize;
					pos.y = -pos.y;
					Dragging = true;
					DragStartX = (int) (pos.x - Target.ObjectX);
					DragStartY = (int) (pos.y - Target.ObjectY);
					CurrentDragMode = DragMode.MOVE;
					CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.Move);
				}
			}
		});
		TransformCenter.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX;
				DragStartY = Target.ObjectY;
				CurrentDragMode = DragMode.TOP_LEFT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeDiagonal1);

			}
		});
		TransformTopLeft.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX + Target.ObjectWidth;
				DragStartY = Target.ObjectY;
				CurrentDragMode = DragMode.TOP_RIGHT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeDiagonal2);
			}
		});
		TransformTopRight.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX;
				DragStartY = Target.ObjectY + Target.ObjectHeight;
				CurrentDragMode = DragMode.BOTTOM_LEFT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeDiagonal2);
			}
		});
		TransformBottomLeft.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX + Target.ObjectWidth;
				DragStartY = Target.ObjectY + Target.ObjectHeight;
				CurrentDragMode = DragMode.BOTTOM_RIGHT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeDiagonal1);
			}
		});
		TransformBottomRight.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX;
				CurrentDragMode = DragMode.LEFT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeH);
			}
		});
		TransformLeft.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartX = Target.ObjectX + Target.ObjectWidth;
				CurrentDragMode = DragMode.RIGHT;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeH);
			}
		});
		TransformRight.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartY = Target.ObjectY;
				CurrentDragMode = DragMode.TOP;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeV);
			}
		});
		TransformTop.triggers.Add(pointerDown);

		pointerDown = new EventTrigger.Entry() { eventID = EventTriggerType.PointerDown };
		pointerDown.callback.AddListener((evtData) => {
			if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is SmartObjectLayer Target)
			{
				StartObjectX = Target.ObjectX;
				StartObjectY = Target.ObjectY;
				StartObjectWidth = Target.ObjectWidth;
				StartObjectHeight = Target.ObjectHeight;

				Dragging = true;
				DragStartY = Target.ObjectY + Target.ObjectHeight;
				CurrentDragMode = DragMode.BOTTOM;
				CursorManager.Instance.AddCursor(-1, CursorManager.CursorType.ResizeV);
			}
		});
		TransformBottom.triggers.Add(pointerDown);
	}

	private void Update()
	{
		if (Dragging)
		{
			Vector2 vec;
			if (RectTransformUtility.ScreenPointToLocalPointInRectangle(Editor.PixelGrid.RectTransform, Input.mousePosition, Camera.main, out vec))
			{
				Vector2 pos = (vec + new Vector2(Editor.PixelGrid.RectTransform.sizeDelta.x / 2f, -Editor.PixelGrid.RectTransform.sizeDelta.y / 2f)) / Editor.PixelSize;
				pos.y = -pos.y;
				int prevX = Target.ObjectX;
				int prevY = Target.ObjectY;
				int prevW = Target.ObjectWidth;
				int prevH = Target.ObjectHeight;

				if (CurrentDragMode == DragMode.MOVE)
				{
					Target.ObjectX = (int) (pos.x - DragStartX);
					Target.ObjectY = (int) (pos.y - DragStartY);
				}
				if (CurrentDragMode == DragMode.LEFT || CurrentDragMode == DragMode.TOP_LEFT || CurrentDragMode == DragMode.BOTTOM_LEFT)
				{
					int right = Target.ObjectX + Target.ObjectWidth;
					if (pos.x >= right)
						pos.x = right - 1;
					Target.ObjectX = (int) pos.x;
					Target.ObjectWidth = right - Target.ObjectX;
				}
				if (CurrentDragMode == DragMode.RIGHT || CurrentDragMode == DragMode.TOP_RIGHT || CurrentDragMode == DragMode.BOTTOM_RIGHT)
				{
					int left = Target.ObjectX;
					if (pos.x - left < 1)
						pos.x = left + 1;
					Target.ObjectWidth = (int) (pos.x - left);
				}
				if (CurrentDragMode == DragMode.TOP || CurrentDragMode == DragMode.TOP_LEFT || CurrentDragMode == DragMode.TOP_RIGHT)
				{
					int bottom = Target.ObjectY + Target.ObjectHeight;
					if (pos.y >= bottom)
						pos.y = bottom - 1;
					Target.ObjectY = (int) pos.y;
					Target.ObjectHeight = bottom - Target.ObjectY;
				}
				if (CurrentDragMode == DragMode.BOTTOM || CurrentDragMode == DragMode.BOTTOM_LEFT || CurrentDragMode == DragMode.BOTTOM_RIGHT)
				{
					int top = Target.ObjectY;
					if (pos.y - top < 1)
						pos.y = top + 1;
					Target.ObjectHeight = (int) (pos.y - top);
				}
				if (prevX != Target.ObjectX ||
					prevY != Target.ObjectY ||
					prevW != Target.ObjectWidth ||
					prevH != Target.ObjectHeight)
				{
					Target.UpdateColors();
					Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
				}
			}
			if (!Input.GetMouseButton(0))
			{
				Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangeTransformAction(CurrentDragMode == DragMode.MOVE ? "Move" : "Resize", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, StartObjectX, StartObjectY, StartObjectWidth, StartObjectHeight, Target.ObjectX, Target.ObjectY, Target.ObjectWidth, Target.ObjectHeight));
				CursorManager.Instance.RemoveCursor(-1);
				Dragging = false;
			}
		}
		TransformContainer.GetComponent<RectTransform>().anchoredPosition = new Vector2(Target.ObjectX * Editor.PixelSize, -Target.ObjectY * Editor.PixelSize);
		TransformContainer.GetComponent<RectTransform>().sizeDelta = new Vector2(Target.ObjectWidth * Editor.PixelSize, Target.ObjectHeight * Editor.PixelSize);
	}
}
