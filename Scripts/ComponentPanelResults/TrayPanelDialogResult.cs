using Godot;
using System;
using System.Collections.Generic;

public partial class TrayPanelDialogResult : ComponentPanelDialogResult
{
    private LineEdit _nameInput;
    private LineEdit _heightInput;
    private LineEdit _widthInput;
    private LineEdit _lengthInput;
    private ColorPickerButton _colorPicker;
    private ComponentPreview _preview;
    private OptionButton _prototypeList;
    private string _selectedPrototypeKey;

    public override void _Ready()
    {
        ComponentType = VisualComponentBase.VisualComponentType.Cube;
        _nameInput = GetNode<LineEdit>("%ItemName");
        _heightInput = GetNode<LineEdit>("%Height");
        _heightInput.TextChanged += t => UpdatePreview();

        _lengthInput = GetNode<LineEdit>("%Length");
        _lengthInput.TextChanged += t => UpdatePreview();

        _widthInput = GetNode<LineEdit>("%Width");
        _widthInput.TextChanged += t => UpdatePreview();

        _colorPicker = GetNode<ColorPickerButton>("%Color");
        _colorPicker.ColorChanged += ColorPickerOnColorChanged;

        _prototypeList = GetNode<OptionButton>("%PrototypeList");
        LoadPrototypeList();
        _prototypeList.ItemSelected += PrototypeSelected;

        _preview = GetNode<ComponentPreview>("%Preview");
    }

    private void LoadPrototypeList()
    {
        _prototypeList.Clear();
        _prototypeList.AddItem("(none)", 0);

        int i = 1;

        foreach (var p in ProjectService.Instance.CurrentProject.Prototypes)
        {
            _prototypeList.AddItem(p.Value.Name, i);
            _prototypeList.SetItemMetadata(i, p.Key.ToString());
            i++;
        }
    }

    private void PrototypeSelected(long index)
    {
        _selectedPrototypeKey = _prototypeList.GetItemMetadata((int)index).ToString();
        UpdatePreview();
    }

    private void ColorPickerOnColorChanged(Color color)
    {
        UpdatePreview();
    }

    private bool _subviewportInitComplete;
    private int _subViewportFrames = 3;

    public override void Activate()
    {
        _preview.SetComponent(GetPreviewComponent(), new Vector3(Mathf.DegToRad(30), 0, 0));
        UpdatePreview();
    }

    private VcTray GetPreviewComponent()
    {
        var scene = GD.Load<PackedScene>("res://Scenes/VisualComponents/VcTray.tscn");
        return scene.Instantiate<VcTray>();
    }

    public override void Deactivate()
    {
        _preview.ClearComponent();
    }

    public override List<string> Validity()
    {
        var ret = new List<string>();

        if (string.IsNullOrEmpty(_nameInput.Text.Trim()))
        {
            ret.Add("Component Name required");
        }

        return ret;
    }

    public override Dictionary<string, object> GetParams()
    {
        var d = new Dictionary<string, object>();

        d.Add("ComponentName", _nameInput.Text);
        d.Add("Height", ParamToFloat(_heightInput.Text));
        d.Add("Width", ParamToFloat(_widthInput.Text));
        d.Add("Length", ParamToFloat(_lengthInput.Text));
        d.Add("Color", _colorPicker.Color);
        d.Add("Prototype", _selectedPrototypeKey);
        return d;
    }

    private void UpdatePreview()
    {
        var d = new Dictionary<string, object>();

        //normalize the size
        var h = ParamToFloat(_heightInput.Text);
        var w = ParamToFloat(_widthInput.Text);
        var l = ParamToFloat(_lengthInput.Text);

        if (h == 0 || w == 0 || l == 0)
        {
            _preview.SetComponentVisibility(false);
            return;
        }

        _preview.SetComponentVisibility(true);

        //normalize dimensions to 10x10x10 outer extants
        //var scale = 10f / Math.Max(h, Math.Max(w, l));
        var scale = 1;

        d.Add("ComponentName", _nameInput.Text);
        d.Add("Height", h * scale);
        d.Add("Width", w * scale);
        d.Add("Length", l * scale);
        d.Add("Color", _colorPicker.Color);
        d.Add("Prototype", _selectedPrototypeKey);
        _preview.Build(d, TextureFactory);
    }

    public override void DisplayPrototype(Guid prototypeId)
    {
        var prototype = ProjectService.Instance.CurrentProject.Prototypes[prototypeId];
        DisplayPrototype(prototype);
    }

    public override void DisplayPrototype(Prototype prototype)
    {
        _nameInput.Text = prototype.Name;
        _heightInput.Text = prototype.Parameters.ContainsKey("Height")
            ? prototype.Parameters["Height"].ToString()
            : "";
        _widthInput.Text = prototype.Parameters.ContainsKey("Width")
            ? prototype.Parameters["Width"].ToString()
            : "";
        _lengthInput.Text = prototype.Parameters.ContainsKey("Length")
            ? prototype.Parameters["Length"].ToString()
            : "";
        _colorPicker.Color = prototype.Parameters.ContainsKey("Color")
            ? (Color)prototype.Parameters["Color"]
            : Colors.Red;

        Activate();
    }
}
