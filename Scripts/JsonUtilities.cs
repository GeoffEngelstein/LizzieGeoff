using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using Godot;

public static class JsonUtilities
{
    public static Dictionary<string, object> ParseJsonToDictionary(
        VisualComponentBase.VisualComponentType vcType,
        Dictionary<string, object> d
    )
    {
        switch (vcType)
        {
            case VisualComponentBase.VisualComponentType.Cube:
                return ParseCube(d);

            case VisualComponentBase.VisualComponentType.Disc:
                return ParseDisc(d);

            case VisualComponentBase.VisualComponentType.Tile:
                break;
            case VisualComponentBase.VisualComponentType.Token:
                return ParseToken(d);

            case VisualComponentBase.VisualComponentType.Board:
                break;
            case VisualComponentBase.VisualComponentType.Card:
                break;
            case VisualComponentBase.VisualComponentType.Deck:
                break;
            case VisualComponentBase.VisualComponentType.Die:
                return ParseDie(d);

            case VisualComponentBase.VisualComponentType.Mesh:
                break;
            case VisualComponentBase.VisualComponentType.Meeple:
                return ParseMeeple(d);

            default:
                throw new ArgumentOutOfRangeException(nameof(vcType), vcType, null);
        }

        return d; //for now
    }

    private static Dictionary<string, object> ParseCube(Dictionary<string, object> d)
    {
        var p = new Dictionary<string, object>();

        p.Add("ComponentName", TryGetString(d, "ComponentName"));
        p.Add("BaseName", TryGetString(d, "BaseName"));
        p.Add("Height", TryGetFloat(d, "Height"));
        p.Add("Width", TryGetFloat(d, "Width"));
        p.Add("Length", TryGetFloat(d, "Length"));
        p.Add("Color", TryGetColor(d, "Color"));

        return p;
    }

    private static Dictionary<string, object> ParseDisc(Dictionary<string, object> d)
    {
        var p = new Dictionary<string, object>();

        p.Add("ComponentName", TryGetString(d, "ComponentName"));
        p.Add("BaseName", TryGetString(d, "BaseName"));
        p.Add("Height", TryGetFloat(d, "Height"));
        p.Add("Diameter", TryGetFloat(d, "Diameter"));
        p.Add("Color", TryGetColor(d, "Color"));

        return p;
    }

    private static Dictionary<string, object> ParseToken(Dictionary<string, object> d)
    {
        var p = new Dictionary<string, object>();

        p.Add("ComponentName", TryGetString(d, "ComponentName"));
        p.Add("BaseName", TryGetString(d, "BaseName"));
        p.Add("Height", TryGetFloat(d, "Height"));
        p.Add("Width", TryGetFloat(d, "Width"));
        p.Add("Thickness", TryGetFloat(d, "Thickness"));

        p.Add("FrontImage", TryGetString(d, "FrontImage"));
        p.Add("BackImage", TryGetString(d, "BackImage"));

        p.Add("Shape", TryGetInt(d, "Shape"));
        p.Add("Mode", (VcToken.TokenBuildMode)TryGetInt(d, "Mode"));

        p.Add("FrontBgColor", TryGetColor(d, "FrontBgColor"));
        p.Add("BackBgColor", TryGetColor(d, "BackBgColor"));

        p.Add("QuickFront", TryGetQTF(d, "QuickFront"));
        p.Add("QuickBack", TryGetQTF(d, "QuickBack"));

        p.Add("Type", TryGetInt(d, "Type"));
        p.Add("FrontFontSize", TryGetInt(d, "FrontFontSize"));
        p.Add("BackFontSize", TryGetInt(d, "BackFontSize"));

        p.Add("DifferentBack", TryGetBool(d, "DifferentBack"));
        return p;
    }

    private static Dictionary<string, object> ParseDie(Dictionary<string, object> d)
    {
        var p = new Dictionary<string, object>();

        p.Add("ComponentName", TryGetString(d, "ComponentName"));
        p.Add("BaseName", TryGetString(d, "BaseName"));
        p.Add("Size", TryGetFloat(d, "Size"));
        p.Add("Color", TryGetColor(d, "Color"));

        p.Add("Sides", TryGetQTFArray(d, "Sides"));

        return p;
    }

    public static Dictionary<string, object> ParseMeeple(Dictionary<string, object> d)
    {
        var p = new Dictionary<string, object>();
        p.Add("ComponentName", TryGetString(d, "ComponentName"));
        p.Add("BaseName", TryGetString(d, "BaseName"));
        p.Add("Height", TryGetFloat(d, "Height"));
        p.Add("Thickness", TryGetFloat(d, "Thickness"));
        p.Add("Color", TryGetColor(d, "Color"));
        p.Add("Grid", TryGetBoolJaggedArray(d, "Grid"));
        return p;
    }
    
    private static string TryGetString(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value) && value != null)
        {
            return value.ToString();
        }
        return string.Empty;
    }

    private static float TryGetFloat(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value))
        {
            if (value is float f)
                return f;
            if (value is double db)
                return (float)db;
            if (value is int i)
                return i;
            if (value is long l)
                return l;
            if (float.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return 0;
    }

    private static int TryGetInt(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value))
        {
            if (int.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return 0;
    }

    private static bool TryGetBool(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value))
        {
            if (bool.TryParse(value.ToString(), out var parsed))
                return parsed;
        }
        return false;
    }

    private static Color TryGetColor(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value) && value != null)
        {
            return JsonSerializer.Deserialize<Color>(value.ToString());
        }

        return Colors.Black; // Default color if not found or deserialization fails
    }

    private static QuickTextureField TryGetQTF(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value) && value != null)
        {
            return JsonSerializer.Deserialize<QuickTextureField>(value.ToString());
        }

        return new QuickTextureField(); // Default if not found or deserialization fails
    }

    private static QuickTextureField[] TryGetQTFArray(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value) && value != null)
        {
            object[] array = JsonSerializer.Deserialize<object[]>(value.ToString());

            var r = new QuickTextureField[array.Length];
            int index = 0;
            foreach (var item in array)
            {
                var qtf = JsonSerializer.Deserialize<QuickTextureField>(item.ToString());
                r[index] = qtf;
                index++;
            }

            return r;
        }
        return []; // Default if not found or deserialization fails
    }

    private static bool[][] TryGetBoolJaggedArray(Dictionary<string, object> d, string key)
    {
        if (d.TryGetValue(key, out var value) && value != null)
        {
            // If it's already a jagged array, return it
            if (value is bool[][] jaggedArray)
            {
                return jaggedArray;
            }

            // Try to deserialize from JSON
            try
            {
                // First deserialize as JsonElement to check structure
                var json = value.ToString();
                using var doc = JsonDocument.Parse(json);
                var root = doc.RootElement;

                if (root.ValueKind == JsonValueKind.Array)
                {
                    var outerArray = root.EnumerateArray().ToArray();
                    var result = new bool[outerArray.Length][];

                    for (int i = 0; i < outerArray.Length; i++)
                    {
                        if (outerArray[i].ValueKind == JsonValueKind.Array)
                        {
                            var innerArray = outerArray[i].EnumerateArray().ToArray();
                            result[i] = new bool[innerArray.Length];

                            for (int j = 0; j < innerArray.Length; j++)
                            {
                                result[i][j] = innerArray[j].GetBoolean();
                            }
                        }
                    }

                    return result;
                }
            }
            catch (Exception ex)
            {
                GD.PrintErr($"Error deserializing jagged bool array: {ex.Message}");
            }
        }

        return new bool[0][]; // Default empty jagged array if not found or deserialization fails
    }

}
