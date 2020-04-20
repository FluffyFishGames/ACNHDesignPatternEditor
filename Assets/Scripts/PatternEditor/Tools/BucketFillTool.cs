using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

public class BucketFillTool : ITool
{
	private PatternEditor Editor;
	public void Destroyed() { }

	public void MouseDown(int x, int y)
	{
		var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
		var size = layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize;
		var oldColors = Marshal.AllocHGlobal(size);
		unsafe
		{
			System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), oldColors.ToPointer(), size, size);
		}
		
		if (layer is RasterLayer rasterLayer)
		{
			unsafe
			{
				var colorPointer = rasterLayer.Texture.GetColors();
				int w = rasterLayer.Texture.Width;
				int h = rasterLayer.Texture.Height;
				var color = *(colorPointer + x + (h - 1 - y) * w);
				List<int> visited = new List<int>();
				List<int> check = new List<int>();
				List<int> found = new List<int>();
				check.Add(x + (h - 1 - y) * w);
				while (check.Count > 0)
				{
					var index = check[0];
					var px = index % w;
					var py = ((index - px) / w);
					check.RemoveAt(0);
					visited.Add(index);
					if (color.Distance(*(colorPointer + index)) <= Editor.Tools.BucketFillTolerance)
					{
						found.Add(index);
						if (px > 0 && !visited.Contains(index - 1) && !check.Contains(index - 1))
							check.Add(index - 1);
						if (px < w - 1 && !visited.Contains(index + 1) && !check.Contains(index + 1))
							check.Add(index + 1);
						if (py > 0 && !visited.Contains(index - w) && !check.Contains(index - w))
							check.Add(index - w);
						if (py < h - 1 && !visited.Contains(index + w) && !check.Contains(index + w))
							check.Add(index + w);
					}
				}
				var col = new TextureBitmap.Color((byte) (Editor.CurrentColor.r * 255f), (byte) (Editor.CurrentColor.g * 255f), (byte) (Editor.CurrentColor.b * 255f), (byte) (Editor.CurrentColor.a * 255f));
				for (var i = 0; i < found.Count; i++)
				{
					*(colorPointer + found[i]) = col;
				}
				rasterLayer.UpdateTexture();
				Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
			}
		}
		var newColors = Marshal.AllocHGlobal(size);
		unsafe
		{
			System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), newColors.ToPointer(), size, size);
		}
		Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Bucket fill", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, oldColors, newColors));
	}

	public void MouseDrag(int x, int y, int previousX, int previousY) {}
	public void MouseMove(int x, int y) {}
	public void MouseUp(int x, int y) {}
	public void SetEditor(PatternEditor editor){ this.Editor = editor; }
}