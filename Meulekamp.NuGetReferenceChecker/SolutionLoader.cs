using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using Meulekamp.NuGetReferenceChecker.EnumarableXmlReaders;
using Meulekamp.NuGetReferenceChecker.Model;

namespace Meulekamp.NuGetReferenceChecker
{
    public class SolutionLoader
    {
        private readonly bool _useTraceWriter;
        private readonly List<string> _ignorePaths;
        public List<string> FoundPackageFiles;
        public string NuGetRepositoryFile;
        public List<string> NugetPackageFolders;
        public List<string> RepositoryPackageFiles;
        public string SolutionRoot;


        public SolutionLoader(bool useTraceWriter = false, string solutionFolder = null, List<string> ignorePaths = null)
        {
            _useTraceWriter = useTraceWriter;
            _ignorePaths = ignorePaths;
            Projects = new List<VsProject>();

            CollectAll(solutionFolder);
        }

        public List<VsProject> Projects { get; private set; }

        public void CollectAll(string solutionFolder = null)
        {
            var fileFinder = new FileFinder(Environment.CurrentDirectory);

            string solutionPath = solutionFolder ?? fileFinder.FindSolutionFolder();

            SolutionRoot = solutionPath;

            LogWrite(string.Format("FindSolutionFolder: {0}", solutionPath));

            //get solution repository file and get all package files
            NuGetRepositoryFile = fileFinder.FindNuGetRepositoryInSolution(solutionPath);
            using (var xmlReader = new EnumerableXmlReader<repository>(NuGetRepositoryFile))
            {
                RepositoryPackageFiles = xmlReader.Stream().Select(r => r.path).Where(f=>!IsInIgnorePath(f)).ToList();
            }

            LogWrite(string.Format("Packages.config found: {0}",
                                   string.Join(Environment.NewLine, RepositoryPackageFiles)));

            //get all packages in nuget package folder
            NugetPackageFolders =
                new FileInfo(NuGetRepositoryFile).Directory.GetDirectories().Select(d => d.Name).ToList();

            LogWrite(string.Format("NuGet Packages found: {0}", string.Join(", ", NugetPackageFolders)));

            //do a search for all package files
            FoundPackageFiles = fileFinder.FindPackageFilesInSolution(solutionPath).Where(f => !IsInIgnorePath(f)).ToList();

            LogWrite(string.Format("FindPackageFilesInSolution: {0}",
                                   string.Join(Environment.NewLine, FoundPackageFiles)));

            //add projects with nuget project package files
            foreach (string packagefile in FoundPackageFiles)
            {
                List<package> nugetPackages;
                using (var xmlReader = new EnumerableXmlReader<package>(packagefile))
                {
                    nugetPackages = xmlReader.Stream().ToList();
                }
                string projectPath = new FileInfo(packagefile).Directory.FullName;
                var project = new VsProject
                                  {
                                      PackageFile = packagefile,
                                      ListPackages = nugetPackages,
                                      ProjectPath = projectPath,
                                      ProjectFile = fileFinder.FindProjectFilesInSolution(projectPath).FirstOrDefault() ?? projectPath
                                  };
                Projects.Add(project);

                LogWrite(string.Format("Project with nuget packages: {0}; Packages:\n{1}",
                                       project.ProjectFile,
                                       string.Join(Environment.NewLine,
                                                   project.ListPackages.Select(
                                                       p => string.Format("\t{0}.{1}", p.id, p.version)))));
            }
            
            //add projects not having nuget package files 
            var projectFiles = fileFinder.FindProjectFilesInSolution(solutionPath) ?? new string[]{};
            foreach (string projectFile in projectFiles.Where(pf => !Projects.Any(p => p.ProjectFile.Equals(pf, StringComparison.OrdinalIgnoreCase))))
            {
                Projects.Add(
                    new VsProject
                        {
                            PackageFile = null,
                            ListPackages = new List<package>(0),
                            ProjectPath = new FileInfo(projectFile).Directory.FullName,
                            ProjectFile = projectFile
                        }
                    );
            }

            //filter projects by ignorepaths
            Projects = Projects.Where(p => !IsInIgnorePath(p.ProjectFile)).ToList();

            LogWrite(string.Format("Total Projects found {0}:\n{1}", Projects.Count,
                                   string.Join(Environment.NewLine, Projects.Select(p => p.ProjectFile))));
        }

        private void LogWrite(string message)
        {
            if (!_useTraceWriter) return;

            Trace.WriteLine(message);
        }

        private bool IsInIgnorePath(string folder)
        {
            if (_ignorePaths == null || !_ignorePaths.Any()) return false;

            foreach(var ignoreFolder in _ignorePaths)
            {
                var fullPath = Directory.Exists(ignoreFolder)
                    ? ignoreFolder
                    : Path.GetFullPath(Path.Combine(SolutionRoot, ignoreFolder));

                if (folder.StartsWith(fullPath, StringComparison.OrdinalIgnoreCase))
                    return true;
            }
            return false;
        }
    }
}