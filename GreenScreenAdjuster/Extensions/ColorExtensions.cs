using System.Windows.Media;

namespace GreenScreenAdjuster.Extensions
{
    public static class ColorExtensions
    {
        public static string HexConverter(this System.Drawing.Color c)
        {
            return c.R.ToString("X2") + c.G.ToString("X2") + c.B.ToString("X2");
        }

        public static Brush FromHex(this string color)
        {
            var converter = new BrushConverter();
            return (Brush)converter.ConvertFromString("#FF" + color);
        }
    }
}
