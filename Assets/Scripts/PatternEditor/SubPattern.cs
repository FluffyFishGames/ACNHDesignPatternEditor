using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
public class SubPattern
{
	public class SubPatternColors
	{
		public SubPattern SubPattern;

		public SubPatternColors(SubPattern subPattern)
		{
			SubPattern = subPattern;
		}

		public UnityEngine.Color this[int index]
		{
			get
			{
				int x = index % SubPattern.Width;
				int y = (index - x) / SubPattern.Width;
				return SubPattern.Pattern.Colors[SubPattern.Part.X + x + (SubPattern.Part.Y + y) * SubPattern.Pattern.Width];
			}
			set
			{
				int x = index % SubPattern.Width;
				int y = (index - x) / SubPattern.Width;
				SubPattern.Pattern.Colors[SubPattern.Part.X + x + (SubPattern.Part.Y + y) * SubPattern.Pattern.Width] = value;
			}
		}
	}

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

	public SubPatternColors Colors;
	public Pattern Pattern;
	public List<Layer> Layers;
	public DesignPatternInformation.DesignPatternPart Part;
	public Layer TemporaryLayer;
	public History History;

	private int _SelectedLayer = 0;

	public void CreateTemporaryLayer()
	{
		TemporaryLayer = new RasterLayer(this, "Temporary Layer");
		TemporaryLayer.Colors = new UnityEngine.Color[this.Width * this.Height];
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
		for (int y = 0; y < Height; y++)
		{
			for (int x = 0; x < Width; x++)
			{
				int pi = x + y * Width;
				//int ci = info.Parts[CurrentSubPattern].X + x + (info.Parts[CurrentSubPattern].Y + y) * CurrentPattern.Width;
				var resultC = new UnityEngine.Color(1f, 1f, 1f, 0f);
				for (int j = 0; j < Layers.Count; j++)
				{
					resultC = resultC.AlphaComposite(Layers[j].Colors[pi]);
					if (j == _SelectedLayer && TemporaryLayer != null)
						resultC = resultC.AlphaComposite(TemporaryLayer.Colors[pi]);
				}
				Colors[pi] = resultC;
			}
		}
		if (updatePreview)
			Pattern.RegeneratePreview();
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
		var newLayer = new RasterLayer(this, "Layer " + Layers.Count) { Colors = new UnityEngine.Color[Width * Height] };
		for (int i = 0; i < newLayer.Colors.Length; i++)
			newLayer.Colors[i] = new UnityEngine.Color(0f, 0f, 0f, 0f);
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
		Colors = new SubPatternColors(this);
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
			backgroundLayer.Colors = new UnityEngine.Color[Width * Height];
			for (int y = 0; y < Height; y++)
				for (int x = 0; x < Width; x++)
					backgroundLayer.Colors[x + y * Width] = Colors[x + y * Width];

			backgroundLayer.UpdateTexture();
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
