using System;
using System.Collections.Generic;
using System.Drawing;
using System.Text;

namespace GreenScreenAdjuster
{
    public static class Utilities
    {
        public static Image CaptureScreen()
        {
            return CaptureWindow(User32.GetDesktopWindow());
        }

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

        public static Image CaptureWindow(IntPtr handle)
        {
            IntPtr hdcSrc = User32.GetWindowDC(handle);

            var windowRect = new Rect();
            User32.GetWindowRect(handle, ref windowRect);

            int width = windowRect.Right - windowRect.Left;
            int height = windowRect.Bottom - windowRect.Top;

            IntPtr hdcDest = Gdi32.CreateCompatibleDC(hdcSrc);
            IntPtr hBitmap = Gdi32.CreateCompatibleBitmap(hdcSrc, width, height);

            IntPtr hOld = Gdi32.SelectObject(hdcDest, hBitmap);
            Gdi32.BitBlt(hdcDest, 0, 0, width, height, hdcSrc, 0, 0, ApiConstants.SRCCOPY);
            Gdi32.SelectObject(hdcDest, hOld);
            Gdi32.DeleteDC(hdcDest);
            User32.ReleaseDC(handle, hdcSrc);

            Image image = Image.FromHbitmap(hBitmap);
            Gdi32.DeleteObject(hBitmap);

            return image;
        }
    }
}
