using GreenScreenAdjuster;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace GreenScreenAdjusterTests
{
    [TestClass]
    public class ColorTests
    {
        [TestMethod]
        public void TestFromHex()
        {
            var color = Color.FromHexCode("4aae78");
            Assert.AreEqual(74, color.Red);
            Assert.AreEqual(174, color.Green);
            Assert.AreEqual(120, color.Blue);
            Assert.AreEqual(4286099018, color.ToObsUInt());
        }

        [TestMethod]
        public void TestMe()
        {
            var thing = 5;
            Assert.AreEqual(5, thing);
        }
    }
}
