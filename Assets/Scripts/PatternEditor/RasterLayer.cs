using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RasterLayer : Layer, IBrushDrawable
{
	public RasterLayer(SubPattern pattern, string name) : base(pattern, name)
	{

	}

	public bool IsDrawable(int px, int py)
	{
		return this.SubPattern.IsDrawable(px, py);
	}

	public void DrawBrush(int brushX, int brushY, UnityEngine.Color color)
	{
		this.SubPattern.Pattern.Editor.CurrentBrush.Draw(this, brushX, brushY, color);
	}

	public void EraseBrush(int brushX, int brushY)
	{
		this.SubPattern.Pattern.Editor.CurrentBrush.Erase(this, brushX, brushY);
	}

	public void DrawLine(int fromX, int fromY, int toX, int toY, UnityEngine.Color color)
	{
		this.SubPattern.Pattern.Editor.CurrentBrush.DrawLine(this, fromX, fromY, toX, toY, color);
	}
}
