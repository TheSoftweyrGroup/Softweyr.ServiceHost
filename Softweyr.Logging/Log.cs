using System;
using System.Collections.Generic;
using System.Linq;
using Softweyr.Logging;

namespace Softweyr.Logging
{
    public interface ILogger
    {
        void Trace(string message);

        void Debug(string message);

        void Information(string message);

        void Warning(string message);

        void Error(string message);

        void Fatal(string message);

        void Trace<TTarget>(string message);

        void Debug<TTarget>(string message);

        void Information<TTarget>(string message);

        void Warning<TTarget>(string message, Exception ex = null);

        void Error<TTarget>(string message, Exception ex = null);

        void Fatal<TTarget>(string message, Exception ex = null);
    }

    public static class Log
    {
        public static ILogger logger = new DefaultLogger();

        private static void WriteToConsole(ConsoleColor color, string type, string message)
        {
            var previousColor = Console.ForegroundColor;
            Console.ForegroundColor = color;
            Console.WriteLine(type + ":" + message);
            Console.ForegroundColor = previousColor;
        }

        public static void Trace(string message)
        {
            WriteToConsole(ConsoleColor.Gray, "Trace", message);
        }

        public static void Debug(string message)
        {
            WriteToConsole(ConsoleColor.White, "Debug", message);
        }

        public static void Information(string message)
        {
            WriteToConsole(ConsoleColor.Green, "Information", message);
        }

        public static void Warning(string message)
        {
            WriteToConsole(ConsoleColor.Yellow, "Warning", message);
        }

        public static void Error(string message)
        {
            WriteToConsole(ConsoleColor.Red, "Error", message);
        }

        public static void Fatal(string message)
        {
            WriteToConsole(ConsoleColor.DarkRed, "Fatal", message);
        }

        public static void Trace<TTarget>(string message)
        {
            Trace(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public static void Debug<TTarget>(string message)
        {
            Debug(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public static void Information<TTarget>(string message)
        {
            Information(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public static void Warning<TTarget>(string message, Exception ex = null)
        {
            Warning(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public static void Error<TTarget>(string message, Exception ex = null)
        {
            Error(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }

        public static void Fatal<TTarget>(string message, Exception ex = null)
        {
            Fatal(string.Format("{0} :: {1}", typeof(TTarget).FullName, message));
        }
    }
}