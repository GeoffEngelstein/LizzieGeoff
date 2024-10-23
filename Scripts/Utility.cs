using Godot;
using System;

public partial class Utility : Node
{
	public static Utility Instance { get; private set; }

	private TokenTextureSubViewport _textureCreator;
	
	public override void _Ready()
	{
		Instance = this;
		_textureCreator = GetNode<TokenTextureSubViewport>("TextureCreator");
	}

	public ViewportTexture CreateQuickTexture(TokenTextureParameters parameters)
	{
		return _textureCreator.CreateQuickTexture(parameters);
	}

	public float GetAabbSize(Aabb aabb)
	{
		return aabb.GetLongestAxisSize();
	}
	
}
