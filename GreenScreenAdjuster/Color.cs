using System;

namespace GreenScreenAdjuster
{
    public class Color
    {
        public int Red { get; set; }
        public int Green { get; set; }
        public int Blue { get; set; }

        public string ToHexCode()
        {
            return Red.ToString("X2") + Green.ToString("X2") + Blue.ToString("X2");
        }

        public override string ToString()
        {
            return ToHexCode();
        }

        public uint ToObsUInt()
        {
            uint result = 0xFF000000;
            result |= (uint)Blue << 16;
            result |= (uint)Green << 8;
            result |= (uint)Red;
            return result;
        }

        public static Color FromHexCode(string hexCode)
        {
            var integer = Convert.ToInt32(hexCode, 16);
            return new Color {
                Red = (integer >> 16) & 0xFF,
                Green = (integer >> 8) & 0xFF,
                Blue = integer & 0xFF
            };
        }
    }
}
