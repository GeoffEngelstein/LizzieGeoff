using Godot;
using System;

/// <summary>
/// This class captures all the properties that need to be synced across the network for a visual component. This is used to ensure that all clients have the same state for each component, and to minimize the amount of data that needs to be sent over the network by only syncing relevant properties.
/// </summary>
public class VcSyncDto
{
    public Vector3 Position { get; set; }
    public Vector3 Rotation { get; set; }
    public bool Visible { get; set; }

    public bool Deleted { get; set; }

    public int ZOrder { get; set; }

    public string DataSetRow { get; set; }
    public VisualComponentBase.ComponentLocation Location { get; set; }

    public VisualComponentBase.LayerType Layer { get; set; }

    public Guid[] ContainedComponents { get; set; }
}
