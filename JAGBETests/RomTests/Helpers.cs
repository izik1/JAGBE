using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using JAGBE.GB.Emulation;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace JAGBETests.RomTests
{
    internal static class Helpers
    {
        private static Cpu InitCpu(string testRomPath) => new Cpu(File.ReadAllBytes("boot rom.bin"), File.ReadAllBytes(testRomPath), null);

        /// <summary>
        /// The maximum number of ms the cpu can be running for, computing SHA's may take a long time
        /// and are therefore discluded from this cap.
        /// </summary>
        private const long MAXELAPSEDMS = 20000;

        internal static void TestDisplayOut(string romPath, string expectedSha256, bool passes, params string[] failShas)
        {
            if (failShas == null)
            {
                throw new ArgumentNullException(nameof(failShas));
            }

            if (!passes && failShas.Length == 0)
            {
                Assert.Inconclusive("Test isn't supposed to pass and there are no SHA's to test.");
            }

            Stopwatch sw = new Stopwatch();
            Cpu c = InitCpu(romPath);
            string shaString = "";
            using (SHA256Managed sha = new SHA256Managed())
            {
                while (sw.ElapsedMilliseconds < MAXELAPSEDMS)
                {
                    sw.Start();
                    c.Tick(0x8000);
                    sw.Stop();
                    shaString = (Convert.ToBase64String(sha.ComputeHash(c.DisplayMemoryAsBytes())));

                    // Check for error just in case something caught the exception.
                    Assert.IsFalse(c.Status == CpuState.HUNG);

                    if (passes)
                    {
                        if (shaString == expectedSha256)
                        {
                            sw.Reset();
                            break;
                        }

                        Assert.IsFalse(failShas.Contains(shaString), "Hit fail SHA256 (Was supposed to pass)");
                    }
                    else
                    {
                        Assert.IsFalse(shaString == expectedSha256, "Test is supposted to fail but it 'passes'.");

                        if (failShas.Contains(shaString))
                        {
                            return; // 'Pass', as this is a expected result.
                        }
                    }
                }
            }

            if (sw.ElapsedMilliseconds >= MAXELAPSEDMS)
            {
                Assert.Inconclusive("Timed out (hash): " + shaString);
            }

            Assert.AreEqual(expectedSha256, shaString);
        }
    }
}
