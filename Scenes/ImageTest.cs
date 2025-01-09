using System.IO;
using Godot;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Processing;
using Color = System.Drawing.Color;
using Vector4 = System.Numerics.Vector4;

public partial class ImageTest : Sprite3D
{
	// Called when the node enters the scene tree for the first time.
	public override void _Ready()
	{
		
	}

	// Called every frame. 'delta' is the elapsed time since the previous frame.
	public override void _Process(double delta)
	{
	}

	private void TextTest()
	{
		using (SixLabors.ImageSharp.Image image = SixLabors.ImageSharp.Image.Load(@"C:\Users\gme\source\repos\TTSS\Textures\card_back.jpg"	)) 
		{
			// Resize the image in place and return it for chaining.
			// 'x' signifies the current image processing context.
			image.Mutate(x => x.Resize(image.Width / 2, image.Height / 2)); 

			FontCollection collection = new();
			FontFamily family = collection.Add(@"c:\windows\fonts\arial.ttf");
			var font = family.CreateFont(20, FontStyle.Bold);
			string yourText = "this is some sample text";

			image.Mutate(x=> x.DrawText("Hello!", font, new SixLabors.ImageSharp.Color(new Vector4(0,0,0,1)), new PointF(10,30)));
			// The library automatically picks an encoder based on the file extension then
			// encodes and write the data to disk.
			// You can optionally set the encoder to choose.
			image.Save("bar.jpg");
			var s = new MemoryStream();
			image.Save(s, image.Metadata.DecodedImageFormat!);
			
			Godot.Image gImage = new();
			gImage.LoadJpgFromBuffer(s.ToArray());
			var t = new ImageTexture();
			t.SetImage(gImage);
			Texture = t;
		} // Dispose - releasing memory into a memory pool ready for the next image you wish to process.
	}
}
