using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class History
{
	public class HistoryEvent
	{
		public string Name;
		public HistoryEvent(string name)
		{
			this.Name = name;
		}

		public virtual void RestoreBackward(History history)
		{ 
		}

		public virtual void RestorerForward(History history)
		{

		}

		public virtual void Dispose()
		{

		}
	}

	public class ChangedLayerAction : HistoryEvent
	{
		private IntPtr FromColors;
		private IntPtr ToColors;
		private int LayerIndex;

		public ChangedLayerAction(string name, int layerIndex, IntPtr previousColors, IntPtr newColors) : base (name)
		{
			this.LayerIndex = layerIndex;
			this.FromColors = previousColors;
			this.ToColors = newColors;
		}

		public override void RestoreBackward(History history)
		{
			var layer = history.SubPattern.Layers[this.LayerIndex] as RasterLayer;
			layer.Texture.CopyFrom(FromColors);
		}

		public override void RestorerForward(History history)
		{
			var layer = history.SubPattern.Layers[this.LayerIndex] as RasterLayer;
			layer.Texture.CopyFrom(ToColors);
		}

		public override void Dispose()
		{
			base.Dispose();
			Marshal.FreeHGlobal(FromColors);
			Marshal.FreeHGlobal(ToColors);
		}
	}

	public class ChangeTransformAction : HistoryEvent
	{
		private int LayerIndex;
		private int ObjectX;
		private int ObjectY;
		private int ObjectWidth;
		private int ObjectHeight;
		private int NewX;
		private int NewY;
		private int NewWidth;
		private int NewHeight;

		public ChangeTransformAction(string name, int layerIndex, int objectX, int objectY, int objectWidth, int objectHeight, int newX, int newY, int newWidth, int newHeight) : base(name)
		{
			this.LayerIndex = layerIndex;
			this.ObjectX = objectX;
			this.ObjectY = objectY;
			this.ObjectWidth = objectWidth;
			this.ObjectHeight = objectHeight;
			this.NewX = newX;
			this.NewY = newY;
			this.NewWidth = newWidth;
			this.NewHeight = newHeight;
		}

		public override void RestoreBackward(History history)
		{
			var layer = history.SubPattern.Layers[this.LayerIndex] as SmartObjectLayer;
			layer.ObjectX = ObjectX;
			layer.ObjectY = ObjectY;
			layer.ObjectWidth = ObjectWidth;
			layer.ObjectHeight = ObjectHeight;
		}

		public override void RestorerForward(History history)
		{
			var layer = history.SubPattern.Layers[this.LayerIndex] as SmartObjectLayer;
			layer.ObjectX = NewX;
			layer.ObjectY = NewY;
			layer.ObjectWidth = NewWidth;
			layer.ObjectHeight = NewHeight;
		}
	}

	public class LayerMoveAction : HistoryEvent
	{
		private int LayerIndexFrom;
		private int LayerIndexTo;

		public LayerMoveAction(string name, int layerIndexFrom, int layerIndexTo) : base(name)
		{
			this.LayerIndexFrom = layerIndexFrom;
			this.LayerIndexTo = layerIndexTo;
		}

		public override void RestoreBackward(History history)
		{
			var l = history.SubPattern.Layers[LayerIndexTo];
			history.SubPattern.Layers.RemoveAt(LayerIndexTo);
			history.SubPattern.Layers.Insert(LayerIndexFrom, l);
		}

		public override void RestorerForward(History history)
		{
			var l = history.SubPattern.Layers[LayerIndexFrom];
			history.SubPattern.Layers.RemoveAt(LayerIndexFrom);
			history.SubPattern.Layers.Insert(LayerIndexTo, l);
		}
	}

	public class LayerDeletedAction : HistoryEvent
	{
		private int LayerIndex;
		private string LayerName;
		private TextureBitmap Bitmap;
		private IntPtr Colors;
		private int ObjectX;
		private int ObjectY;
		private int ObjectWidth;
		private int ObjectHeight;

		public LayerDeletedAction(string name, int layerIndex, Layer layer) : base(name)
		{
			this.LayerIndex = layerIndex;
			this.LayerName = layer.Name;
			if (layer is SmartObjectLayer sol)
			{
				Bitmap = sol.Bitmap.Clone();
				ObjectX = sol.ObjectX;
				ObjectY = sol.ObjectY;
				ObjectWidth = sol.ObjectWidth;
				ObjectHeight = sol.ObjectHeight;
			}
			else
			{
				Colors = Marshal.AllocHGlobal(layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize);
				unsafe
				{
					Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), Colors.ToPointer(), layer.Texture.Width * layer.Texture.Height * 4, layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize);
				}
			}
		}


		public override void RestoreBackward(History history)
		{
			if (this.Bitmap != null)
			{
				var newLayer = new SmartObjectLayer(history.SubPattern, this.LayerName, this.Bitmap, this.ObjectX, this.ObjectY, this.ObjectWidth, this.ObjectHeight);
				newLayer.Bitmap = Bitmap.Clone();
				history.SubPattern.Layers.Insert(this.LayerIndex, newLayer);
			}
			else
			{
				var newLayer = new RasterLayer(history.SubPattern, this.LayerName);
				newLayer.Texture.CopyFrom(Colors);
				history.SubPattern.Layers.Insert(this.LayerIndex, newLayer);
			}
		}

		public override void RestorerForward(History history)
		{
			history.SubPattern.Layers.RemoveAt(LayerIndex);
		}

		public override void Dispose()
		{
			base.Dispose();
			if (this.Bitmap != null)
				this.Bitmap.Dispose();
			else
				Marshal.FreeHGlobal(this.Colors);
		}
	}


	public class TransformChangeCropResampling : HistoryEvent
	{
		private int LayerIndex;
		private TextureBitmap.CropMode PreviousCrop;
		private ResamplingFilters PreviousSampling;
		private TextureBitmap.CropMode NewCrop;
		private ResamplingFilters NewSampling;

		public TransformChangeCropResampling(string name, int layerIndex, TextureBitmap.CropMode previousCrop, ResamplingFilters previousSampling, TextureBitmap.CropMode newCrop, ResamplingFilters newSampling) : base(name)
		{
			this.LayerIndex = layerIndex;
			this.PreviousCrop = previousCrop;
			this.PreviousSampling = previousSampling;
			this.NewCrop = newCrop;
			this.NewSampling = newSampling;
		}

		public override void RestoreBackward(History history)
		{
			var smo = history.SubPattern.Layers[this.LayerIndex] as SmartObjectLayer;
			smo.ChangeCrop(this.PreviousCrop);
			smo.ChangeResampling(this.PreviousSampling);
		}

		public override void RestorerForward(History history)
		{
			var smo = history.SubPattern.Layers[this.LayerIndex] as SmartObjectLayer;
			smo.ChangeCrop(this.NewCrop);
			smo.ChangeResampling(this.NewSampling);
		}
	}

	public class LayerCreatedAction : HistoryEvent
	{
		private int LayerIndex;
		private string LayerName;
		private TextureBitmap Bitmap;
		private IntPtr Colors;
		private int ObjectX;
		private int ObjectY;
		private int ObjectWidth;
		private int ObjectHeight;
		
		public LayerCreatedAction(string name, int layerIndex, Layer layer) : base(name)
		{
			this.LayerIndex = layerIndex;
			this.LayerName = layer.Name;
			if (layer is SmartObjectLayer sol)
			{
				Bitmap = sol.Bitmap.Clone();
				ObjectX = sol.ObjectX;
				ObjectY = sol.ObjectY;
				ObjectWidth = sol.ObjectWidth;
				ObjectHeight = sol.ObjectHeight;
			}
			else
			{
				Colors = Marshal.AllocHGlobal(layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize);
				unsafe
				{
					Buffer.MemoryCopy(layer.Texture.Bytes.ToPointer(), Colors.ToPointer(), layer.Texture.Width * layer.Texture.Height * 4, layer.Texture.Width * layer.Texture.Height * layer.Texture.PixelSize);
				}
			}
		}

		public override void RestoreBackward(History history)
		{
			history.SubPattern.Layers.RemoveAt(LayerIndex);
		}

		public override void RestorerForward(History history)
		{
			if (this.Bitmap != null)
			{
				var newLayer = new SmartObjectLayer(history.SubPattern, this.LayerName, this.Bitmap, this.ObjectX, this.ObjectY, this.ObjectWidth, this.ObjectHeight);
				newLayer.Bitmap = Bitmap.Clone();
				history.SubPattern.Layers.Insert(this.LayerIndex, newLayer);
			}
			else
			{
				var newLayer = new RasterLayer(history.SubPattern, this.LayerName);
				newLayer.Texture.CopyFrom(Colors);
				history.SubPattern.Layers.Insert(this.LayerIndex, newLayer);
			}
		}

		public override void Dispose()
		{
			base.Dispose();
			if (this.Bitmap != null)
				this.Bitmap.Dispose();
			else
				Marshal.FreeHGlobal(this.Colors);
		}
	}

	public List<HistoryEvent> Events;
	public int MaxEvents;
	public int CurrentEvent = 0;
	public SubPattern SubPattern;
	public System.Action OnHistoryChanged;

	public History(SubPattern subPattern, int maxEvents)
	{
		this.SubPattern = subPattern;
		this.MaxEvents = maxEvents;
		this.Events = new List<HistoryEvent>();
	}

	public void AddEvent(HistoryEvent ev)
	{
		if (this.Events.Count - (this.CurrentEvent + 1) > 0)
			this.Events.RemoveRange(this.CurrentEvent + 1, this.Events.Count - (this.CurrentEvent + 1));
		this.Events.Add(ev);
		this.CurrentEvent = this.Events.Count - 1;

		if (this.Events.Count > MaxEvents)
		{
			for (int i = 0; i < this.Events.Count - MaxEvents; i++)
				this.Events[i].Dispose();
			this.Events.RemoveRange(0, this.Events.Count - MaxEvents);
		}
		OnHistoryChanged?.Invoke();
	}

	public void RestoreTo(int ev)
	{
		if (ev > this.CurrentEvent)
		{
			for (int i = this.CurrentEvent + 1; i <= ev; i++)
			{
				this.Events[i].RestorerForward(this);
			}
		}
		else
		{
			for (int i = this.CurrentEvent; i > ev; i--)
			{
				this.Events[i].RestoreBackward(this);
			}
		}
		this.CurrentEvent = ev;

		for (int i = 0; i < SubPattern.Layers.Count; i++)
		{
			if (SubPattern.Layers[i] is SmartObjectLayer smo)
				smo.UpdateColors();
			SubPattern.Layers[i].UpdateTexture();
		}
		SubPattern.UpdateImage();
		SubPattern.Pattern.Editor.LayersChanged();
		OnHistoryChanged?.Invoke();
	}

	public void Dispose()
	{
		for (var i = 0; i < Events.Count; i++)
			Events[i].Dispose();
	}
}
