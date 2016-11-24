using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RemoveUnusedUsings
{
    internal static class Program
    {
        private const string solutionKey = "solution";
        private const string verboseKey = "verbose";

        [STAThread]
        private static void Main(string[] args)
        {
            var keyValues = args.Select(x => x.Split('=')).ToArray();
            if (args.Length == 0 || keyValues.Any(x => x.Length != 2))
            {
                Console.WriteLine($"usage: removeUnusedUsings.exe {solutionKey}=\"C:\\folder\\solution.sln\" [{verboseKey}=quiet|detail]");
                return;
            }

            var dictionary = BuildParametersDictionary(keyValues);

            var solutionPath = dictionary[solutionKey];
            var logLevel = dictionary.ContainsKey(verboseKey)
                ? (dictionary[verboseKey] == "quiet" ? LogLevel.Quiet : LogLevel.Detail)
                : LogLevel.Detail;

            ILogger logger = new Logger(logLevel);
            IOrganizer organizer = new RoslynOrganizer(logger);

            var stopwatch = new Stopwatch();
            stopwatch.Start();
            organizer.OrganizeSolution(solutionPath);
            stopwatch.Stop();
            logger.Log($"Elasped: {stopwatch.ElapsedMilliseconds} ms");
            Console.ReadKey();
        }

        private static Dictionary<string, string> BuildParametersDictionary(string[][] keyValues)
        {
            try
            {
                var dictionary =  keyValues.ToDictionary(x => x[0], x => x[1]);
                if (dictionary.ContainsKey(solutionKey))
                {
                    return dictionary;
                }

                Console.WriteLine($"Args must contains '{solutionKey}' key");
                return null;
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Please set key before equals sign");
                return null;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Please remove key duplicates");
                return null;
            }
        }
    }
}
