using Godot;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class ComponentDefinition : HBoxContainer
{
	// Called when the node enters the scene tree for the first time.
	[Export] private ComponentTemplate[] _components;

	private VBoxContainer buttonPanel;

	private Panel _componentPanel;

	private string _buttonTemplate = "res://Scenes/ComponentPanels/component_type_button.tscn";	//button to copy for 'sidepane' buttons.

	private Dictionary<string, CanvasItem> _panelDictionary = new();
	
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		buttonPanel = GetNode<VBoxContainer>("ButtonStrip");
		_componentPanel = GetNode<Panel>("ComponentPanel");
		
		var bg = new ButtonGroup();

		foreach (var c in _components)
		{
			var b = CreateButton(c.ComponentName, c.Icon, bg);
			buttonPanel.AddChild(b);

			var ci = CreateComponentPanel(c.DefinitionDialogName);
			_componentPanel.AddChild(ci);

			_panelDictionary.Add(c.ComponentName, ci);
		}
		
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private string _curItem;

	public string CurName
	{
		get => _curItem;
		set
		{
			_curItem = value;
			UpdatePanelVisibility(_curItem);
		}
	}

	private void UpdatePanelVisibility(string name)
	{
		foreach (var kv in _panelDictionary)
		{
			kv.Value.Visible = (kv.Key == name);
		}
	}

	private Button CreateButton(string name, Texture2D icon, ButtonGroup bg)
	{
		GD.Print($"Trying to create {name}, {_buttonTemplate}");
		var scene = ResourceLoader.Load<PackedScene>(_buttonTemplate).Instantiate();
		
		GD.Print(scene.Name);
		
		if (scene is Button b)
		{
			GD.Print($"Creating button {name}");
			b.Text = name;
			b.Icon = icon;
			b.ButtonGroup = bg;

			b.Pressed += () => ButtonPressed(name);
			return b;
		}

		return new Button();
		
	}

	public void ButtonPressed(string name)
	{
		CurName = name;
	}


	private CanvasItem CreateComponentPanel(string _panelTemplate)
	{
		var scene = ResourceLoader.Load<PackedScene>(_panelTemplate).Instantiate();

		if (scene is CanvasItem ci)
		{
			ci.Visible = false;	 //all panels start out hidden
			return ci;
		}

		return null;
	}
}
