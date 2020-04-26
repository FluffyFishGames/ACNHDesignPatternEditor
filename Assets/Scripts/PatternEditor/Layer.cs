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
	public Sprite Sprite;
	public bool IsSelected = false;
	protected SubPattern SubPattern;
	public TextureBitmap Texture;

	public Layer(SubPattern pattern, string name = "")
	{
		SubPattern = pattern;
		Name = name;
		_Width = pattern.Width;
		_Height = pattern.Height;
		Texture = new TextureBitmap(_Width, _Height);
		Texture.Clear();
		Sprite = Sprite.Create(Texture.Texture, new Rect(0, 0, Texture.Width, Texture.Height), new Vector2(0.5f, 0.5f));
	}

	public void Merge(Layer otherLayer)
	{
		Texture.AlphaComposite(otherLayer.Texture, new TextureBitmap.Color(255, 255, 255, 255));
	}

	public void UpdateTexture()
	{
		Texture.Apply();
	}

	public virtual void Dispose()
	{
		GameObject.DestroyImmediate(Sprite);
		Texture.Dispose();
	}
}