using System;
using System.Diagnostics;

namespace RPINode
{
    public static class Logger
    {
        public static void Log(string message)
        {
            var method = new StackTrace().GetFrame(1)?.GetMethod()?.Name;
           Console.WriteLine($"{DateTime.Now} - {method??"<unknown>"}: {message}"); 
        }

        public static void Log(Exception e)
        {
            Console.WriteLine(e);
        }
    }
}