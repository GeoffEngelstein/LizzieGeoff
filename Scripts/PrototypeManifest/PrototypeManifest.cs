using Godot;
using System;
using System.Linq;

public partial class PrototypeManifest : Control
{
    private SplitContainer _mainContainer;
    private Tree _prototypeTree;
    private TreeItem _root;

    private ComponentPreview _preview;

    private Prototype _selectedPrototype;
    public Prototype SelectedPrototype
    {
        get => _selectedPrototype;
        private set
        {
            _selectedPrototype = value;
            OnPrototypeSelected();
        }
    }

    private int _sortColumn = 0;
    private bool _sortAscending = true;

    public override void _Ready()
    {
        _mainContainer = GetNode<SplitContainer>("%MainContainer");
        _preview = GetNode<ComponentPreview>("%ComponentPreview");
        _preview.SetComponentX(-200);
        InitializePrototypeGrid();
        LoadPrototypes();
    }

    private void InitializePrototypeGrid()
    {
        _prototypeTree = new Tree();
        _prototypeTree.Columns = 2;
        _prototypeTree.HideRoot = true;
        _prototypeTree.SelectMode = Tree.SelectModeEnum.Row;
        _prototypeTree.SizeFlagsVertical = SizeFlags.ExpandFill;
        _prototypeTree.SizeFlagsHorizontal = SizeFlags.ExpandFill;

        _prototypeTree.SetColumnTitle(0, "Name");
        _prototypeTree.SetColumnTitle(1, "Type");
        _prototypeTree.SetColumnExpand(0, true);
        _prototypeTree.SetColumnExpand(1, true);
        _prototypeTree.SetColumnExpandRatio(0, 2);
        _prototypeTree.SetColumnExpandRatio(1, 1);

        _prototypeTree.ColumnTitleClicked += OnColumnHeaderClicked;
        _prototypeTree.ItemSelected += OnTreeItemSelected;

        if (_mainContainer != null)
        {
            _mainContainer.AddChild(_prototypeTree);
            _mainContainer.MoveChild(_prototypeTree, 0);
        }
        else
        {
            AddChild(_prototypeTree);
            MoveChild(_prototypeTree, 0);
        }

        _root = _prototypeTree.CreateItem();
    }


    public void Refresh()
    {
        _preview.ClearComponent();
        _preview.SetComponentVisibility(false);
        LoadPrototypes();
    }
    private void LoadPrototypes()
    {
        if (ProjectService.Instance?.CurrentProject == null)
            return;

        _prototypeTree.Clear();
        _root = _prototypeTree.CreateItem();

        var prototypes = ProjectService.Instance.CurrentProject.Prototypes.Values.ToList();

        if (_sortColumn == 0)
        {
            prototypes = _sortAscending
                ? prototypes.OrderBy(p => p.Name).ToList()
                : prototypes.OrderByDescending(p => p.Name).ToList();
        }
        else if (_sortColumn == 1)
        {
            prototypes = _sortAscending
                ? prototypes.OrderBy(p => p.Type.ToString()).ToList()
                : prototypes.OrderByDescending(p => p.Type.ToString()).ToList();
        }

        foreach (var prototype in prototypes)
        {
            var item = _prototypeTree.CreateItem(_root);
            item.SetText(0, prototype.Name ?? "");
            item.SetText(1, prototype.Type.ToString());
            item.SetMetadata(0, prototype.PrototypeRef.ToString());
        }
    }

    private void OnColumnHeaderClicked(long column, long mouseButtonIndex)
    {
        if (mouseButtonIndex != (long)MouseButton.Left)
            return;

        if (_sortColumn == column)
        {
            _sortAscending = !_sortAscending;
        }
        else
        {
            _sortColumn = (int)column;
            _sortAscending = true;
        }

        LoadPrototypes();
    }

    private void OnTreeItemSelected()
    {
        var selectedItem = _prototypeTree.GetSelected();
        if (selectedItem == null)
            return;

        var prototypeRef = Guid.Parse(selectedItem.GetMetadata(0).AsString());

        if (ProjectService.Instance?.CurrentProject?.Prototypes.TryGetValue(prototypeRef, out var prototype) == true)
        {
            SelectedPrototype = prototype;
        }
    }

    private void OnPrototypeSelected()
    {
        //update the preview
        _preview.ClearComponent();
        _preview.Build(SelectedPrototype, TextureFactory);
    }

    public TextureFactory TextureFactory { get; set; }

    public void RefreshGrid()
    {
        LoadPrototypes();
    }
}
