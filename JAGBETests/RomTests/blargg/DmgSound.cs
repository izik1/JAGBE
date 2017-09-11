using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.blargg
{
    [TestClass]
    [TestCategory("blargg/dmg_sound")]
    public class DmgSound
    {
        private const string baseTestPath = "blargg/dmg_sound/individual/";

        [TestMethod]
        public void Dmg_sound_01() => TestDisplayOut(baseTestPath + "01-registers.gb", "", false);

        [TestMethod]
        public void Dmg_sound_02() => TestDisplayOut(baseTestPath + "02-len ctr.gb", "", false);

        [TestMethod]
        public void Dmg_sound_03() => TestDisplayOut(baseTestPath + "03-trigger.gb", "", false);

        [TestMethod]
        public void Dmg_sound_04() => TestDisplayOut(baseTestPath + "04-sweep.gb", "", false);

        [TestMethod]
        public void Dmg_sound_05() => TestDisplayOut(baseTestPath + "05-sweep details.gb", "", false);

        [TestMethod]
        public void Dmg_sound_06() => TestDisplayOut(baseTestPath + "06-overflow on trigger.gb", "", false);

        [TestMethod]
        public void Dmg_sound_07() => TestDisplayOut(baseTestPath + "07-len sweep period sync.gb", "", false);

        [TestMethod]
        public void Dmg_sound_08() => TestDisplayOut(baseTestPath + "08-len ctr during power.gb", "", false);

        [TestMethod]
        public void Dmg_sound_09() => TestDisplayOut(baseTestPath + "09-wave read while on.gb", "", false);

        [TestMethod]
        public void Dmg_sound_10() => TestDisplayOut(baseTestPath + "10-wave trigger while on.gb", "", false);

        [TestMethod]
        public void Dmg_sound_11() => TestDisplayOut(baseTestPath + "11-regs after power.gb", "", false);

        [TestMethod]
        public void Dmg_sound_12() => TestDisplayOut(baseTestPath + "12-wave write while on.gb", "", false);
    }
}
