using Microsoft.VisualStudio.TestTools.UnitTesting;
using static JAGBETests.RomTests.Helpers;

namespace JAGBETests.RomTests.mooneye
{
    [TestClass]
    [TestCategory("mooneye-gb_hwtests/acceptance/timer")]
    public class Timer
    {
        private const string BasePath = AcceptanceTests.BasePath + "timer/";

        [TestMethod]
        public void Div_write() => TestDisplayOut(BasePath + "div_write.gb", "ct/pSMvekPxIlT/NLHziuq1NDmhtjOC6zt2GF3aDBrs=", true);

        [TestMethod]
        public void Rapid_toggle() => TestDisplayOut(BasePath + "rapid_toggle.gb",
                "", false, "B1VQiIvWUVF9q5TPxWNfBNIEObZYWPhndpl7QhvgstE=", "uyuV/XHV79yfeZ7YCmdrC/qzDYNza6SfYFU79+zSvkQ=");

        [TestMethod]
        public void Tim00() => TestDisplayOut(BasePath + "tim00.gb",
                "bOspG9EOxql8miQM0HT9CUGNaEM+msiffUK9VPog5vU=", true, "kqHHFZX8+VTWPm387UNr3XVsekc9IZ57pmY76m8MBdQ=");

        [TestMethod]
        public void Tim00_div_trigger() => TestDisplayOut(BasePath + "tim00_div_trigger.gb",
            "bOspG9EOxql8miQM0HT9CUGNaEM+msiffUK9VPog5vU=", true, "kqHHFZX8+VTWPm387UNr3XVsekc9IZ57pmY76m8MBdQ=");

        [TestMethod]
        public void Tim01() => TestDisplayOut(BasePath + "tim01.gb", "6zkveaormeqhJ0rIvoe0n6EOjzWVL2ByU3Xi7b4AvTw=", true);

        [TestMethod]
        public void Tim01_div_trigger() =>
            TestDisplayOut(BasePath + "tim01_div_trigger.gb", "FEKgX6xmwvxOCDpAMSIy6Zzs8KnSZ1DSdpHA24WEZ4g=", true);

        [TestMethod]
        public void Tim10() => TestDisplayOut(BasePath + "tim10.gb", "bOspG9EOxql8miQM0HT9CUGNaEM+msiffUK9VPog5vU=", true);

        [TestMethod]
        public void Tim10_div_trigger() =>
            TestDisplayOut(BasePath + "tim10_div_trigger.gb", "1YG3zaeiQDuJ5glz8OBA2ELfnI6gc9OoDyrrwUPmlfs=", true);

        [TestMethod]
        public void Tim11() => TestDisplayOut(BasePath + "tim11.gb", "bOspG9EOxql8miQM0HT9CUGNaEM+msiffUK9VPog5vU=", true);

        [TestMethod]
        public void Tim11_div_trigger() =>
            TestDisplayOut(BasePath + "tim11_div_trigger.gb", "bOspG9EOxql8miQM0HT9CUGNaEM+msiffUK9VPog5vU=", true);

        [TestMethod]
        public void Tima_reload() => TestDisplayOut(BasePath + "tima_reload.gb",
            "", false, "iBBMHwQQlHbfhxgyQcBnu4wdjc3p8kcdtrGND6+VVSc=", "YSDJnrv8TiZASsybmkEQ1cR0dNA3t3NRlw0YZz/OCr4=");

        [TestMethod]
        public void Tima_write_reloading() => TestDisplayOut(BasePath + "tima_write_reloading.gb",
            "1Dch5XwHPLKcUqwlNitn3ji5xtpct14F78hgmgI/aEA=", true, "rCJEsbbvrCoNqNrYbDpj2ZaG6mzg5c17RQuMp3B6zbs=");

        [TestMethod]
        public void Tma_write_reloading() => TestDisplayOut(BasePath + "tma_write_reloading.gb",
            "lCBRN2pVffiBnn2iXOPRAfNsjarwG90bXfBm9lmNyFg=", true, "lVh/1pAZ+jJHdn4m6v71r2dlFjh9Ha7wp9XRVmi92h0=");
    }
}
