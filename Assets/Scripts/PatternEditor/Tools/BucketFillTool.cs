using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class BucketFillTool : ITool
{
	private PatternEditor Editor;
	public void Destroyed() { }

	private UnityEngine.Color[] OldColors;

	public void MouseDown(int x, int y)
	{
		OldColors = new UnityEngine.Color[Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors.Length];
		System.Array.Copy(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors, OldColors, OldColors.Length);

		var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
		if (layer is RasterLayer rasterLayer)
		{
			var color = rasterLayer.Colors[x + y * rasterLayer.Width];
			List<int> visited = new List<int>();
			List<int> check = new List<int>();
			List<int> found = new List<int>();
			check.Add(x + y * rasterLayer.Width);
			while (check.Count > 0)
			{
				var index = check[0];
				var px = index % rasterLayer.Width;
				var py = (index - px) / rasterLayer.Width;
				check.RemoveAt(0);
				visited.Add(index);
				if (color.Distance(rasterLayer.Colors[index]) <= Editor.Tools.BucketFillTolerance)
				{
					found.Add(index);
					if (px > 0 && !visited.Contains(index - 1) && !check.Contains(index - 1))
						check.Add(index - 1);
					if (px < rasterLayer.Width - 1 && !visited.Contains(index + 1) && !check.Contains(index + 1))
						check.Add(index + 1);
					if (py > 0 && !visited.Contains(index - rasterLayer.Width) && !check.Contains(index - rasterLayer.Width))
						check.Add(index - rasterLayer.Width);
					if (py < rasterLayer.Height - 1 && !visited.Contains(index + rasterLayer.Width) && !check.Contains(index + rasterLayer.Width))
						check.Add(index + rasterLayer.Width);
				}
			}
			for (var i = 0; i < found.Count; i++)
			{
				rasterLayer.Colors[found[i]] = Editor.CurrentColor;
			}
			rasterLayer.UpdateTexture();
			Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
		}

		Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Bucket fill", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors));
	}

	public void MouseDrag(int x, int y, int previousX, int previousY) {}
	public void MouseMove(int x, int y) {}
	public void MouseUp(int x, int y) {}
	public void SetEditor(PatternEditor editor){ this.Editor = editor; }
}