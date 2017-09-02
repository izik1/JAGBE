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
        public void Instr_timing() =>
            TestDisplayOut(baseTestPath + "instr_timing.gb", "f2X5PeVcUBLTLSu5710IDIgmKec/TIGudHjv+NbcQ3Q=", true);

        private const string memTiming2Path = baseTestPath + "mem_timing-2/rom_singles/";

        [TestMethod]
        public void Mem_timing2_1() => TestDisplayOut(memTiming2Path + "01-read_timing.gb",
                "v16iKyqfBrhuzDeygQ6cyUSMEytauKnmdfehnL+RwC8=", true, "sjoFBu3vwDklM/CzJ4UgKWnMb6ZTE4lhmMdL6CjEtGE=");

        [TestMethod]
        public void Mem_timing2_2() => TestDisplayOut(memTiming2Path + "02-write_timing.gb",
                "98cPLkBHhtuw8PcvXTumtyZUplFF5kf/2WSnTDFR42M=", true, "1zeW0Hqvhe1lJPNT4Dty2+IKSNSY81KRTs+jO1pbihg=");

        [TestMethod]
        public void Mem_timing2_3() => TestDisplayOut(memTiming2Path + "03-modify_timing.gb",
                "FHcBI+nQVf9VA7lGDsDVrmXxVYb+gkjBOjbyYTFCE/k=", true, "Us9xIpS9n54kZbH9rvnvIo64nMDhGqJL14hgmfpU9f0=");
    }
}
