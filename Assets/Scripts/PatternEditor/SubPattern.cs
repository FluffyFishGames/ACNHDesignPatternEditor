using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class SubPattern
{/*
	public class SubPatternColors
	{
		public SubPattern SubPattern;

		public SubPatternColors(SubPattern subPattern)
		{
			SubPattern = subPattern;
		}

		public TextureBitmap.Color this[int index]
		{
			get
			{
				int x = index % SubPattern.Width;
				int y = (index - x) / SubPattern.Width;
				return Pattern.Bitmap.Colors[SubPattern.Part.X + x + (SubPattern.Part.Y + y) * SubPattern.Pattern.Width];
			}
			set
			{
				int x = index % SubPattern.Width;
				int y = (index - x) / SubPattern.Width;
				SubPattern.Pattern.Colors[SubPattern.Part.X + x + (SubPattern.Part.Y + y) * SubPattern.Pattern.Width] = value;
			}
		}
	}
	*/
	public int Width
	{
		get
		{
			return Part.Width;
		}
	}

	public int Height
	{
		get
		{
			return Part.Height;
		}
	}

	//public SubPatternColors Colors;
	public Pattern Pattern;
	public List<Layer> Layers;
	public DesignPatternInformation.DesignPatternPart Part;
	public Layer TemporaryLayer;
	public History History;
	public TextureBitmap Bitmap;

	private int _SelectedLayer = 0;

	public void CreateTemporaryLayer()
	{
		TemporaryLayer = new RasterLayer(this, "Temporary Layer");
	}

	public void RemoveTemporaryLayer()
	{
		TemporaryLayer = null;
		UpdateImage();
	}

	public bool IsDrawable(int px, int py)
	{
		return px >= 0 && px < Width && py >= 0 && py < Height;
	}

	public Layer SelectedLayer
	{
		get
		{
			if (_SelectedLayer >= Layers.Count)
				_SelectedLayer = Layers.Count - 1;
			return Layers[_SelectedLayer];
		}

		set
		{
			for (int i = 0; i < Layers.Count; i++)
			{
				if (Layers[i] == value)
				{
					_SelectedLayer = i;
					return;
				}
			}
		}
	}

	public int SelectedLayerIndex
	{
		get
		{
			return _SelectedLayer;
		}
	}

	public void SelectLayer(int num)
	{
		if (num >= Layers.Count)
			num = Layers.Count - 1;
		var layer = Layers[num];
		Pattern.Editor.OnLayerSelected(num, layer);
		Pattern.Editor.LayersChanged();
	}

	public void SelectLayer(Layer layer)
	{
		if (Layers.Contains(layer))
		{
			_SelectedLayer = Layers.IndexOf(layer);
			Pattern.Editor.OnLayerSelected(_SelectedLayer, layer);
		}
		Pattern.Editor.LayersChanged();
	}

	public void UpdateImage(bool updatePreview = true)
	{
		Bitmap.Clear();
		for (int j = 0; j < Layers.Count; j++)
		{
			if (Layers[j] is SmartObjectLayer sol)
				Bitmap.AlphaComposite(Layers[j].Texture, new TextureBitmap.Color(255, 255, 255, 255), new TextureBitmap.Point(sol.ObjectX, sol.ObjectY));
			else 
				Bitmap.AlphaComposite(Layers[j].Texture, new TextureBitmap.Color(255, 255, 255, 255));
			if (TemporaryLayer != null && j == SelectedLayerIndex)
				Bitmap.AlphaComposite(TemporaryLayer.Texture, new TextureBitmap.Color(255, 255, 255, 255));
		}
		Bitmap.Apply();
		if (updatePreview)
			Pattern.RegeneratePreview();

		Pattern.Bitmap.Replace(this.Bitmap, new TextureBitmap.Color(255, 255, 255, 255), new TextureBitmap.Point(Part.X, Part.Y));
		//Pattern.Bitmap.Apply();
		Pattern.Editor.OnImageUpdated();
	}

	public void Selected()
	{
		Pattern.Editor.OnLayerSelected(_SelectedLayer, SelectedLayer);
		Pattern.Editor.LayersChanged();
	}

	public bool IsVisible(int x, int y)
	{
		int pi = x + y * Width;
		if (Part.Visible.Length > pi)
			return Part.Visible[pi] == '1';
		return false;
	}

	public void CreateLayer()
	{
		var newLayer = new RasterLayer(this, "Layer " + Layers.Count);
		newLayer.UpdateTexture();
		Layers.Insert(_SelectedLayer + 1, newLayer);
		SelectedLayer = newLayer;

		Pattern.Editor.LayersChanged();

		History.AddEvent(new History.LayerCreatedAction("Created: " + newLayer.Name, _SelectedLayer, newLayer));
	}

	public void DeleteLayer(Layer layer)
	{
		var selected = SelectedLayer;
		var lastSelectedIndex = _SelectedLayer;
		int layerIndex = 0;
		for (int i = 0; i < Layers.Count; i++)
		{
			if (Layers[i] == layer)
			{
				if (i == 0) return;
				layerIndex = i;
				Layers.RemoveAt(i);
			}
		}
		
		History.AddEvent(new History.LayerDeletedAction("Deleted: " + layer.Name, layerIndex, layer));

		SelectedLayer = selected;
		if (SelectedLayer == null)
		{
			if (lastSelectedIndex >= Layers.Count)
				lastSelectedIndex = Layers.Count - 1;
			_SelectedLayer = lastSelectedIndex;
		}

		Pattern.Editor.LayersChanged();
		UpdateImage();

	}

	public SubPattern(Pattern pattern, DesignPatternInformation.DesignPatternPart part, bool import = false)
	{
		Bitmap = new TextureBitmap(part.Width, part.Height);
		Bitmap.Clear();
		Bitmap.Texture.filterMode = UnityEngine.FilterMode.Point;
		Pattern = pattern;
		Part = part;
		Layers = new List<Layer>();
		History = new History(this, 50);
		History.OnHistoryChanged = () => {
			Pattern.Editor.Tools.HistoryChanged(History);
		};
		if (!import)
		{
			var backgroundLayer = new RasterLayer(this, "Background");

			unsafe
			{
				var colors = pattern.Bitmap.GetColors();
				var layerColors = backgroundLayer.Texture.GetColors();
				for (int y = 0; y < Height; y++)
					for (int x = 0; x < Width; x++)
						layerColors[x + (Height - 1 - y) * Width] = colors[Part.X + x + (pattern.Height - 1 - (Part.Y + y)) * pattern.Width];
						//backgroundLayer.Texture.SetPixel(x, y, pattern.Bitmap.GetPixel(Part.X + x, (pattern.Height - 1 - Part.Y) + y));
			}
			Layers.Add(backgroundLayer);
			History.AddEvent(new History.LayerCreatedAction("Opened", 0, backgroundLayer));
		}
	}

	public void Dispose()
	{
		this.History.Dispose();
		for (var i = 0; i < this.Layers.Count; i++)
			this.Layers[i].Dispose();
	}
}
