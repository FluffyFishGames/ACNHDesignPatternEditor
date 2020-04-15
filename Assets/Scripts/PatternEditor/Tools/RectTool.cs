using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class RectTool : ITool
{
	private PatternEditor Editor;

	private int StartX = 0;
	private int StartY = 0;
	private Layer TemporaryLayer;
	public void Destroyed() { }

	private UnityEngine.Color[] OldColors;

	public void MouseDown(int x, int y)
	{
		OldColors = new UnityEngine.Color[Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors.Length];
		System.Array.Copy(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors, OldColors, OldColors.Length);

		if (this.Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is RasterLayer rasterLayer)
		{
			this.Editor.CurrentPattern.CurrentSubPattern.CreateTemporaryLayer();
			TemporaryLayer = this.Editor.CurrentPattern.CurrentSubPattern.TemporaryLayer;
			StartX = x;
			StartY = y;
		}
	}
	
	public void MouseDrag(int x, int y, int previousX, int previousY)
	{
		if (TemporaryLayer is RasterLayer rasterLayer)
		{
			var fromX = Mathf.Min(StartX, x);
			var toX = Mathf.Max(StartX, x);
			var fromY = Mathf.Min(StartY, y);
			var toY = Mathf.Max(StartY, y);

			for (int i = 0; i < TemporaryLayer.Colors.Length; i++)
				TemporaryLayer.Colors[i] = new UnityEngine.Color(1f, 1f, 1f, 0f);

			if (Editor.Tools.CurrentTool == Tools.Tool.LineCircle || Editor.Tools.CurrentTool == Tools.Tool.FullCircle)
			{
				float cX = fromX + (toX - fromX) / 2f;
				float cY = fromY + (toY - fromY) / 2f;
				float rX = (toX - fromX) / 2f;
				float rY = (toY - fromY) / 2f;
				if (Editor.Tools.CurrentTool == Tools.Tool.FullCircle)
				{
					float distanceX = rX * 2;
					float distanceY = rY * 2;
					for (int circY = 0; circY < Editor.CurrentBrush.Size + distanceY; circY++)
					{
						for (int circX = 0; circX < Editor.CurrentBrush.Size + distanceX; circX++)
						{
							int coordX = (fromX + circX);
							int coordY = (fromY + circY);
							if (Editor.CurrentPattern.CurrentSubPattern.IsDrawable(coordX, coordY))
							{
								if (Mathf.Pow((circX - (rX)) / (rX), 2f) + Mathf.Pow((circY - (rY)) / (rY), 2f) <= 1f)
								{
									rasterLayer.Colors[(fromX + circX) + (fromY + circY) * rasterLayer.Width] = this.Editor.CurrentColor;
								}
							}
						}
					}
				}
				else
				{
					int lX = -999;
					int lY = -999;
					for (float r = 0; r < Mathf.PI / 2f; r += 0.05f)
					{
						int ccx = (int) Mathf.Round(Mathf.Cos(r) * rX);
						int ccy = (int) Mathf.Round(Mathf.Sin(r) * rY);
						if (ccx != lX || ccy != lY)
						{
							rasterLayer.DrawBrush((int) (cX - ccx), (int) (cY - ccy), Editor.CurrentColor);
							rasterLayer.DrawBrush((int) (cX + ccx), (int) (cY - ccy), Editor.CurrentColor);
							rasterLayer.DrawBrush((int) (cX - ccx), (int) (cY + ccy), Editor.CurrentColor);
							rasterLayer.DrawBrush((int) (cX + ccx), (int) (cY + ccy), Editor.CurrentColor);
							lX = ccx;
							lY = ccy;
						}
					}
				}
			}
			else
			{
				if (Editor.Tools.CurrentTool == Tools.Tool.FullRect)
				{
					for (int rx = fromX; rx < toX + Editor.CurrentBrush.Size; rx++)
						for (int ry = fromY; ry < toY + Editor.CurrentBrush.Size; ry++)
							if (Editor.CurrentPattern.CurrentSubPattern.IsDrawable(rx, ry))
								rasterLayer.Colors[rx + ry * rasterLayer.Width] = Editor.CurrentColor;
				}
				else
				{
					for (int rx = fromX; rx < toX + 1; rx++)
					{
						rasterLayer.DrawBrush(rx, fromY, Editor.CurrentColor);
						rasterLayer.DrawBrush(rx, toY, Editor.CurrentColor);
					}
					for (int ry = fromY + 1; ry < toY; ry++)
					{
						rasterLayer.DrawBrush(fromX, ry, Editor.CurrentColor);
						rasterLayer.DrawBrush(toX, ry, Editor.CurrentColor);
					}
				}
			}
			Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
		}
	}

	public void MouseUp(int x, int y)
	{
		if (TemporaryLayer != null)
		{
			Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Merge(TemporaryLayer);
			Editor.CurrentPattern.CurrentSubPattern.RemoveTemporaryLayer();
			Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.UpdateTexture();
			Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
			TemporaryLayer = null;

			string actionName = "Line Rect";
			if (Editor.Tools.CurrentTool == Tools.Tool.LineCircle)
				actionName = "Line Circle";
			if (Editor.Tools.CurrentTool == Tools.Tool.FullRect)
				actionName = "Full Rect";
			if (Editor.Tools.CurrentTool == Tools.Tool.FullCircle)
				actionName = "Full Circle";
			Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction(actionName, Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors));
		}
	}

	public void MouseMove(int x, int y) { }
	public void SetEditor(PatternEditor editor)
	{
		Editor = editor;
	}
}
