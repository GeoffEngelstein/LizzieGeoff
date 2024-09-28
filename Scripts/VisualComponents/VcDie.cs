using Godot;
using System;
using System.Collections.Generic;

public partial class VcDie : VisualComponentBase
{
	[Export] private int _sides;
	[Export] private Vector3[] _sideRotations;

	private MeshInstance3D _mainMesh;

	public override void _Ready()
	{
		base._Ready();
		Visible = true;
		
		_mainMesh = GetNode<MeshInstance3D>("ObjectMesh");
		HighlightMesh = GetNode<MeshInstance3D>("HighlightMesh");
		
		ComponentType = VisualComponentType.Die;
	}

	private bool _rollInProcess;
	private int _rollTarget;
	private double _rollDuration = 0.5;
	private double _rollTime;

	public override void _Process(double delta)
	{
		if (_rollInProcess)
		{
			_rollTime += delta;
			if (_rollTime > _rollDuration)
			{
				ShowSide(_rollTarget);
				_rollInProcess = false;
			}
			else
			{
				ShowSide((int)(GD.Randi() % _sides +1));
			}
		}
	}

	public override GeometryInstance3D DragMesh => _mainMesh;
	
	public override CommandResponse ProcessCommand(SceneController.VisualCommand command)
	{
		var cr = new CommandResponse(false, null);
		
		switch (command)
		{
			case SceneController.VisualCommand.ToggleLock:
				break;
			case SceneController.VisualCommand.Flip:
				break;
			case SceneController.VisualCommand.ScaleUp:
				break;
			case SceneController.VisualCommand.ScaleDown:
				break;
			case SceneController.VisualCommand.RotateCw:
				break;
			case SceneController.VisualCommand.RotateCcw:
				break;
			case SceneController.VisualCommand.Delete:
				break;
			case SceneController.VisualCommand.Duplicate:
				break;
			case SceneController.VisualCommand.Edit:
				break;
			case SceneController.VisualCommand.MoveDown:
				break;
			case SceneController.VisualCommand.MoveToBottom:
				break;
			case SceneController.VisualCommand.MoveUp:
				break;
			case SceneController.VisualCommand.MoveToTop:
				break;
			
			case SceneController.VisualCommand.Num1:
				ShowSide(1);
				break;
			case SceneController.VisualCommand.Num2:
				ShowSide(2);
				break;
			case SceneController.VisualCommand.Num3:
				ShowSide(3);
				break;
			case SceneController.VisualCommand.Num4:
				ShowSide(4);
				break;
			case SceneController.VisualCommand.Num5:
				ShowSide(5);
				break;
			case SceneController.VisualCommand.Num6:
				ShowSide(6);
				break;
			case SceneController.VisualCommand.Num7:
				ShowSide(7);
				break;
			case SceneController.VisualCommand.Num8:
				ShowSide(8);
				break;
			case SceneController.VisualCommand.Num9:
				ShowSide(9);
				break;
			case SceneController.VisualCommand.Num10:
				ShowSide(10);
				break;
			case SceneController.VisualCommand.Num11:
				ShowSide(11);
				break;
			case SceneController.VisualCommand.Num12:
				ShowSide(12);
				break;
			case SceneController.VisualCommand.Num13:
				ShowSide(13);
				break;
			case SceneController.VisualCommand.Num14:
				ShowSide(14);
				break;
			case SceneController.VisualCommand.Num15:
				ShowSide(15);
				break;
			case SceneController.VisualCommand.Num16:
				ShowSide(16);
				break;
			case SceneController.VisualCommand.Num17:
				ShowSide(17);
				break;
			case SceneController.VisualCommand.Num18:
				ShowSide(18);
				break;
			case SceneController.VisualCommand.Num19:
				ShowSide(19);
				break;
			case SceneController.VisualCommand.Num20:
				ShowSide(20);
				break;
			
			case SceneController.VisualCommand.Roll:
				Roll();
				break;

			default:
				throw new ArgumentOutOfRangeException(nameof(command), command, null);
		}
		
		if (cr.Consumed == false) return base.ProcessCommand(command);

		return cr;
	}
	
	public override List<MenuCommand> GetMenuCommands()
	{
		var l = new List<MenuCommand>();

		foreach (var i in base.GetMenuCommands())
		{
			l.Add(i);
		}

		l.Add(new MenuCommand(SceneController.VisualCommand.Roll));
		
		return l;
	}

	private void Roll()
	{
		_rollTarget = (int)(GD.Randi() % _sides + 1);
		_rollInProcess = true;
		_rollTime = 0;
	}

	private void ShowSide(int side)
	{
		if (side > _sideRotations.Length) return;

		Rotation = _sideRotations[side - 1] * (3.14159f / 180f);	//convert to radians
	}

	public override bool Build(Dictionary<string, object> parameters)
	{
		base.Build(parameters);

		_mainMesh = GetNode<MeshInstance3D>("ObjectMesh");
		
		float size = 0;

		if (parameters.ContainsKey("Size"))
		{
			if (parameters["Size"] is float h)
			{
				if (h <= 0) return false;
				size = h / 10f;
			}
		}

		if (parameters.ContainsKey("Color"))
		{
			if (parameters["Color"] is Color color)
			{
				if (_mainMesh.GetSurfaceOverrideMaterial(0) is StandardMaterial3D material)
				{
					material.AlbedoColor = color;
				}
			}
		}

		YHeight = size;
		
		//create card
		Scale = new Vector3(size, size, size);
		
		return true;
	}
	
	
	public override List<string> ValidateParameters(Dictionary<string, object> parameters)
	{
		return new List<string>();
	}
}
