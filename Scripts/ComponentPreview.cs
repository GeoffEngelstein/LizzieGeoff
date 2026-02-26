using Godot;
using System;
using System.Collections.Generic;

public partial class ComponentPreview : Panel
{
	private Node3D _parentNode;
	private Label _previewLabel;
	

	private Button _spinButton;
	private Button _frontView;
	private Button _backView;

	private SubViewportContainer _subViewportContainer;
	private SubViewport _subViewport;

    private PageControl _pageControl;
	
	public override void _Ready()
	{
		_parentNode = GetNode<Node3D>("SubViewportContainer/SubViewport/Node3D");
		_previewLabel = GetNode<Label>("PreviewLabel");
		_pageControl = GetNode<PageControl>("PageControl");
		_pageControl.ItemSelected += ChangePage;
		_pageControl.Visible = false;
		_pageControl.SetItemCount(ItemCount);
		
		_spinButton = GetNode<Button>("%SpinButton");
		
		_frontView = GetNode<Button>("%FrontView");
		_frontView.Pressed += () => ShowView(0);
		
		_backView = GetNode<Button>("%BackView");
		_backView.Pressed += () => ShowView(180);

        _subViewportContainer = GetNode<SubViewportContainer>("%SubViewportContainer");
        _subViewport = GetNode<SubViewport>("%SubViewport");
        _subViewport.Size = new Vector2I((int)Size.X, (int)_subViewportContainer.Size.Y);
    }

	public override void _Process(double delta)
	{
		if (_component != null && _spinButton.ButtonPressed)
		{
			_component.Rotation += new Vector3(0,(float)delta, 0);
		}

		if (_buildNeeded && _component != null && _component.IsNodeReady())
		{
			Build(_component.Parameters, _textureFactory);
			_buildNeeded = false;
		}
	}

	private TextureFactory _textureFactory;
	
	private VisualComponentBase _component;
    private bool _buildNeeded;

	private bool _componentActive;
	
	public void SetComponent(VisualComponentBase component, Vector3 rotation)
	{


        if (_componentActive)
		{
			ClearComponent();
		}

		_component = component;
		_componentActive = true;
		_component.Rotation = rotation;
		_parentNode.AddChild(_component);
	}

	public void ClearComponent()
	{
		if (_component == null) return;
		_component.QueueFree();
		_component = null;
		_componentActive = false;
	}

	public void SetComponentVisibility(bool visibility)
	{
		if (_component == null) return;
		_component.Visible = visibility;
	}

    public void SetComponentX(float x)
    {
        var c = _subViewport.GetChildren();

        var a = c?[0] as Node3D;
        if (a == null) return;	
		
		a.Position = new Vector3(x, a.Position.Y, a.Position.Z);
    }

	public void Build(Dictionary<string, object> parameters, TextureFactory textureFactory)
	{
		if (_component != null)
		{
			_component.Build(parameters, textureFactory);
		}
	}

    public void Build(Prototype prototype, TextureFactory textureFactory)
    {
        Build(prototype.Type, prototype.Parameters, textureFactory);
    }

	public void Build(VisualComponentBase.VisualComponentType componentType, Dictionary<string, object> parameters, TextureFactory textureFactory)
	{
		var c= SpawnComponent(componentType, parameters);
        _buildNeeded = true;

        SetComponent(c, GetRotationVector(componentType));
		_textureFactory = textureFactory;
    }

	private VisualComponentBase SpawnComponent(VisualComponentBase.VisualComponentType componentType, Dictionary<string, object> parameters)
	{
		var s = Utility.ComponentTypeToScenePath(componentType, parameters);
        var scene = GD.Load<PackedScene>(s);
        var c = scene.Instantiate<VisualComponentBase>();
		c.Parameters = parameters;
		return c;
    }

    private Vector3 GetRotationVector(VisualComponentBase.VisualComponentType componentType)
    {
        Vector3 v = componentType switch
        {
            VisualComponentBase.VisualComponentType.Cube => new Vector3(Mathf.DegToRad(-10), 0, 0),
            VisualComponentBase.VisualComponentType.Disc => new Vector3(Mathf.DegToRad(-10), 0, 0),
            VisualComponentBase.VisualComponentType.Tile => new Vector3(Mathf.DegToRad(90), 0, 0),
            VisualComponentBase.VisualComponentType.Token => new Vector3(Mathf.DegToRad(90), 0, 0),
            VisualComponentBase.VisualComponentType.Board => new Vector3(Mathf.DegToRad(90), 0, 0),
            VisualComponentBase.VisualComponentType.Card => new Vector3(Mathf.DegToRad(90), 0, 0),
            VisualComponentBase.VisualComponentType.Deck => new Vector3(Mathf.DegToRad(90), 0, 0),
            VisualComponentBase.VisualComponentType.Die => new Vector3(Mathf.DegToRad(-45), 0, 0),
            VisualComponentBase.VisualComponentType.Mesh => new Vector3(Mathf.DegToRad(-10), 0, 0),
            VisualComponentBase.VisualComponentType.Meeple => new Vector3(Mathf.DegToRad(-10), 0, 0),
            _ => Vector3.Zero
        };
        return v;
    }

    #region Multi-preview mode

    private bool _multiItemMode;

	public bool MultiItemMode
	{
		get => _multiItemMode;
		set
		{
			_multiItemMode = value;
			_previewLabel.Visible = !value;
			_pageControl.Visible = value;
		}
	}

	private void ChangePage(object sender, ItemSelectedEventArgs e)
	{
		CurrentItem = _pageControl.GetCurrentItem();
		ItemSelected?.Invoke(this, new ItemSelectedEventArgs(CurrentItem) );
	}
	

	private int _itemCount = 1;

	public int ItemCount
	{
		get => _itemCount;
		set
		{
			if (value < 1)
			{
				_itemCount = 1;
			}
			else
			{
				_itemCount = value;
			}
			_pageControl.SetItemCount(_itemCount);
		}
	}

	public void SetItemLabels(IList<string> labels)
	{
		_pageControl.SetItemLabels(labels);
	}
	
	public int CurrentItem { get; set; }
	
	public event EventHandler<ItemSelectedEventArgs> ItemSelected;
	
	#endregion

	#region Viewing controls

	private void ShowView(float angle)
	{
		_spinButton.ButtonPressed = false;	//stop spinning

		var r = new Vector3(_component.Rotation.X, Mathf.DegToRad(angle), _component.Rotation.Z);
		_component.Rotation = r;
	}
	
	
	#endregion
	
}

public class ItemSelectedEventArgs : EventArgs
{
	public ItemSelectedEventArgs(int index)
	{
		Index = index;
	}

	public ItemSelectedEventArgs(int index, string caption)
	{
		Index = index;
		Caption = caption;
	}
	public int Index { get; set; }
	public string Caption { get; set; }
}

