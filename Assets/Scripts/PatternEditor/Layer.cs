using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

public class Layer
{
	protected int _Width;
	protected int _Height;
	public int Width
	{
		get
		{
			return _Width;
		}
	}

	public int Height
	{
		get
		{
			return _Height;
		}
	}

	public string Name;
	protected Color[] _Colors;
	public Color[] Colors 
	{
		get 
		{
			return _Colors;
		}
		set
		{
			_Colors = value;
		}
	}

	public Sprite Sprite;
	public Texture2D Texture;
	public bool IsSelected = false;
	protected SubPattern SubPattern;

	public Layer(SubPattern pattern, string name = "")
	{
		SubPattern = pattern;
		Name = name;
		_Width = pattern.Width;
		_Height = pattern.Height;
	}

	public void Merge(Layer otherLayer)
	{
		for (int i = 0; i < _Colors.Length; i++)
			_Colors[i] = _Colors[i].AlphaComposite(otherLayer._Colors[i]);
	}

	public void UpdateTexture()
	{
		if (Texture == null)
		{
			Texture = new Texture2D(_Width, _Height, TextureFormat.ARGB32, false);
			Sprite = Sprite.Create(Texture,new Rect(0, 0, _Width, _Height), new Vector2(0.5f, 0.5f));
		}
		for (int y = 0; y < Texture.height; y++)
			for (int x = 0; x < Texture.width; x++)
				Texture.SetPixel(x, Texture.height - y, _Colors[x + y * _Width]);

		Texture.Apply();
	}

	public virtual void Dispose()
	{
		GameObject.DestroyImmediate(Sprite);
		GameObject.DestroyImmediate(Texture);
	}
}