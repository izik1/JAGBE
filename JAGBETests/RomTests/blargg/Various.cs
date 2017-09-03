using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.blargg
{
    [TestClass]
    [TestCategory("blargg/various")]
    public class Various
    {
        private const string BasePath = "blargg/";

        [TestMethod]
        public void Halt_bug() => TestDisplayOut(BasePath + "halt_bug.gb", "xmlMgqFxGsPYoFT5tP0Ge9UOelB6R/SgDyhkSiORyks=", true);
    }
}
