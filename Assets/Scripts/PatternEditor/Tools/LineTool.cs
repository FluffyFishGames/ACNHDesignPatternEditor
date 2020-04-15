using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class LineTool : ITool
{
	private PatternEditor Editor;
	private int StartX;
	private int StartY;
	private RasterLayer TemporaryLayer;

	public void Destroyed(){}

	private UnityEngine.Color[] OldColors;

	public void MouseDown(int x, int y)
	{
		OldColors = new UnityEngine.Color[Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors.Length];
		System.Array.Copy(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors, OldColors, OldColors.Length);

		if (this.Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is RasterLayer rasterLayer)
		{
			this.Editor.CurrentPattern.CurrentSubPattern.CreateTemporaryLayer();
			TemporaryLayer = (RasterLayer) this.Editor.CurrentPattern.CurrentSubPattern.TemporaryLayer;
			StartX = x;
			StartY = y;
		}
	}

	public void MouseDrag(int x, int y, int previousX, int previousY)
	{
		for (int i = 0; i < TemporaryLayer.Colors.Length; i++)
			TemporaryLayer.Colors[i] = new UnityEngine.Color(1f, 1f, 1f, 0f);
		TemporaryLayer.DrawLine(StartX, StartY, x, y, Editor.CurrentColor);
		Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
	}

	public void MouseMove(int x, int y) {}

	public void MouseUp(int x, int y)
	{
		if (TemporaryLayer != null)
		{
			Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Merge(TemporaryLayer);
			Editor.CurrentPattern.CurrentSubPattern.RemoveTemporaryLayer();
			Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.UpdateTexture();
			Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
			TemporaryLayer = null;

			Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Line", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors));
		}
	}

	public void SetEditor(PatternEditor editor)
	{
		Editor = editor;
	}
}
