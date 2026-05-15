using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Godot;

[Command(VisualCommand.Flip)]
public class FlipCommand : BasicCommand
{
    public FlipCommand()
    {
        Caption = "Flip";
        Command = VisualCommand.Flip;
    }
}
