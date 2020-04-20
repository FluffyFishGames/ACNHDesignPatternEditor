using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class LineTool : ITool
{
	private PatternEditor Editor;
	private int StartX;
	private int StartY;
	private RasterLayer TemporaryLayer;

	public void Destroyed(){}

	private IntPtr OldColors;

	public void MouseDown(int x, int y)
	{
		var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
		var size = layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize;
		OldColors = Marshal.AllocHGlobal(size);
		unsafe
		{
			System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), OldColors.ToPointer(), size, size);
		}

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
		TemporaryLayer.Bitmap.Clear();
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

			var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
			var size = layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize;
			var newColors = Marshal.AllocHGlobal(size);
			unsafe
			{
				System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), newColors.ToPointer(), size, size);
			}
			Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Line", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, newColors));
		}
	}

	public void SetEditor(PatternEditor editor)
	{
		Editor = editor;
	}
}
