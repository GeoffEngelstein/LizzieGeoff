using Godot;
using System;

public class MenuCommand
{
   public MenuCommand(SceneController.VisualCommand command, bool isChecked = false, bool isEnabled = true)
   {
      Command = command;
      IsChecked = isChecked;
      IsEnabled = isEnabled;
   }
   
   public SceneController.VisualCommand Command { get; set; }
   public bool IsChecked { get; set; }
   public bool IsEnabled { get; set; }
}
