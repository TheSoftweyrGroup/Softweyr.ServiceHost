// -----------------------------------------------------------------------
// <copyright file="DefaultLogger.cs" company="Access UK Ltd">
// TODO: Update copyright text.
// </copyright>
// -----------------------------------------------------------------------

namespace Softweyr.Logging
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    /// <summary>
    /// TODO: Update summary.
    /// </summary>
    public class DefaultLogger : ILogger
    {
        private void WriteToConsole(ConsoleColor color, string type, string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(type + ":" + message);
            Console.ForegroundColor = previousColor;
        }

        public void Trace(string message)
        {
            WriteToConsole(ConsoleColor.Gray, "Trace", message);
        }

        public void Debug(string message)
        {
            WriteToConsole(ConsoleColor.White, "Debug", message);
        }

        public void Information(string message)
        {
            WriteToConsole(ConsoleColor.Green, "Information", message);
        }

        public void Warning(string message)
        {
            WriteToConsole(ConsoleColor.Yellow, "Warning", message);
        }

        public void Error(string message)
        {
            WriteToConsole(ConsoleColor.Red, "Error", message);
        }

        public void Fatal(string message)
        {
            WriteToConsole(ConsoleColor.DarkRed, "Fatal", message);
        }

        public void Trace<TTarget>(string message)
        {
            Trace(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public void Debug<TTarget>(string message)
        {
            Debug(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public void Information<TTarget>(string message)
        {
            Information(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public void Warning<TTarget>(string message, Exception ex = null)
        {
            Warning(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public void Error<TTarget>(string message, Exception ex = null)
        {
            Error(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public void Fatal<TTarget>(string message, Exception ex = null)
        {
            Fatal(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }
    }
}
