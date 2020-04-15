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
        this.Editor.ChangeCurrentColor(this.Editor.CurrentPattern.CurrentSubPattern.Colors[x + y * this.Editor.CurrentPattern.CurrentSubPattern.Width]);
    }

    public void MouseMove(int x, int y){}
    public void MouseUp(int x, int y){}
}