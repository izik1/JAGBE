using System;
using System.IO;

namespace JAGBE.Logging
{
    internal sealed class Logger
    {
        public static readonly Logger Instance = new Logger(Console.Error, 3);

        private ushort minPriority;
        private TextWriter writer;

        public Logger(TextWriter writer, ushort minPriority)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.minPriority = minPriority;
        }

        public void Redirect(TextWriter writer) => this.writer = writer ?? throw new ArgumentNullException(nameof(writer));

        public void SetLogLevel(ushort minPriority) => this.minPriority = minPriority;

        public void WriteLine(ushort priority, string message)
        {
            if (priority == 0)
            {
                throw new ArgumentException(nameof(priority) + " cannot be 0");
            }

            if (this.minPriority >= priority)
            {
                return;
            }

            this.writer.WriteLine("[" + priority.ToString("X4") + "]: " + message);
        }
    }
}
