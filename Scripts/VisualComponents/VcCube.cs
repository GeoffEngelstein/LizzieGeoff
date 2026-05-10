using System;
using System.Collections.Generic;
using Godot;

public partial class VcCube : VisualComponentBase
{
    public override void _Ready()
    {
        base._Ready();
        ComponentType = VisualComponentType.Cube;

        MainMesh = GetNode<GeometryInstance3D>("ObjectMesh");
        HighlightMesh = GetNode<MeshInstance3D>("HighlightMesh");
    }

    public override bool Setup(
        Dictionary<string, object> parameters,
        string dataSetRow,
        TextureFactory textureFactory
    )
    {
        return Setup(parameters, textureFactory);
    }

    public override bool Setup(Dictionary<string, object> parameters, TextureFactory textureFactory)
    {
        base.Setup(parameters, string.Empty, textureFactory);

        MainMesh = GetNode<GeometryInstance3D>("ObjectMesh");
        HighlightMesh = GetNode<MeshInstance3D>("HighlightMesh");

        if (parameters.ContainsKey(nameof(Height)))
        {
            var h = Utility.GetParam<float>(parameters, "Height");
            if (h <= 0)
                return false;
            Height = h / 10f;

            var w = Utility.GetParam<float>(parameters, "Width");
            Width = w / 10f;

            var l = Utility.GetParam<float>(parameters, "Length");
            Length = l / 10f;

            if (parameters["Color"] is Color color)
            {
                CubeColor = color;
            }
        }

        //create cube
        if (Width <= 0 || Length <= 0)
        {
            Scale = new Vector3(Height, Height, Height);
        }
        else
        {
            Scale = new Vector3(Width, Height, Length);
        }

        YHeight = Height;

        SetColor(CubeColor);

        var r = new RectangleShape2D();
        r.Size = new Vector2(Width, Length);

        ShapeProfiles.Add(new OffsetShape2D(r));

        return true;
    }

    public override List<string> ValidateParameters(Dictionary<string, object> parameters)
    {
        var ret = new List<string>();

        //must have a name and height. Width/length optional
        if (parameters.ContainsKey(nameof(ComponentName)))
        {
            if (string.IsNullOrEmpty(parameters[nameof(ComponentName)].ToString()))
                ret.Add("Instance Name may not be blank");
        }
        else
        {
            ret.Add("Instance Name not included");
        }

        if (parameters.ContainsKey(nameof(Height)))
        {
            var h = Utility.GetParam<float>(parameters, "Height");
            if (h <= 0)
                ret.Add("Height must be > 0");
        }
        else
        {
            ret.Add("Height not included");
        }

        return ret;
    }

    public override GeometryInstance3D DragMesh => MainMesh;

    public override float MaxAxisSize => Math.Max(Math.Max(Height, Width), Length);

    private float Height;
    private float Width;
    private float Length;
    private Color CubeColor;
}
