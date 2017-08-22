using System;
using System.IO;

namespace JAGBE.Logging
{
    internal sealed class Logger
    {
        /// <summary>
        /// The active instance of the <see cref="Logger"/> class, Used by static log functions.
        /// </summary>
        public static readonly Logger Instance = new Logger(Console.Error, 1);

        /// <summary>
        /// One greater than the minimum priority level a logging function must give to be written to
        /// the output.
        /// </summary>
        private ushort minPriority;

        /// <summary>
        /// The log drain, where the output of logging functions goes.
        /// </summary>
        private TextWriter writer;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <param name="minPriority">The minimum priority.</param>
        /// <exception cref="ArgumentNullException">writer</exception>
        public Logger(TextWriter writer, ushort minPriority)
        {
            this.writer = writer ?? throw new ArgumentNullException(nameof(writer));
            this.minPriority = minPriority;
        }

        /// <summary>
        /// Logs the <paramref name="message"/> at error priority.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogError(string message) => Instance.WriteError(message);

        /// <summary>
        /// Logs the <paramref name="message"/> at information priority.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogInfo(string message) => Instance.WriteInfo(message);

        /// <summary>
        /// Logs the <paramref name="message"/> at verbose priority.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogVerbose(string message) => Instance.WriteVerbose(message);

        /// <summary>
        /// Logs the <paramref name="message"/> at warning priority.
        /// </summary>
        /// <param name="message">The message.</param>
        public static void LogWarning(string message) => Instance.WriteWarning(message);

        /// <summary>
        /// Redirects the the output to <paramref name="writer"/>.
        /// </summary>
        /// <param name="writer">The writer.</param>
        /// <exception cref="ArgumentNullException">writer</exception>
        public void Redirect(TextWriter writer) => this.writer = writer ?? throw new ArgumentNullException(nameof(writer));

        /// <summary>
        /// Sets the minimum log level.
        /// </summary>
        /// <param name="minPriority">The minimum priority.</param>
        public void SetMinLogLevel(ushort minPriority) => this.minPriority = minPriority;

        /// <summary>
        /// Writes the error.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteError(string message) => WriteLine(4, message);

        /// <summary>
        /// Writes the information.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteInfo(string message) => WriteLine(2, message);

        /// <summary>
        /// Writes the verbose.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteVerbose(string message) => WriteLine(1, message);

        /// <summary>
        /// Writes the warning.
        /// </summary>
        /// <param name="message">The message.</param>
        public void WriteWarning(string message) => WriteLine(3, message);

        /// <summary>
        /// Writes the line.
        /// </summary>
        /// <param name="priority">The priority.</param>
        /// <param name="message">The message.</param>
        /// <exception cref="ArgumentException">When <paramref name="priority"/> is 0</exception>
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
