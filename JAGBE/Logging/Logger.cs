using System;
using System.IO;

namespace JAGBE.Logging
{
    internal sealed class Logger
    {
        public static readonly Logger Instance = new Logger(Console.Error, 1);

        private ushort minPriority;
        private TextWriter writer;

        public Logger(TextWriter writer, ushort minPriority)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.minPriority = minPriority;
        }

        public static void LogError(string message) => Instance.WriteError(message);

        public static void LogInfo(string message) => Instance.WriteInfo(message);

        public static void LogVerbose(string message) => Instance.WriteVerbose(message);

        public static void LogWarning(string message) => Instance.WriteWarning(message);

        public void Redirect(TextWriter writer) => this.writer = writer ?? throw new ArgumentNullException(nameof(writer));

        public void SetLogLevel(ushort minPriority) => this.minPriority = minPriority;

        public void WriteError(string message) => WriteLine(4, message);

        public void WriteInfo(string message) => WriteLine(2, message);

        public void WriteVerbose(string message) => WriteLine(1, message);

        public void WriteWarning(string message) => WriteLine(3, message);

        private void WriteLine(ushort priority, string message)
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
