using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EraseTool : ITool
{
    private PatternEditor Editor;
    public void Destroyed() { }
    public void SetEditor(PatternEditor editor)
    {
        this.Editor = editor;
    }

    private UnityEngine.Color[] OldColors;

    public void MouseDown(int x, int y)
    {
        OldColors = new UnityEngine.Color[Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors.Length];
        System.Array.Copy(Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors, OldColors, OldColors.Length);

        MouseDrag(x, y, x, y);
    }

    public void MouseDrag(int x, int y, int previousX, int previousY)
    {
        if (this.Editor.CurrentPattern.CurrentSubPattern.SelectedLayer is IBrushDrawable drawable)
        {
            this.Editor.CurrentBrush.Erase(drawable, x, y);
            this.Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.UpdateTexture();
            this.Editor.CurrentPattern.CurrentSubPattern.UpdateImage();
        }
    }

    public void MouseMove(int x, int y)
    {
    }

    public void MouseUp(int x, int y)
    {
        Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Erase", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, Editor.CurrentPattern.CurrentSubPattern.SelectedLayer.Colors));
    }
}