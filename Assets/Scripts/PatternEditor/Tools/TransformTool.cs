using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class TransformTool : ITool
{
	public PatternEditor Editor;
	public void Destroyed() { Editor.Tools.TransformTool.Hide(); }
	public void MouseDown(int x, int y) {}

	public void MouseDrag(int x, int y, int previousX, int previousY) {}

	public void MouseMove(int x, int y) {}

	public void MouseUp(int x, int y) {}

	public void SetEditor(PatternEditor editor)
	{
		Editor = editor;
		if (Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is ITransformable smartObjectLayer)
			Editor.Tools.TransformTool.Show(smartObjectLayer);
	}
}
