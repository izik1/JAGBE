using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests
{
    [TestClass]
    [TestCategory("cpu_instrs")]
    public class BlarggCpuInstrsTests
    {
        private const string baseTestPath = "blargg/cpu_instrs/individual/";

        [TestMethod]
        public void Cpu_Instrs_01() => TestDisplayOut(baseTestPath + "01-special.gb", "bob4OkiQuQnncCCWmjPnNAth9isO5lXBF9S6lO0vSB4=");

        [TestMethod]
        public void Cpu_Instrs_02()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(baseTestPath + "02-interrupts.gb", "");
        }

        [TestMethod]
        public void Cpu_Instrs_03() => TestDisplayOut(baseTestPath + "03-op sp,hl.gb", "CgqhrBepL9+hTWlUBzGjV0ajffCcArgdCEEKaNQ0SIE=");

        [TestMethod]
        public void Cpu_Instrs_04() => TestDisplayOut(baseTestPath + "04-op r,imm.gb", "sJE9ieevr0n64hiMKAdQ0tbbpTw9nRnUfIHMazvBSAQ=");

        [TestMethod]
        public void Cpu_Instrs_05() => TestDisplayOut(baseTestPath + "05-op rp.gb", "shUu/ROKPI7PZ5Xw/9D6m0eX2J6g9IqI1eSrMTHm+Lk=");

        [TestMethod]
        public void Cpu_Instrs_06() => TestDisplayOut(baseTestPath + "06-ld r,r.gb", "u8jHutord6tOSUK3M7tTVR/jY+Nm7j+JptYIe8ozTU8=");

        [TestMethod]
        public void Cpu_Instrs_07() =>
            TestDisplayOut(baseTestPath + "07-jr,jp,call,ret,rst.gb", "3YZRlXoku5DpegFroVPUpXFRR1EA3F7f21bYiOrO10Q=");

        [TestMethod]
        public void Cpu_Instrs_08() => TestDisplayOut(baseTestPath + "08-misc instrs.gb", "pDNJyFIFGShHqjb6R73Yg974+NBN9oqoHyFzF7k9xPY=");

        [TestMethod]
        public void Cpu_Instrs_09() => TestDisplayOut(baseTestPath + "09-op r,r.gb", "j5qZ3AqOkFxEK83Z7J/00p9pvsgbNqDcZDOB1R58+x4=");

        [TestMethod]
        public void Cpu_Instrs_10() => TestDisplayOut(baseTestPath + "10-bit ops.gb", "lwPf+SpOoWkPqLdXhMH7vv922kwuswR3pQCy/Cm3sZA=");

        [TestMethod]
        public void Cpu_Instrs_11() => TestDisplayOut(baseTestPath + "11-op a,(hl).gb", "EOxaj0QrghgxcscFQUHsv5v5+WYglfg/JTHl6oy7PnI=");
    }
}
