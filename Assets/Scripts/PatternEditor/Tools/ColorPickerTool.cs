using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ColorPickerTool : ITool
{
    private PatternEditor Editor;
    public void Destroyed() { }
    public void SetEditor(PatternEditor editor)
    {
        this.Editor = editor;
    }

    public void MouseDown(int x, int y)
    {
        MouseDrag(x, y, x, y);
    }

    public void MouseDrag(int x, int y, int previousX, int previousY)
    {
        if (x >= 0 && x < Editor.CurrentPattern.CurrentSubPattern.Bitmap.Width && y >= 0 && y < Editor.CurrentPattern.CurrentSubPattern.Bitmap.Height)
        {
            var col = Editor.CurrentPattern.CurrentSubPattern.Bitmap.GetPixel(x, Editor.CurrentPattern.CurrentSubPattern.Bitmap.Height - 1 - y);
            this.Editor.ChangeCurrentColor(new Color(col.R / 255f, col.G / 255f, col.B / 255f, col.A / 255f));
        }
    }

    public void MouseMove(int x, int y) { }
    public void MouseUp(int x, int y) { }
}