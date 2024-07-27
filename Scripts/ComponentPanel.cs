using Godot;
using System;
using System.Collections.Generic;
using System.Diagnostics;

public partial class ComponentPanel : Panel
{
	[Export] private ComponentTemplate[] components;
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		foreach (var c in components)
		{
			var scene = ResourceLoader.Load<PackedScene>(c.DefinitionDialogName).Instantiate();

			if (scene is CanvasItem ci)
			{
				ci.Visible = true;
				AddChild(ci);
				GD.Print($"Adding {c.DefinitionDialogName}");
			}
			
			
		}
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}
}
