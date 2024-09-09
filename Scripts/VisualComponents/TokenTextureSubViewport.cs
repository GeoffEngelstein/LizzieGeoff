using Godot;
using System;

public partial class TokenTextureSubViewport : SubViewport
{
	private ColorRect _square;
	private Label _label;
	private TextureRect _hexPoint;
	private TextureRect _hexFlat;
	private TextureRect _circle;
	private LabelSettings _labelSettings;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		_square = GetNode<ColorRect>("ColorRect");
		_label = GetNode<Label>("Label");
		_labelSettings = new LabelSettings();
		_labelSettings.FontSize = 24;
		_labelSettings.FontColor = Colors.Black;
		_label.LabelSettings = _labelSettings;

		_hexPoint = GetNode<TextureRect>("HexPoint");
		_hexFlat = GetNode<TextureRect>("HexFlat");
		_circle = GetNode<TextureRect>("Circle");
		
		SetShape(TokenShape.Square);
	}

	public void SetBackgroundColor(Color color)
	{
		_square.Color = color;
		_hexPoint.Modulate = color;
		_hexFlat.Modulate = color;
		_circle.Modulate = color;
	}

	public void SetText(string text)
	{
		_label.Text = text;
	}

	public void SetTextColor(Color color)
	{
		_label.LabelSettings.FontColor = color;
	}
	
	public enum TokenShape {Square, Circle, HexPoint, HexFlat}

	public void SetShape(TokenShape shape)
	{
		_square.Visible = (shape == TokenShape.Square);
		_circle.Visible = (shape == TokenShape.Circle);
		_hexPoint.Visible = (shape == TokenShape.HexPoint);
		_hexFlat.Visible = (shape == TokenShape.HexFlat);
	}


}
