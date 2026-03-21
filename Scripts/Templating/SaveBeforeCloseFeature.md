# Save Before Close Feature - TemplateCreator

## Overview

Added a confirmation dialog to the TemplateCreator window that prompts users to save their changes before closing. This prevents accidental loss of work.

## Implementation

### Changes Made

1. **New Dialog Field**
   - Added `ConfirmationDialog _saveBeforeCloseDialog` field to store the dialog instance

2. **Dialog Initialization**
   - Created `InitializeSaveBeforeCloseDialog()` method that:
     - Creates a new `ConfirmationDialog` programmatically
     - Sets dialog text: "Do you want to save changes before closing?"
     - Customizes button labels:
       - **"Save and Close"** (OK button)
       - **"Cancel"** (Cancel button)
       - **"Don't Save"** (custom button)
     - Connects signal handlers

3. **Close Handling**
   - Modified `OnClose()` to show the dialog instead of closing immediately
   - Added `OnCloseRequested()` to handle the X button click (Window's CloseRequested signal)

4. **Dialog Response Handlers**
   - `OnSaveAndClose()` - Saves the template and fires the Closed event
   - `OnCancelClose()` - Closes the dialog without closing the window
   - `OnCustomAction(StringName action)` - Handles the "Don't Save" button to close without saving

## User Experience

### Three Options for Closing

1. **Save and Close**
   - Saves the current template to the project
   - Closes the TemplateCreator window
   - Triggers the `Closed` event

2. **Don't Save**
   - Closes the TemplateCreator window without saving
   - Any unsaved changes are lost
   - Triggers the `Closed` event

3. **Cancel**
   - Closes the dialog
   - Keeps the TemplateCreator window open
   - User can continue editing

### Trigger Points

The save confirmation dialog appears when:
- User clicks the "Close" button in the toolbar
- User clicks the X button on the window frame

## Code Structure

### Dialog Creation

```csharp
private void InitializeSaveBeforeCloseDialog()
{
    _saveBeforeCloseDialog = new ConfirmationDialog();
    _saveBeforeCloseDialog.DialogText = "Do you want to save changes before closing?";
    _saveBeforeCloseDialog.Title = "Save Changes";
    _saveBeforeCloseDialog.OkButtonText = "Save and Close";
    _saveBeforeCloseDialog.CancelButtonText = "Cancel";
    
    // Add a "Don't Save" button
    _saveBeforeCloseDialog.AddButton("Don't Save", false, "dont_save");
    
    // Connect signals
    _saveBeforeCloseDialog.Confirmed += OnSaveAndClose;
    _saveBeforeCloseDialog.Canceled += OnCancelClose;
    _saveBeforeCloseDialog.CustomAction += OnCustomAction;
    
    AddChild(_saveBeforeCloseDialog);
}
```

### Signal Handlers

```csharp
private void OnSaveAndClose()
{
    SaveTemplate();
    Closed?.Invoke(this, EventArgs.Empty);
}

private void OnCancelClose()
{
    // Do nothing - just close the dialog, keep the window open
}

private void OnCustomAction(StringName action)
{
    if (action == "dont_save")
    {
        // Close without saving
        Closed?.Invoke(this, EventArgs.Empty);
    }
}
```

### Close Request Handling

```csharp
private void OnClose()
{
    // Show save confirmation dialog
    _saveBeforeCloseDialog.PopupCentered();
}

private void OnCloseRequested()
{
    // When user clicks X button, show save confirmation instead of closing immediately
    _saveBeforeCloseDialog.PopupCentered();
}
```

## Godot Signals Used

| Signal | Source | Description |
|--------|--------|-------------|
| `Confirmed` | ConfirmationDialog | Emitted when OK button ("Save and Close") is pressed |
| `Canceled` | ConfirmationDialog | Emitted when Cancel button is pressed |
| `CustomAction` | AcceptDialog | Emitted when custom button ("Don't Save") is pressed |
| `CloseRequested` | Window | Emitted when X button on window frame is clicked |

## Button Flow

```
User clicks Close or X
        ↓
Show Dialog: "Do you want to save changes before closing?"
        ↓
    ┌───────┬───────────┬─────────┐
    │       │           │         │
Save and   Don't     Cancel
  Close     Save
    │       │           │
    ↓       ↓           ↓
SaveTemplate() Closed  (Nothing)
    ↓       event       
Closed event
```

## Testing Checklist

- [ ] Click "Close" button - dialog appears
- [ ] Click X button - dialog appears
- [ ] Click "Save and Close" - template saves and window closes
- [ ] Click "Don't Save" - window closes without saving
- [ ] Click "Cancel" - dialog closes, window stays open
- [ ] Press Escape while dialog is open - dialog closes (Cancel behavior)
- [ ] Make changes and close - verify "Don't Save" doesn't save changes
- [ ] Make changes and close - verify "Save and Close" does save changes

## Future Enhancements

Potential improvements:
1. **Track Dirty State**
   - Only show dialog if there are unsaved changes
   - Set a `_hasUnsavedChanges` flag when template is modified
   - Skip dialog if no changes were made

2. **Auto-save**
   - Add option to automatically save on close
   - User preference setting

3. **Save As Option**
   - Add "Save As..." button to save with a new name

4. **Comparison**
   - Show what changed since last save
   - Allow user to review changes before deciding

## Build Status

✅ **Build successful** - All code compiles without errors

## Files Modified

- `Scripts/Templating/TemplateCreator.cs`
  - Added `_saveBeforeCloseDialog` field
  - Added `InitializeSaveBeforeCloseDialog()` method
  - Added `OnCloseRequested()` method
  - Added `OnSaveAndClose()` method
  - Added `OnCancelClose()` method
  - Added `OnCustomAction()` method
  - Modified `OnClose()` to show dialog
  - Connected `CloseRequested` signal in `_Ready()`

## Related Code

The save dialog integrates with existing functionality:
- Uses existing `SaveTemplate()` method for saving
- Uses existing `Closed` event to notify parent components
- Follows existing dialog pattern (similar to `_acceptDialog` for delete confirmation)

---

**Note:** This is a standard UX pattern for document editors and prevents users from accidentally losing their work when closing the window.
