using System.Drawing;

namespace UnoReng;

public class ColorModel
{
    public string HEX { get; }
    public string RGB { get; }
    public Color Color { get; }

    public ColorModel(Color color)
    {
        Color = color;
        HEX = $"#{color.R:X2}{color.G:X2}{color.B:X2}";
        RGB = $"rgb({color.R},{color.G},{color.B})";
    }
}
