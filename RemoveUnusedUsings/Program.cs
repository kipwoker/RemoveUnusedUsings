using System;
using System.Collections.Generic;
using System.Linq;

namespace RemoveUnusedUsings
{
    static class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            const string solutionKey = "solution";
            const string verboseKey = "verbose";

            var keyValues = args.Select(x => x.Split('=')).ToArray();
            if (args.Length == 0 || keyValues.Any(x => x.Length != 2))
            {
                Console.WriteLine($"usage: removeUnusedUsings.exe {solutionKey}=\"C:\\folder\\solution.sln\" [{verboseKey}=quiet|detail]");
                return;
            }


            Dictionary<string, string> dictionary;
            try
            {
                dictionary = keyValues.ToDictionary(x => x[0], x => x[1]);
            }
            catch (ArgumentNullException)
            {
                Console.WriteLine("Please set key before equals sign");
                return;
            }
            catch (ArgumentException)
            {
                Console.WriteLine("Please remove key duplicates");
                return;
            }

            if (!dictionary.ContainsKey(solutionKey))
            {
                Console.WriteLine($"Args must contains '{solutionKey}' key");
                return;
            }

            var solutionPath = dictionary[solutionKey];
            var logLevel = dictionary.ContainsKey(verboseKey)
                ? (dictionary[verboseKey] == "quiet" ? LogLevel.Quiet : LogLevel.Detail)
                : LogLevel.Detail;

            var organizer = new Organizer(new Logger(logLevel));
            organizer.OrganizeSolution(solutionPath);
        }
    }
}
