using SFB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class Layers : MonoBehaviour
{
	public PatternEditor Editor;
	public Button AddLayer;
	public Button DeleteLayer;
	public Button ImportLayer;
	public Transform LayersTransform;
	public GameObject LayerPrefab;

	private bool Dragging = false;
	private Layer DraggingLayer = null;
	private bool Initialized = false;
	private int DraggingStartIndex = 0;
	private int DraggingEndIndex = 0;

	private void Start()
	{
		Initialize();
	}

	private void OnEnable()
	{
		Initialize();
	}

	private void Initialize()
	{
		if (Initialized) return;
		Initialized = true;

		AddLayer.onClick.AddListener(() => {
			Editor.CurrentPattern.CurrentSubPattern.CreateLayer();
		});

		DeleteLayer.onClick.AddListener(() => {
			Editor.CurrentPattern.CurrentSubPattern.DeleteLayer(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer);
		});

		ImportLayer.onClick.AddListener(() => {
			ImportImage();
			//Editor.CurrentPattern.CurrentSubPattern.DeleteLayer(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer);
			//DeleteSelectedLayer();
		});
	}


	public void DragLayer(Layer layer)
	{
		DraggingLayer = layer;
		DraggingStartIndex = Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(layer);
	}

	public void UpdateLayers()
	{
		while (LayersTransform.childCount > Editor.CurrentPattern.CurrentSubPattern.Layers.Count)
		{
			Destroy(LayersTransform.GetChild(0).gameObject);
		}
		while (LayersTransform.childCount < Editor.CurrentPattern.CurrentSubPattern.Layers.Count)
		{
			var layer = GameObject.Instantiate(LayerPrefab, LayersTransform);
		}

		for (int i = 0; i < Editor.CurrentPattern.CurrentSubPattern.Layers.Count; i++)
		{
			Editor.CurrentPattern.CurrentSubPattern.Layers[i].IsSelected = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer == Editor.CurrentPattern.CurrentSubPattern.Layers[i];
			var layerComponent = LayersTransform.GetChild(LayersTransform.childCount - 1 - i).GetComponent<LayerComponent>();
			layerComponent.Editor = this.Editor;
			layerComponent.Layers = this;
			layerComponent.SetLayer(Editor.CurrentPattern.CurrentSubPattern.Layers[i]);
		}
	}

	private void Update()
	{
		if (DraggingLayer != null)
		{
			if (!Input.GetMouseButton(0))
			{
				if (DraggingStartIndex != DraggingEndIndex)
				{
					Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.LayerMoveAction("Moved " + DraggingLayer.Name, DraggingStartIndex, DraggingEndIndex));
				}
				DraggingLayer = null;
			}
			else
			{
				Vector2 pos;
				if (RectTransformUtility.ScreenPointToLocalPointInRectangle(LayersTransform.GetComponent<RectTransform>(), Input.mousePosition, Camera.main, out pos))
				{
					int index = (int) (pos.y / 65f);
					if (index >= Editor.CurrentPattern.CurrentSubPattern.Layers.Count - 1)
						index = Editor.CurrentPattern.CurrentSubPattern.Layers.Count - 1;
					if (index < 0)
						index = 0;
					int oldIndex = Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(DraggingLayer);

					if (oldIndex != index)
					{
						var selectedLayer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;

						Editor.CurrentPattern.CurrentSubPattern.Layers.RemoveAt(oldIndex);
						Editor.CurrentPattern.CurrentSubPattern.Layers.Insert(index, DraggingLayer);

						Editor.CurrentPattern.CurrentSubPattern.SelectedLayer = selectedLayer;
						UpdateLayers();
						Editor.CurrentPattern.CurrentSubPattern.UpdateImage();

						DraggingEndIndex = index;
					}
				}
			}
		}
	}

	private void ImportImage()
	{
		Controller.Instance.Popup.SetText("Please select any <#FF6666>Image<#FFFFFF> file to import.", false, () => {
			StandaloneFileBrowser.OpenFilePanelAsync("Import image", "", new ExtensionFilter[] { new ExtensionFilter("Image", new string[] { "png", "jpg", "jpeg", "bmp", "gif" }) }, false, (path) =>
			{
				if (path.Length > 0)
				{
					try
					{
						int x = 0;
						int y = 0;
						int width = Editor.CurrentPattern.CurrentSubPattern.Width;
						int height = Editor.CurrentPattern.CurrentSubPattern.Height;
						var bmp = TextureBitmap.Load(path[0]);
						var newLayer = new SmartObjectLayer(Editor.CurrentPattern.CurrentSubPattern, System.IO.Path.GetFileNameWithoutExtension(path[0]), bmp, x, y, width, height);
						newLayer.UpdateColors();
						var index = Editor.CurrentPattern.CurrentSubPattern.Layers.IndexOf(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer) + 1;
						Editor.CurrentPattern.CurrentSubPattern.Layers.Insert(index, newLayer);
						Editor.CurrentPattern.CurrentSubPattern.SelectedLayer = newLayer;
						Editor.LayersChanged();
						Editor.CurrentPattern.CurrentSubPattern.SelectLayer(newLayer);
						Editor.CurrentPattern.CurrentSubPattern.UpdateImage();

						Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.LayerCreatedAction("Created: " + newLayer.Name, index, newLayer));

						Controller.Instance.Popup.Close();
					}
					catch (System.IO.FileLoadException e)
					{
						Debug.LogException(e);
						Controller.Instance.Popup.SetText("Failed to load the file. File error.", false, () =>
						{
							return true;
						});
						return;
					}
					catch (System.Exception e)
					{
						Debug.LogException(e);
						Controller.Instance.Popup.SetText("Invalid image file.", false, () =>
						{
							return true;
						});
						return;
					}
				}
				else
				{
					Controller.Instance.Popup.Close();
				}
			});
			return false;
		});
	}
}