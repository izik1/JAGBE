using JAGBE.UI;
using System.Diagnostics;

namespace JAGBE
{
    /// <summary>
    /// Defines the entry class of the application.
    /// </summary>
    internal static class Program
    {
        /// <summary>
        /// The number of updates the emulator will recieve per second.
        /// </summary>
        public const int UPS = 64;

        /// <summary>
        /// Defines the entry point of the application.
        /// </summary>
        private static void Main()
        {
            Stats.AttributeReflector.GetAllStubs();
            Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            using (Window w = new Window(1))
            {
                w.Run(UPS, 59.97);
            }
        }
    }
}
