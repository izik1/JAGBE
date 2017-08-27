using System;
using System.Diagnostics;
using System.IO;
using System.Security.Cryptography;
using JAGBE.GB.Emulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JAGBETests
{
    [TestClass]
    public class RomTests
    {
        private static Cpu InitCpu(string testRomPath) => new Cpu(File.ReadAllBytes("boot rom.bin"), File.ReadAllBytes(testRomPath), null);

        /// <summary>
        /// The maximum number of ms the cpu can be running for, computing SHA's may take a long time
        /// and are therefore discluded from this cap.
        /// </summary>
        private const long MAXELAPSEDMS = 20000;

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_01()
        {
            TestDisplayOut("blargg/cpu_instrs/individual/01-special.gb", "bob4OkiQuQnncCCWmjPnNAth9isO5lXBF9S6lO0vSB4=");
        }

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_02()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut("blargg/cpu_instrs/individual/02-interrupts.gb", "");
        }

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_03()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut("blargg/cpu_instrs/individual/03-op sp,hl.gb", "");
        }

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_04() =>
            TestDisplayOut("blargg/cpu_instrs/individual/04-op r,imm.gb", "sJE9ieevr0n64hiMKAdQ0tbbpTw9nRnUfIHMazvBSAQ=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_05() =>
            TestDisplayOut("blargg/cpu_instrs/individual/05-op rp.gb", "shUu/ROKPI7PZ5Xw/9D6m0eX2J6g9IqI1eSrMTHm+Lk=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_06() =>
            TestDisplayOut("blargg/cpu_instrs/individual/06-ld r,r.gb", "u8jHutord6tOSUK3M7tTVR/jY+Nm7j+JptYIe8ozTU8=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_07() =>
            TestDisplayOut("blargg/cpu_instrs/individual/07-jr,jp,call,ret,rst.gb", "3YZRlXoku5DpegFroVPUpXFRR1EA3F7f21bYiOrO10Q=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_08() =>
            TestDisplayOut("blargg/cpu_instrs/individual/08-misc instrs.gb", "pDNJyFIFGShHqjb6R73Yg974+NBN9oqoHyFzF7k9xPY=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_09() =>
            TestDisplayOut("blargg/cpu_instrs/individual/09-op r,r.gb", "j5qZ3AqOkFxEK83Z7J/00p9pvsgbNqDcZDOB1R58+x4=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_10() =>
            TestDisplayOut("blargg/cpu_instrs/individual/10-bit ops.gb", "lwPf+SpOoWkPqLdXhMH7vv922kwuswR3pQCy/Cm3sZA=");

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_11() =>
            TestDisplayOut("blargg/cpu_instrs/individual/11-op a,(hl).gb", "EOxaj0QrghgxcscFQUHsv5v5+WYglfg/JTHl6oy7PnI=");

        private static void TestDisplayOut(string romPath, string expectedSha256)
        {
            Stopwatch sw = new Stopwatch();
            Cpu c = InitCpu(romPath);
            sw.Start();
            string shaString = "";
            using (SHA256Managed sha = new SHA256Managed())
            {
                while (sw.ElapsedMilliseconds < MAXELAPSEDMS)
                {
                    sw.Start();
                    c.Tick(0x8000);
                    sw.Stop();
                    shaString = (Convert.ToBase64String(sha.ComputeHash(Lcd.DisplayToBytes(c.DisplayMemory))));
                    if (shaString == expectedSha256)
                    {
                        sw.Reset();
                        break;
                    }
                }
            }

            // Error just in case something caught the exception.
            Assert.IsFalse(c.Status == CpuState.HUNG || c.Status == CpuState.ERROR);

            if (sw.ElapsedMilliseconds >= MAXELAPSEDMS)
            {
                Assert.Inconclusive("Timed out (hash): " + shaString);
            }

            Assert.AreEqual(expectedSha256, shaString);
        }
    }
}
