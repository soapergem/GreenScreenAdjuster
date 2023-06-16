using System;
using System.Runtime.InteropServices;

namespace GreenScreenAdjuster
{
    [StructLayout(LayoutKind.Sequential)]
    public struct Rect : IEquatable<Rect>
    {
        public int Left;
        public int Top;
        public int Right;
        public int Bottom;
        public override bool Equals(object obj)
        {
            if (!(obj is Rect))
            {
                return false;
            }

            var rect = (Rect)obj;
            return Equals(rect);
        }
        public bool Equals(Rect rect)
        {
            return rect.Left == Left && rect.Right == Right && rect.Top == Top && rect.Bottom == Bottom;
        }
        public bool SizeEquals(Rect rect)
        {
            return rect.Right - rect.Left == Right - Left && rect.Bottom - rect.Top == Bottom - Top;
        }
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
        public static bool operator ==(Rect left, Rect right)
        {
            return left.Equals(right);
        }
        public static bool operator !=(Rect left, Rect right)
        {
            return !left.Equals(right);
        }
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ExtPoint
    {
        public int X;
        public int Y;
    }
}
