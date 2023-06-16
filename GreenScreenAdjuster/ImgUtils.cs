using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using GreenScreenAdjuster.Extensions;

namespace GreenScreenAdjuster
{
    public class ImgUtils
    {
        public static string GetAverageColor(Bitmap image)
        {
            var reds = new List<int>();
            var greens = new List<int>();
            var blues = new List<int>();

            for (var i = 0; i < image.Width; i++)
            {
                for (var j = 0; j < image.Height; j++)
                {
                    System.Drawing.Color color = image.GetPixel(i, j);
                    reds.Add(color.R);
                    greens.Add(color.G);
                    blues.Add(color.B);
                }
            }

            var combined = System.Drawing.Color.FromArgb(
                (int)reds.Average(),
                (int)greens.Average(),
                (int)blues.Average()
            );

            return combined.HexConverter();
        }
    }
}
