using Godot;
using System;
using System.Collections.Generic;

public partial class TokenPanelDialogResult : ComponentPanelDialogResult
{
	private LineEdit _nameInput;
		private LineEdit _heightInput;
		private LineEdit _widthInput;
		private LineEdit _thicknessInput;
		
		private LineEdit _frontImage;
		private LineEdit _backImage;
	
		private Button _frontButton;
		private Button _backButton;

		private ColorPickerButton _quickBackgroundColor;
		private ColorPickerButton _quickTextColor;
		private LineEdit _quickText;



		//quick method back of token
		private ColorPickerButton _quickBackgroundColor2;
		private ColorPickerButton _quickTextColor2;
		private LineEdit _quickText2;

		private TokenTextureSubViewport _previewTop;
		private TokenTextureSubViewport _previewBottom;
	
		
		private CheckBox _quickBackCheckbox;

		private OptionButton _shapePicker;

		private TabContainer _tabs;
		
		public override void _Ready()
		{
			ComponentType = VisualComponentBase.VisualComponentType.Token;
			
			
			_nameInput = GetNode<LineEdit>("%ItemName");
			_heightInput = GetNode<LineEdit>("%Height");
			_widthInput = GetNode<LineEdit>("%Width");
			_thicknessInput = GetNode<LineEdit>("%Thickness");
	
			_frontImage = GetNode<LineEdit>("%FrontFile");
			_backImage = GetNode<LineEdit>("%BackFile");
			
			_frontButton = GetNode<Button>("%FrontFileButton");
			_frontButton.Pressed += GetFrontFile;
			_backButton = GetNode<Button>("%BackFileButton");
			_backButton.Pressed += GetBackFile;

			_quickBackgroundColor = GetNode<ColorPickerButton>("%TopBgColor");
			_quickText =
				GetNode<LineEdit>("%TopCaption");
			_quickTextColor = GetNode<ColorPickerButton>("%TopTextColor");
			_quickBackCheckbox =
				GetNode<CheckBox>("%ToggleBack");
			
			_quickText.TextChanged += OnTextChange;
			_quickBackgroundColor.ColorChanged += OnBackgroundColorChanged;
			_quickTextColor.ColorChanged += OnPreviewTextColorChange;
			_quickBackCheckbox.Pressed += OnQuickBackCheckboxChange;
			
			_quickBackgroundColor2 = GetNode<ColorPickerButton>("%BottomBgColor");
			_quickText2 =
				GetNode<LineEdit>("%BottomCaption");
			_quickTextColor2 = GetNode<ColorPickerButton>("%BottomTextColor");
			
			_quickText2.TextChanged += OnText2Change;
			_quickBackgroundColor2.ColorChanged += OnBackgroundColor2Changed;
			_quickTextColor2.ColorChanged += OnPreviewTextColor2Change;

			_shapePicker = GetNode<OptionButton>("%ShapePicker");
			_shapePicker.ItemSelected += ShapePickerOnItemSelected;
			
			_previewTop =
				GetNode<TokenTextureSubViewport>("%TopPreview");	
			
			_previewBottom =
				GetNode<TokenTextureSubViewport>("%BottomPreview");

			_tabs = GetNode<TabContainer>("%Tabs");
		
			OnQuickBackCheckboxChange();	//just to set the initial line visibility in case someone messed with the control.
			ShapePickerOnItemSelected(0);
		}

		private void ShapePickerOnItemSelected(long index)
		{
			TokenTextureSubViewport.TokenShape shape = TokenTextureSubViewport.TokenShape.Square;
			
			switch (index)
			{
				case 0:
					shape = TokenTextureSubViewport.TokenShape.Square;
					break;
					
				case 1:
					shape = TokenTextureSubViewport.TokenShape.Circle;
					break;
				case 2:
					shape = TokenTextureSubViewport.TokenShape.HexPoint;
					break;
				case 3:
					shape = TokenTextureSubViewport.TokenShape.HexFlat;
					break;
			}

			PrototypeIndex = (int)index;
			_previewTop.SetShape(shape);
			_previewBottom.SetShape(shape);
		}

		private void OnQuickBackCheckboxChange()
		{
			var h4 = GetNode<HBoxContainer>("%BottomBgContainer");
			
			var h5 = GetNode<HBoxContainer>("%BottomCaptionContainer");

			h4.Visible = _quickBackCheckbox.ButtonPressed;
			h5.Visible = _quickBackCheckbox.ButtonPressed;

			GetNode<SubViewportContainer>("%BottomPreviewContainer").Visible = _quickBackCheckbox.ButtonPressed;
		}

		private void OnPreviewTextColorChange(Color color)
		{
			_previewTop.SetTextColor(color);
		}

		private void OnBackgroundColorChanged(Color color)
		{
			_previewTop.SetBackgroundColor(color);
		}

		private void OnTextChange(string newtext)
		{
			_previewTop.SetText(newtext);
		}

		private void OnPreviewTextColor2Change(Color color)
		{
			_previewBottom.SetTextColor(color);
		}

		private void OnBackgroundColor2Changed(Color color)
		{
			_previewBottom.SetBackgroundColor(color);
		}

		private void OnText2Change(string newtext)
		{
			_previewBottom.SetText(newtext);
		}
		
		private void GetFrontFile()
		{
			ShowFileDialog("Select Front Image File", FrontFileSelected);
		}

		private Texture GetQuickTexture()
		{
			return _previewTop.GetTexture();
		}
		
		private Texture GetQuickTexture2()
		{
			var t = _previewBottom.GetTexture();
			var i = t.GetImage();
			i.FlipX();

			var it = new ImageTexture();
			it.SetImage(i);
			return it;
		}
		
		private void FrontFileSelected(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				_frontImage.Text = file;
			}
		}
	
		private void GetBackFile()
		{
			ShowFileDialog("Select Back Image File", BackFileSelected);
		}
		private void BackFileSelected(string file)
		{
			if (!string.IsNullOrEmpty(file))
			{
				_backImage.Text = file;
			}
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
			d.Add("Thickness", ParamToFloat(_thicknessInput.Text));
			d.Add("FrontImage", _frontImage.Text);
			d.Add("BackImage", _backImage.Text);
			d.Add("Shape", _shapePicker.Selected);
			d.Add("Mode", _tabs.CurrentTab);
			d.Add("FrontBgColor", _quickBackgroundColor.Color);
			d.Add("FrontCaption", _quickText.Text);
			d.Add("FrontCaptionColor", _quickTextColor.Color);
			d.Add("DifferentBack", _quickBackCheckbox.ButtonPressed);
			d.Add("BackBgColor", _quickBackgroundColor2.Color);
			d.Add("BackCaption", _quickText2.Text);
			d.Add("BackCaptionColor", _quickTextColor2.Color);
			return d;
		}
}
