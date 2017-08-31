using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.mooneye
{
    [TestClass]
    [TestCategory("mooneye-gb_hwtests/acceptance")]
    public class AcceptanceTests
    {
        private const string BasePath = "mooneye-gb_hwtests/acceptance/";

        [TestMethod]
        public void Add_sp_e_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "add_sp_e_timing.gb", "");
        }

        [TestMethod]
        public void Boot_hwio_dmgABCXmgb()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "boot_hwio-dmgABCXmgb.gb", "");
        }

        [TestMethod]
        public void Boot_regs_dmgABCX() =>
            TestDisplayOut(BasePath + "boot_regs-dmgABCX.gb", "2pe1gNp6ILzHC1A8Ds4tTn7GKwImh18zL3UWsVZkCpg=");

        [TestMethod]
        public void Call_cc_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "call_cc_timing.gb", "");
        }

        [TestMethod]
        public void Call_cc_timing2()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "call_cc_timing2.gb", "");
        }

        [TestMethod]
        public void Call_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "call_timing.gb", "");
        }

        [TestMethod]
        public void Call_timing2()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "call_timing2.gb", "");
        }

        [TestMethod]
        public void DI_timing_GS() =>
            TestDisplayOut(BasePath + "di_timing-GS.gb", "ct/pSMvekPxIlT/NLHziuq1NDmhtjOC6zt2GF3aDBrs=");

        [TestMethod]
        public void Div_timing() => TestDisplayOut(BasePath + "div_timing.gb", "gLcCnRta6x+9hIQm+320dn8ErOqS9fFYGCKsAuZXQ2E=");

        [TestMethod]
        public void EI_timing() => TestDisplayOut(BasePath + "ei_timing.gb", "jPm4UvL49A9TOhdjVuUCOctyOBxcpInzC1frlVtyq1s=");

        [TestMethod]
        public void Halt_ime0_ei()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "halt_ime0_ei.gb", "");
        }

        [TestMethod]
        public void Halt_ime0_nointr_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "halt_ime0_nointr_timing.gb", "");
        }

        [TestMethod]
        public void Halt_ime1_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "halt_ime1_timing.gb", "");
        }

        [TestMethod]
        public void Halt_ime1_timing2_GS() =>
            TestDisplayOut(BasePath + "halt_ime1_timing2-GS.gb", "mYRy+tkF2McRZrOD90zONWZv4diwUfG3cuPjvOEHZJM=");

        [TestMethod]
        public void IF_IE_registers() => TestDisplayOut(BasePath + "if_ie_registers.gb", "cMXEWWiLEgUdvTelKty/59AYf5GPI71lp5DpGS45n6c=");

        [TestMethod]
        public void Intr_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "intr_timing.gb", "");
        }

        [TestMethod]
        public void JP_cc_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "jp_cc_timing.gb", "");
        }

        [TestMethod]
        public void JP_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "jp_timing.gb", "");
        }

        [TestMethod]
        public void LD_hl_sp_e_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "ld_hl_sp_e_timing.gb", "");
        }

        [TestMethod]
        public void Oam_dma_restart() => TestDisplayOut(BasePath + "oam_dma_restart.gb", "j9oxRWGIXPDL+nSTCyJtroBt2PwJNvRFtTD1t03y/8A=");

        [TestMethod]
        public void Oam_dma_start()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "oam_dma_start.gb", "");
        }

        [TestMethod]
        public void Oam_dma_timing() => TestDisplayOut(BasePath + "oam_dma_timing.gb", "j9oxRWGIXPDL+nSTCyJtroBt2PwJNvRFtTD1t03y/8A=");

        [TestMethod]
        public void Pop_timing() => TestDisplayOut(BasePath + "pop_timing.gb", "yXzxDxECgU1W/KW+HBl9/2LLopxEkMGJ83lOv6siIVc=");

        [TestMethod]
        public void Push_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "push_timing.gb", "");
        }

        [TestMethod]
        public void Rapid_DI_EI() => TestDisplayOut(BasePath + "rapid_di_ei.gb", "GUYCaGx8WO8jS7qPZdfAZplqRj1HY04nSuhJ+Lahdnw=");

        [TestMethod]
        public void Ret_cc_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "ret_cc_timing.gb", "");
        }

        [TestMethod]
        public void Ret_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "ret_timing.gb", "");
        }

        [TestMethod]
        public void Reti_intr_timing() => TestDisplayOut(BasePath + "reti_intr_timing.gb", "Rue6zf+aapVRSCfsX7QHbX/V/RiDO1BrLV2bC3uqN2A=");

        [TestMethod]
        public void Reti_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "reti_timing.gb", "");
        }

        [TestMethod]
        public void Rst_timing()
        {
            Assert.Inconclusive("No SHA");
            TestDisplayOut(BasePath + "rst_timing.gb", "");
        }
    }
}
