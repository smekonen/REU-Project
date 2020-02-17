using System;
using System.Collections.Generic;
using System.Text;
using System.Diagnostics;

namespace StationCustomVisionApp
{
    
    public static class Benchmark
    {
        public static Dictionary<string, Stopwatch> Timers = new Dictionary<string, Stopwatch>()
        {
            {"Transform", new Stopwatch() },
            {"Compress", new Stopwatch() },
            {"Classify", new Stopwatch() }
        };


        public static string GetTimes()
        {
            string output = "";

            foreach(var tKey in Timers.Keys)
            {
                output += $"{tKey + ":",-10}{Timers[tKey].ElapsedMilliseconds,4} ms\n";
            }

            return output;
        }
    }
}
