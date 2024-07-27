using Godot;
using System;

public partial class ComponentTemplate : Resource
{
	[Export] public string ComponentName;
	[Export] public Texture2D Icon;
	[Export] public PackedScene DefinitionDialog;
	[Export] public string DefinitionDialogName;
}
