using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.mooneye
{
    public static class EmulatorOnly
    {
        internal const string BasePath = "mooneye-gb_hwtests/emulator-only/";

        [TestClass]
        [TestCategory("mooneye-gb_hwtests/emulator-only/MBC1")]
        public class Mbc1
        {
            private const string Path = BasePath + "MBC1/";

            private const string GenericSuccessSha256 = "ct/pSMvekPxIlT/NLHziuq1NDmhtjOC6zt2GF3aDBrs=";

            [TestMethod]
            public void Multicart_rom_8Mb() => TestDisplayOut(Path + "multicart_rom_8Mb.gb",
                "", false, "SQ4jC+XQ0mnl+kn/+ornDMwcgDUm3uOu9eFUY31Gcj0=");

            [TestMethod]
            public void Ram_64Kb() => TestDisplayOut(Path + "ram_64Kb.gb", GenericSuccessSha256, true);

            [TestMethod]
            public void Ram_256Kb() => TestDisplayOut(Path + "ram_256Kb.gb", GenericSuccessSha256, true);

            [TestMethod]
            public void Rom_1Mb() => TestDisplayOut(Path + "rom_1Mb.gb", GenericSuccessSha256, true);

            [TestMethod]
            public void Rom_2Mb() => TestDisplayOut(Path + "rom_2Mb.gb", GenericSuccessSha256, true);

            [TestMethod]
            public void Rom_4Mb() => TestDisplayOut(Path + "rom_4Mb.gb", GenericSuccessSha256, true);

            [TestMethod]
            public void Rom_8Mb() => TestDisplayOut(Path + "rom_8Mb.gb", "", false, "9eWFVgmC+DsQ9G+Ep4gyYpk2L7I/IzYsgP6srBBzFr4=");

            [TestMethod]
            public void Rom_16Mb() => TestDisplayOut(Path + "rom_16Mb.gb", "", false, "9eWFVgmC+DsQ9G+Ep4gyYpk2L7I/IzYsgP6srBBzFr4=");

            [TestMethod]
            public void Rom_512Kb() => TestDisplayOut(Path + "rom_512Kb.gb", GenericSuccessSha256, true);
        }
    }
}
