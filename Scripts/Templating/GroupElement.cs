using Godot;
using System;
using System.Collections.Generic;
using Lizzie.Scripts.Templating;

public class GroupElement : TemplateElement
{

	public GroupElement()
	{
		ElementType = ITemplateElement.TemplateElementType.Container;

		Initialize();	
	}

	private void Initialize()
	{
		SetParameterValue("Width", "100");
		SetParameterValue("Height", "100");
		SetParameterValue("X", "70");
		SetParameterValue("Y", "70");
	}

	
}
