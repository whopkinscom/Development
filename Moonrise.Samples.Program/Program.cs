using System;
using System.Diagnostics;
using Moonrise.Logging;

namespace Moonrise
{
    class Program
    {
        static void Main(string[] args)
        {
            Stopwatch watch = Stopwatch.StartNew();

            for (int i = 0; i < 1000; i++)
            {
                Moonrise.Samples.Run();
            }

            watch.Stop();
            Logger.Fatal($"That took {watch.Elapsed}");
            Logger.Flush();
            Console.ReadKey();
        }
    }
}