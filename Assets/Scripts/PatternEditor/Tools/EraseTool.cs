using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;

public class EraseTool : ITool
{
    private PatternEditor Editor;
    public void Destroyed() { }
    public void SetEditor(PatternEditor editor)
    {
        this.Editor = editor;
    }

    private IntPtr OldColors;

    public void MouseDown(int x, int y)
    {
        var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
        var size = layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize;
        OldColors = Marshal.AllocHGlobal(size);
        unsafe
        {
            System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), OldColors.ToPointer(), size, size);
        }

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
        var layer = Editor.CurrentPattern.CurrentSubPattern.SelectedLayer;
        var size = layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize;
        var newColors = Marshal.AllocHGlobal(size);
        unsafe
        {
            System.Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), newColors.ToPointer(), size, size);
        }
        Editor.CurrentPattern.CurrentSubPattern.History.AddEvent(new History.ChangedLayerAction("Erase", Editor.CurrentPattern.CurrentSubPattern.SelectedLayerIndex, OldColors, newColors));
    }
}