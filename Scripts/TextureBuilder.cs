using Godot;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using SixLabors.Fonts;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Drawing;
using SixLabors.ImageSharp.Drawing.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using Color = Godot.Color;
using Font = Godot.Font;
using PointF = SixLabors.ImageSharp.PointF;
using Vector4 = System.Numerics.Vector4;
using VerticalAlignment = SixLabors.Fonts.VerticalAlignment;

public static class TextureBuilder
{
    public static ImageTexture Build(Color backgroundColor, string caption, Color textColor, int height, int width)
    {
        var bgc = GodotColorToImageSharp(backgroundColor);
        var tc = GodotColorToImageSharp(textColor);

        using SixLabors.ImageSharp.Image image = new Image<Rgba32>(width, height);
        
        image.Mutate(x => x.Fill(bgc));
        FontCollection collection = new();
        FontFamily family = collection.Add(@"c:\windows\fonts\arial.ttf");

        var size = AutosizeFont(caption, family, height, width, 9);
        var font = family.CreateFont(size);

        var to = new TextOptions(font);
        to.VerticalAlignment = VerticalAlignment.Center;

        var textPath = TextBuilder.GenerateGlyphs(caption, to);
        
        var fontRect = textPath.Bounds;
        var wOffset = (width - fontRect.Width) / 2;
        var hOffset = ((height - fontRect.Height) / 2) - fontRect.Y;
        
        var offsetP = new PointF(wOffset, hOffset);

        var finalPath = textPath.Translate(offsetP);

        var white = TextureBuilder.GodotColorToImageSharp(Colors.White);

        //image.Mutate(x => x.DrawText(caption, font, tc, offsetP));
        
        DrawingOptions options = new()
        {
            GraphicsOptions = new()
            {
                ColorBlendingMode  = PixelColorBlendingMode.Normal
            }
        };
        image.Mutate(x => x.Fill(tc, finalPath));
        
        List<IPath> paths = new();
        foreach (var p in finalPath)
        {
            paths.Add(p.GenerateOutline(1));
        }

        var outlinePath = new PathCollection(paths);
        
        //image.Mutate(x => x.Draw(options, white, 1f, outlinePath));

        using var s = new MemoryStream();
        
        image.Save(s, JpegFormat.Instance);

        Godot.Image gImage = new();
        gImage.LoadJpgFromBuffer(s.ToArray());

        var t = new ImageTexture();
        t.SetImage(gImage);

        return t;
    }

    private static int AutosizeFont(string caption, FontFamily fontFamily, int height, int width, int minSize)
    {
        var size = minSize;

        float targetWidth = width * 0.8f;

        while (true)
        {
            var font = fontFamily.CreateFont(size);
            var fontRect = TextMeasurer.MeasureSize(caption, new TextOptions(font));

            if (fontRect.Width > targetWidth)
            {
                return Math.Max(size, minSize);
            }

            size++;
        }
    }

    public static SixLabors.ImageSharp.Color GodotColorToImageSharp(Color color)
    {
        return new SixLabors.ImageSharp.Color(new Vector4(color.R, color.G, color.B,
            color.A));
    }
}
