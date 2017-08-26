using Microsoft.VisualStudio.TestTools.UnitTesting;
using JAGBE.GB.Emulation;

namespace JAGBETests
{
    [TestClass]
    public class GbMemoryTests
    {
        [TestMethod]
        [TestCategory("Construction")]
        public void MemoryConstructorsAllowNull()
        {
#pragma warning disable RECS0026 // Possible unassigned object created by 'new'
            new GbMemory();
            new GbMemory(null);
#pragma warning restore RECS0026 // Possible unassigned object created by 'new'
        }
    }
}
