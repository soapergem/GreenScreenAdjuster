using System.Drawing;
using System.Windows.Forms;

namespace GreenScreenAdjuster
{
    public static class ControlExtensions
    {
        public static Image DrawToImage(this Control control)
        {
            return Utilities.CaptureWindow(control.Handle);
        }
    }
}
