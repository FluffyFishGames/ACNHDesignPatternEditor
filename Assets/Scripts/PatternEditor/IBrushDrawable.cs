using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public interface IBrushDrawable
{
	int Width { get; }
	int Height { get; }
	UnityEngine.Color[] Colors { get; }
	bool IsDrawable(int x, int y);
}