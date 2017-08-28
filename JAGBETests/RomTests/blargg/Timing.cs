using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.blargg
{
    [TestClass]
    [TestCategory("blargg/timing")]
    public class Timing
    {
        private const string baseTestPath = "blargg/";

        [TestMethod]
        public void Instr_timing() => TestDisplayOut(baseTestPath + "instr_timing.gb", "f2X5PeVcUBLTLSu5710IDIgmKec/TIGudHjv+NbcQ3Q=");
    }
}
