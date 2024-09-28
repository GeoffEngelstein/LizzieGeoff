using Godot;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Security.AccessControl;
using System.Text;
using System.Threading.Tasks.Dataflow;

public partial class SceneController : Node3D
{
	private Pseudo2DCamera _2dCamera;
	private CameraController _3dCamera;
	private Node _gameObjects;
	private ICameraBase _currentCamera;

	private int _stackingUpdateRequired;

	private DragSelectRectangle _dragSelectRectangle;

	private UndoService _undoService = new UndoService();

	[Export] private int _popupOffset = 32;

	private enum CursorMode
	{
		Normal,
		Spawn,
		Drag,
		DragSelect,
		PopupMenu
	}

	private CursorMode _uiMode = CursorMode.Normal;

	public enum SceneMode
	{
		TwoD,
		ThreeDFixed,
		ThreeDPhysics,
		Creator
	}

	public override void _Ready()
	{
		_3dCamera = GetNode<CameraController>("CameraBase");
		_2dCamera = GetNode<Pseudo2DCamera>("Pseudo2DCamera");
		_gameObjects = GetNode<Node>("GameObjects");
		_currentCamera = _2dCamera;
		_dragSelectRectangle = GetNode<DragSelectRectangle>("DragSelectRectangle");
	}





	private Vector2 UpperLeftCorner(VisualComponentBase obj)
	{
		var aabb = obj.Aabb;

		//float maxY = float.MinValue;
		float minX = float.MaxValue;
		float minY = float.MaxValue;

		for (int i = 0; i < 8; i++)
		{
			var c = _currentCamera.Camera.UnprojectPosition(aabb.GetEndpoint(i));
			minY = Mathf.Min(minY, c.Y);
			minX = Mathf.Min(minX, c.X);
		}

		int minYI = (int)Math.Floor(minY);
		int minXI = (int)Math.Floor(minX);
		GD.Print($"offset: {minX} {minY}");
		return new Vector2I(minXI - _popupOffset, minYI - _popupOffset);
	}

	public override void _PhysicsProcess(double delta)
	{
		if (_stackingUpdateRequired > 0)
		{
			_stackingUpdateRequired--;

			if (_stackingUpdateRequired <= 0)
			{
				UpdateStackingHeights();
				_stackingUpdateRequired = 0;
			}
		}
	}

	public override void _UnhandledInput(InputEvent @event)
	{
		switch (_uiMode)
		{
			case CursorMode.Normal:
				HandleNormal(@event);
				break;

			case CursorMode.Spawn:
				HandleSpawnMode(@event);
				break;

			case CursorMode.Drag:
				HandleDrag(@event);
				break;

			case CursorMode.DragSelect:
				HandleDragSelect();
				break;
			
			case CursorMode.PopupMenu:
				HandlePopupMenu();
				break;

			default:
				throw new ArgumentOutOfRangeException();
		}


		//do not process commands or rotate/zoom while in Drag Select or Popup modes
		if (_uiMode != CursorMode.DragSelect && _uiMode != CursorMode.PopupMenu)
		{
			CheckForCommands();

			_currentCamera.ProcessViewEvent(@event);

			//check for zoom
			if (@event.IsActionPressed("zoom_in")) _currentCamera.ZoomIn();
			if (@event.IsActionPressed("zoom_out")) _currentCamera.ZoomOut();
			if (@event.IsActionPressed("reset_view")) _currentCamera.ResetView();
			if (@event.IsActionPressed("move_to_top")) MoveToTop();
			if (@event.IsActionPressed("move_to_bottom")) MoveToBottom();
			if (@event.IsActionPressed("exit_mode")) ResetModes();
			if (@event.IsActionPressed("ui_undo")) ProcessUndo();
		}

		base._Input(@event);
	}

	private void ProcessUndo()
	{
		_undoService.Undo();
	}

	private void ResetModes()
	{
		DeselectClickedComponents();

		_uiMode = CursorMode.Normal;
	}

	private void DeselectClickedComponents()
	{
		foreach (var go in _gameObjects.GetChildren())
		{
			if (go is VisualComponentBase v) v.IsClickSelected = false;
		}
	}

	#region Normal

	private void HandleNormal(InputEvent @event)
	{
		if (@event is InputEventMouseButton mb)
		{
			if (mb.ButtonIndex == MouseButton.Right && IsObjectHovered())
			{
				StartPopupMenu();
			}

			if (mb.ButtonIndex == MouseButton.Left)
			{
				var go = GetMouseSelectedObject(); //only start dragging if we are hovering an item
				if (go == null)
				{
					DeselectClickedComponents();
					_uiMode = CursorMode.DragSelect;
					StartDragSelect();
				}
				else
				{
					_uiMode = CursorMode.Drag;
					StartDragUndo(go);
					_currentCamera.StartDrag();
				}
			}
		}
	}

	private Change _dragChange;

	private void StartDragUndo(VisualComponentBase go)
	{
		//TODO Handle multiple items being dragged
		_dragChange = new()
		{
			Component = go,
			Begin = go.Transform
		};
	}

	private void EndDragUndo()
	{
		_dragChange.End = _dragChange.Component.Transform;
		_undoService.Add(_dragChange);
	}

	#endregion

	#region PopupMenu

	private void StartPopupMenu()
	{
		_uiMode = CursorMode.PopupMenu;
		
		var m = GetViewport().GetMousePosition();

		Vector2I v = new Vector2I((int)Math.Floor(m.X), (int)Math.Floor(m.Y));
		var vch = new List<VisualComponentBase> { GetHoveredObject() };
		GetParent<GameController>().ShowComponentPopup(v, vch);
	}

	private void EndPopupMenu()
	{
		if (_uiMode == CursorMode.PopupMenu) _uiMode = CursorMode.Normal;
	}

	public void PopupClosed()
	{
		EndPopupMenu();
	}

	private void HandlePopupMenu()
	{
		//
	}

	#endregion
	
	#region DragSelect

	private void HandleDragSelect()
	{
		if (!Input.IsMouseButtonPressed(MouseButton.Left))
		{
			StopDragSelect();
		}
		else
		{
			ProcessDragSelect();
		}
	}

	private Vector2 _dragSelectStart;

	private void StartDragSelect()
	{
		GD.Print("Start Drag Select");
		_uiMode = CursorMode.DragSelect;
		_dragSelectRectangle.StartDragSelect(_dragSelectStart);
	}

	private void StopDragSelect()
	{
		_uiMode = CursorMode.Normal;
		_dragSelectRectangle.StopDragSelect();
	}

	private void ProcessDragSelect()
	{
		var rect = _dragSelectRectangle.CurRectangle;

		foreach (var go in _gameObjects.GetChildren())
		{
			if (go is VisualComponentBase vcb)
			{
				var screenPos = _currentCamera.Camera.UnprojectPosition(vcb.Position);
				vcb.IsClickSelected = PointInRect(screenPos, rect);
			}
		}
	}

	private bool PointInRect(Vector2 point, Rect2 rect)
	{
		//normalize in case the size is negative
		float minX = Mathf.Min(rect.Position.X, rect.Position.X + rect.Size.X);
		float maxX = Mathf.Max(rect.Position.X, rect.Position.X + rect.Size.X);

		float minY = Mathf.Min(rect.Position.Y, rect.Position.Y + rect.Size.Y);
		float maxY = Mathf.Max(rect.Position.Y, rect.Position.Y + rect.Size.Y);

		return (point.X >= minX && point.X <= maxX
								&& point.Y >= minY && point.Y <= maxY);
	}

	#endregion

	#region Drag

	private void HandleDrag(InputEvent @event)
	{
		var mouseMotion = new Vector2(0, 0);
		if (@event is InputEventMouseMotion mouse)
		{
			mouseMotion = mouse.Relative;
		}

		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			_currentCamera.ProcessDrag(mouseMotion);
		}
		else
		{
			_uiMode = CursorMode.Normal;
			QueueStackingUpdate();
			EndDragUndo();
			_currentCamera.StopDrag();
		}
	}

	#endregion


	#region Spawn

	private void HandleSpawnMode(InputEvent @event)
	{
		if (Input.IsMouseButtonPressed(MouseButton.Left))
		{
			SpawnComponent();
			QueueStackingUpdate();
		}
		else
		{
			_spawnDebounce = false;
			if (@event.IsActionPressed("exit_mode"))
			{
				ExitSpawnMode();
				QueueStackingUpdate();
			}
		}
	}

	public void EnterSpawnMode(VisualComponentBase component)
	{
		_uiMode = CursorMode.Spawn;
		_spawnComponent = component;
		_spawnComponent.DimMode(true);
		_currentCamera.EnterSpawnMode(component);
		_gameObjects.AddChild(component);
	}

	public void ExitSpawnMode()
	{
		GD.Print("Exit Spawn Mode");
		_currentCamera.ExitSpawnMode();

		_uiMode = CursorMode.Normal;

		if (!IsInstanceValid(_spawnComponent)) return;

		if (_spawnComponent == null) return;

		if (_spawnComponent.IsQueuedForDeletion()) return;

		_spawnComponent?.QueueFree();
		//_spawnComponent = null;
	}

	private bool _spawnDebounce;

	private void SpawnComponent()
	{
		if (_spawnComponent == null || _spawnDebounce) return;

		var newComp = (VisualComponentBase)_spawnComponent.Duplicate();
		newComp.Build(_spawnComponent.Parameters);
		newComp.DimMode(false);
		newComp.ZOrder = GetMaxComponentZ() + 1;
		newComp.NeverHighlight = false;
		GD.Print($"New Z: {newComp.ZOrder}");

		var spawnPos = _currentCamera.GetSpawnPos();
		newComp.Position = new Vector3(spawnPos.X, newComp.YHeight / 2f, spawnPos.Z);

		_gameObjects.AddChild(newComp);
		QueueStackingUpdate();

		_spawnDebounce = true;
	}

	#endregion

	#region Commands

	public enum VisualCommand
	{
		None,
		ToggleLock,
		Flip,
		ScaleUp,
		ScaleDown,
		RotateCw,
		RotateCcw,
		Delete,
		Duplicate,
		Edit,
		MoveDown,
		MoveToBottom,
		MoveUp,
		MoveToTop,
		Num1,
		Num2,
		Num3,
		Num4,
		Num5,
		Num6,
		Num7,
		Num8,
		Num9,
		Num10,
		Num11,
		Num12,
		Num13,
		Num14,
		Num15,
		Num16,
		Num17,
		Num18,
		Num19,
		Num20,
		Roll,
		MaximumVC
	}

	private void CheckForCommands()
	{
		if (Input.IsActionJustPressed("flip")) SendCommandToSelected(VisualCommand.Flip);
		if (Input.IsActionPressed("lock")) SendCommandToSelected(VisualCommand.ToggleLock);

		if (Input.IsActionPressed("num_1")) SendCommandToSelected(VisualCommand.Num1);
		if (Input.IsActionPressed("num_2")) SendCommandToSelected(VisualCommand.Num2);
		if (Input.IsActionPressed("num_3")) SendCommandToSelected(VisualCommand.Num3);
		if (Input.IsActionPressed("num_4")) SendCommandToSelected(VisualCommand.Num4);
		if (Input.IsActionPressed("num_5")) SendCommandToSelected(VisualCommand.Num5);
		if (Input.IsActionPressed("num_6")) SendCommandToSelected(VisualCommand.Num6);
		if (Input.IsActionPressed("num_7")) SendCommandToSelected(VisualCommand.Num7);
		if (Input.IsActionPressed("num_8")) SendCommandToSelected(VisualCommand.Num8);
		if (Input.IsActionPressed("num_9")) SendCommandToSelected(VisualCommand.Num9);
		if (Input.IsActionPressed("num_10")) SendCommandToSelected(VisualCommand.Num10);
		if (Input.IsActionPressed("num_11")) SendCommandToSelected(VisualCommand.Num11);
		if (Input.IsActionPressed("num_12")) SendCommandToSelected(VisualCommand.Num12);
		if (Input.IsActionPressed("num_13")) SendCommandToSelected(VisualCommand.Num13);
		if (Input.IsActionPressed("num_14")) SendCommandToSelected(VisualCommand.Num14);
		if (Input.IsActionPressed("num_15")) SendCommandToSelected(VisualCommand.Num15);
		if (Input.IsActionPressed("num_16")) SendCommandToSelected(VisualCommand.Num16);
		if (Input.IsActionPressed("num_17")) SendCommandToSelected(VisualCommand.Num17);
		if (Input.IsActionPressed("num_18")) SendCommandToSelected(VisualCommand.Num18);
		if (Input.IsActionPressed("num_19")) SendCommandToSelected(VisualCommand.Num19);
		if (Input.IsActionPressed("num_20")) SendCommandToSelected(VisualCommand.Num20);

		if (Input.IsActionPressed("roll")) SendCommandToSelected(VisualCommand.Roll);
	}

	public bool SendCommandToSelected(VisualCommand command)
	{
		return SendCommandToComponents(command, SelectedObjects());
	}
	
	public bool SendCommandToComponents(VisualCommand command, IEnumerable<VisualComponentBase> components)
	{
		bool result = false;

		Update _update = new();

		foreach (var c in components)
		{
			var change = c.ProcessCommand(command);
			if (!change.Consumed) return false;

			//accumulate changes for undo across multiple objects
			if (change.UndoAction != null) _update.Add(change.UndoAction);
			result = true;
		}

		//if any actions were performed, add the to the Undo stack
		if (_update.Count != 0)
		{
			_undoService.Add(_update);
		}

		return result;
	}
	

	private IEnumerable<VisualComponentBase> SelectedObjects()
	{
		foreach (var go in _gameObjects.GetChildren())
		{
			if (go is VisualComponentBase { IsSelected: true } vcb)
			{
				yield return vcb;
			}
		}
	}

	#endregion


	private void MoveToTop()
	{
		var go = GetSelectedObject();
		if (go == null) return;

		var curZ = go.ZOrder;
		var maxZ = GetMaxComponentZ();

		//move everything below the selected object one higher
		foreach (var g in _gameObjects.GetChildren())
		{
			if (g is VisualComponentBase vcb && vcb.ZOrder > curZ)
			{
				vcb.ZOrder--;
			}
		}

		go.ZOrder = maxZ;
		QueueStackingUpdate();
	}

	private void MoveToBottom()
	{
		var go = GetSelectedObject();
		if (go == null) return;

		var curZ = go.ZOrder;

		//move everything below the selected object one higher
		foreach (var g in _gameObjects.GetChildren())
		{
			if (g is VisualComponentBase vcb && vcb.ZOrder < curZ)
			{
				vcb.ZOrder++;
			}
		}

		go.ZOrder = 1;
		QueueStackingUpdate();
	}

	private bool IsObjectMouseSelected()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsMouseSelected: true } p)
			{
				return true;
			}
		}


		return false;
	}

	private bool IsObjectSelected()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsSelected: true } p)
			{
				return true;
			}
		}

		return false;
	}
	
	private bool IsObjectHovered()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsHovered: true } )
			{
				return true;
			}
		}

		return false;
	}

	private VisualComponentBase GetHoveredObject()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsHovered: true } p)
			{
				return p;
			}
		}

		return null;
	}

	private VisualComponentBase GetSelectedObject()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsSelected: true } p)
			{
				return p;
			}
		}

		return null;
	}

	private VisualComponentBase GetMouseSelectedObject()
	{
		foreach (var n in _gameObjects.GetChildren())
		{
			if (n is VisualComponentBase { IsMouseSelected: true } p)
			{
				return p;
			}
		}

		return null;
	}

	[Export] private int _stackingUpdateFrames = 3; //Test hack to avoid issue with stacking not seeing colliders

	private void QueueStackingUpdate()
	{
		_stackingUpdateRequired = _stackingUpdateFrames;
	}

	private int GetMaxComponentZ()
	{
		var max = 0;

		foreach (var c in _gameObjects.GetChildren())
		{
			if (c is VisualComponentBase vcb)
			{
				max = Math.Max(max, vcb.ZOrder);
			}
		}

		return max;
	}

	public virtual void SetMode(SceneMode mode)
	{
		switch (mode)
		{
			case SceneMode.TwoD:
				_2dCamera.Current = true;
				_currentCamera = _2dCamera;
				break;
			case SceneMode.ThreeDFixed:
				_3dCamera.Current = true;
				_currentCamera = _3dCamera;
				break;
			case SceneMode.ThreeDPhysics:
				break;
			case SceneMode.Creator:
				break;
			default:
				throw new ArgumentOutOfRangeException(nameof(mode), mode, null);
		}
	}

	private VisualComponentBase _spawnComponent;


	public void TestFunction()
	{
		//put test function here
	}

	public void UpdateStackingHeights()
	{
		var children = _gameObjects.GetChildren();

		//this dictionary keeps track of objects that are below a certain object. The key is the object id 
		//(in the children array), and the list elements are the object ids of the things that are under it.
		Dictionary<int, List<int>> underneath = new();
		for (int i = 0; i < children.Count; i++)
		{
			var ci = children[i] as VisualComponentBase;

			if (ci == null)
			{
				GD.PrintErr($"{children[i].Name} not VCB");
				continue;
			}

			if (ci.ShapeProfiles.Count == 0) continue;
			
			for (int j = 0; j < children.Count; j++)
			{
				var cj = children[j] as VisualComponentBase;

				if (cj == null)
				{
					GD.PrintErr($"{children[j].Name} not VCB");
					continue;
				}

				if (cj.ZOrder < ci.ZOrder && CheckOverlap(ci, cj)) //lower zOrders are below other items
				{
					//GD.PrintErr($"Area {i} overlaps Area {j}");
					//add to dictionary
					if (underneath.ContainsKey(i))
					{
						underneath[i].Add(j);
					}
					else
					{
						underneath.Add(i, new List<int> { j });
					}
				}
				
			}
		}

		GD.Print("Collision check complete");

		//uncomment the below to get a printout of the Underneath dictionary

		/*
		foreach (var r in underneath)
		{
			string s = String.Empty;
			foreach (var q in r.Value)
			{
				s += $"{q} ";
			}

			GD.PrintErr($"{r.Key} is above {s}");
		}
		*/

		//loop through all the objects and check the dictionary (which is in Z order) and stack
		//The y coordinate is set to the sum of all of the YHeight values below it.
		//We loop through all the children (and not just the UNDERNEATH dictionary entries)
		//in case there's nothing underneath them. The dictionary only contains items with something below
		//them

		for (int i = 0; i < children.Count; i++)
		{
			var ci = children[i] as VisualComponentBase;
			if (ci is null) continue;

			float floor = 0;

			if (underneath.TryGetValue(i, out var elements))
			{
				foreach (var o in elements)
				{
					if (children[o] is VisualComponentBase co) floor += co.YHeight;
				}
			}

			//GD.Print($"New pos for {i}: {floor + (ci.YHeight / 2f)}");
			ci.Position = new Vector3(ci.Position.X, floor + (ci.YHeight / 2f), ci.Position.Z);
		}
	}

	private bool CheckOverlap(VisualComponentBase comp1, VisualComponentBase comp2)
	{
		Transform2D t1 = new Transform2D(comp1.Rotation.Y, new Vector2(comp1.Position.X, comp1.Position.Z));
		Transform2D t2 = new Transform2D(comp2.Rotation.Y, new Vector2(comp2.Position.X, comp2.Position.Z));

		foreach (var s1 in comp1.ShapeProfiles)
		foreach (var s2 in comp2.ShapeProfiles)
		{
			if (s1.Collide(t1, s2, t2)) return true;
		}

		return false;
	}
}
