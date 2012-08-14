using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using CommandLine;
using Meulekamp.NuGetReferenceChecker;

namespace NuGetReferenceCheck
{
    class Program
    {
        static int Main(string[] args)
        {
            var config = GetRunOptions(args);

            if (config == null) return -2;

            var stopwatch = new Stopwatch();

            stopwatch.Start();

            List<string> ignorefolders = null;
            if(!string.IsNullOrWhiteSpace(config.IgnoreFolders))
            {
                ignorefolders = config.IgnoreFolders.Split(' ').ToList();
            }

            var checker = new ReferenceChecker(new SolutionLoader(config.Verbose, config.SolutionFolder, ignorefolders));

            Console.WriteLine(string.Format("Starting checker in SolutionFolder {0}",
                                            checker.SolutionFolder));

            var listOfErrors = new Dictionary<string, List<string>>();

            listOfErrors.Add("FindAllNonUsedPackages", checker.FindAllNonUsedPackages());
            listOfErrors.Add("FindAllPackagesWithMultipleVersions", checker.FindAllPackagesWithMultipleVersions());
            listOfErrors.Add("FindMissingPackages", checker.FindMissingPackages());
            listOfErrors.Add("FindPackageFilesNotInRepositoryFile", checker.FindPackageFilesNotInRepositoryFile());
            listOfErrors.Add("FindWronglyReferencedNugetAssemblies", checker.FindWronglyReferencedNugetAssemblies());
            stopwatch.Stop();

            int numberOfIssues = listOfErrors.Select(s => s.Value.Count).Sum();

            Console.WriteLine(string.Format("NuGetReferenceCheck done, found {0} issues in {1} milliseconds", 
                                             numberOfIssues, stopwatch.ElapsedMilliseconds));

            if (numberOfIssues == 0) return 0;

            foreach (var issues in listOfErrors.Where(i=>i.Value.Any()))
            {
                if (issues.Value.Any())
                {
                    Console.WriteLine();
                    Console.WriteLine(string.Format("Check: {0}", issues.Key));
                    issues.Value.ForEach(e=> Console.WriteLine(string.Format("   -{0}", e)));
                }
            }            
            return -1;
        }

       
        private static RunOptions GetRunOptions(string[] args)
        {
            if(args==null || args.Length == 0)
            {
                return new RunOptions();
            }
            var options = new RunOptions();
            if (CommandLineParser.Default.ParseArguments(args, options))
                return options;

            return null;
        }
    }

    //Use commandline parser:https://github.com/gsscoder/commandline/wiki/Quickstart
    public class RunOptions
    {
        [Option("v","verbose", Required = false, HelpText = "Verbose tracing")]//not yet implemented;-)
        public bool Verbose { get; set; }
        [Option("p", "path", Required = false, HelpText = "SolutionFolderPath to scan")]
        public string SolutionFolder { get; set; }

        [Option("i", "ignore", Required = false, HelpText = "Ignore folders, foldername in solution root, separated by space")] 
        public string IgnoreFolders { get; set; }      

        [HelpOption]
        public string GetUsage()
        {
            // this without using CommandLine.Text
            var usage = new StringBuilder();
            usage.AppendLine("Quickstart");
            usage.AppendLine("Read project wiki for usage instructions...");
            return usage.ToString();
        }
    }

    
}
