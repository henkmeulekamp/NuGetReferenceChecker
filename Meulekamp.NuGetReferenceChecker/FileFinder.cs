using System;
using System.IO;
using System.Linq;

namespace Meulekamp.NuGetReferenceChecker
{
    internal class FileFinder
    {
        private readonly string _startDirectory;
        private readonly int _maxNestedLevel;

        public FileFinder(string path, int maxNestedLevel = 4)
        {
            if (string.IsNullOrWhiteSpace(path) || !Directory.Exists(path))
            {
                throw new ArgumentException(string.Format("FileFinder cannot find path {0}", path));
            }

            _startDirectory = path;
            _maxNestedLevel = maxNestedLevel;
        }

        public string FindSolutionFolder()
        {           
            var dir = new DirectoryInfo(_startDirectory);
            var files = dir.GetFiles("*.sln");            
            int upLevel = 0;

            while (upLevel <= _maxNestedLevel && !files.Any())
            {
                dir = dir.Parent;
                
                if (dir == null) break;

                files = dir.GetFiles("*.sln");                
                upLevel++;
            }

            if (files.Any() && dir != null)
                return dir.FullName;

            return null;
        }

        public string FindNuGetRepositoryInSolution(string solutionPath)
        {
            var repoFiles = Directory.GetFiles(solutionPath, "repositories.config", SearchOption.AllDirectories);

            //if we find multiples we take the one with the shortest path
            return repoFiles.OrderBy(r=>r.Length).FirstOrDefault();
        }

        public string[] FindPackageFilesInSolution(string solutionPath)
        {
            return Directory.GetFiles(solutionPath, "packages.config", SearchOption.AllDirectories);
        }

        public string[] FindProjectFilesInSolution(string solutionPath)
        {
            return Directory.GetFiles(solutionPath, "*.csproj", SearchOption.AllDirectories);
        }
    }
}