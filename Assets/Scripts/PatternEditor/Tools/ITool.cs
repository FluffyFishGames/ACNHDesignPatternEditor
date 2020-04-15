using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface ITool
{
	void SetEditor(PatternEditor editor);
	void MouseMove(int x, int y);
	void MouseDrag(int x, int y, int previousX, int previousY);
	void MouseDown(int x, int y);
	void MouseUp(int x, int y);
	void Destroyed();
}
