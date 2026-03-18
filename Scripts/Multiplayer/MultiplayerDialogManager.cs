using Godot;

/// <summary>
/// Manager for spawning and controlling the multiplayer dialog
/// Add this as a child of your main UI scene
/// </summary>
public partial class MultiplayerDialogManager : Node
{
    private MultiplayerDialog _dialog;

    public override void _Ready()
    {
        // You can bind this to a menu item or button press
    }

    /// <summary>
    /// Show the multiplayer dialog
    /// </summary>
    public void ShowDialog()
    {
        if (_dialog == null)
        {
            var d = GD.Load<PackedScene>("res://Scenes/Controls/multiplayer_connect.tscn");
            _dialog = d.Instantiate<MultiplayerDialog>();
            _dialog.CloseRequested += OnDialogClosed;
            AddChild(_dialog);
        }

        _dialog.PopupCentered();
    }

    /// <summary>
    /// Hide the multiplayer dialog
    /// </summary>
    public void HideDialog()
    {
        _dialog?.Hide();
    }

    private void OnDialogClosed()
    {
        _dialog?.QueueFree();
        _dialog = null;
    }

    public override void _Input(InputEvent @event)
    {
        // Optional: Bind to a key for quick access (e.g., F8)
        if (@event is InputEventKey keyEvent && keyEvent.Pressed && keyEvent.Keycode == Key.F8)
        {
            if (_dialog == null || !_dialog.Visible)
            {
                ShowDialog();
            }
            else
            {
                HideDialog();
            }
            GetViewport().SetInputAsHandled();
        }
    }
}
