using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using GreenScreenAdjuster.Extensions;

namespace GreenScreenAdjuster
{
    public static class Utilities
    {
        public static Dictionary<IntPtr, string> GetOpenWindows()
        {
            var windowHandles = GetOpenWindowHandles();
            var windows = new Dictionary<IntPtr, string>();
            foreach (var windowHandle in windowHandles)
            {
                var title = GetWindowTitle(windowHandle);
                if (!string.IsNullOrWhiteSpace(title))
                {
                    windows[windowHandle] = title;
                }
            }
            return windows;
        }

        public static List<IntPtr> GetOpenWindowHandles()
        {
            List<IntPtr> windowHandles = new List<IntPtr>();
            User32.EnumWindowsProc callback = (hwnd, lParam) =>
            {
                if (User32.IsWindowVisible(hwnd))
                {
                    windowHandles.Add(hwnd);
                }
                return true;
            };

            User32.EnumWindows(callback, IntPtr.Zero);
            return windowHandles;
        }

        public static Rect GetWindowRect(IntPtr windowHandle)
        {
            var windowBounds = new Rect();
            User32.GetWindowRect(windowHandle, ref windowBounds);
            return windowBounds;
        }

        public static string GetWindowTitle(IntPtr windowHandle)
        {
            const int maxLength = 256;
            StringBuilder titleBuilder = new StringBuilder(maxLength);
            User32.GetWindowText(windowHandle, titleBuilder, maxLength);
            return titleBuilder.ToString();
        }
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
