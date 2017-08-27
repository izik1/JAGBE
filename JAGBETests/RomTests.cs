using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using JAGBE.GB.Emulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JAGBETests
{
    [TestClass]
    public class RomTests
    {
        private Cpu InitCpu(string testRomPath) => new Cpu(File.ReadAllBytes("boot rom.bin"), File.ReadAllBytes(testRomPath), null);

        /// <summary>
        /// The maximum number of ms the cpu can be running for, computing SHA's may take a long time
        /// and are therefore discluded from this cap.
        /// </summary>
        private const long MAXELAPSEDMS = 20000;

        [TestMethod]
        [TestCategory("Rom Test")]
        public void Blargg_cpu_instrs_01()
        {
            const string expectedOutputSHA256 = "bob4OkiQuQnncCCWmjPnNAth9isO5lXBF9S6lO0vSB4=";

            Stopwatch sw = new Stopwatch();
            Cpu c = InitCpu("blargg/cpu_instrs/individual/01-special.gb");
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
                    if (shaString == expectedOutputSHA256)
                    {
                        break;
                    }
                }
            }

            Assert.AreEqual(expectedOutputSHA256, shaString);
        }
    }
}
