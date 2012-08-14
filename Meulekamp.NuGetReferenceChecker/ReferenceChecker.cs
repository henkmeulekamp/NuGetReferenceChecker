using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using Meulekamp.NuGetReferenceChecker.EnumarableXmlReaders;
using Meulekamp.NuGetReferenceChecker.Model;

namespace Meulekamp.NuGetReferenceChecker
{
    public class ReferenceChecker
    {
        public string SolutionFolder { get; private set; }
        private readonly SolutionLoader _solutionLoader;

        public ReferenceChecker(SolutionLoader solutionLoader)
        {
            _solutionLoader = solutionLoader;
            SolutionFolder = solutionLoader.SolutionRoot;
            if (!_solutionLoader.Projects.Any()) throw new Exception("No Projects Found");
        }

        /// <summary>
        /// Returns names of packages that are in the [solutionfolder]/packages/* but are not referenced in [projects]/packages.config
        /// </summary>
        /// <returns></returns>
        public List<string> FindAllNonUsedPackages()
        {
            IEnumerable<string> notUsed = _solutionLoader.NugetPackageFolders
                                .Select(s=>s.ToLowerInvariant())
                                .Except(_solutionLoader.Projects.SelectMany(p => p.ListPackages)
                                .Select(p => string.Format("{0}.{1}", p.id, p.version).ToLowerInvariant())
                );
            return notUsed.ToList();
        }

        public List<string> FindPackageFilesNotInRepositoryFile()
        {
            var repositoryRegisteredFiles = _solutionLoader.RepositoryPackageFiles
                                .Select(s => GetFileName(s.Replace("..\\",""), _solutionLoader.SolutionRoot).FullName.ToLowerInvariant())
                                .ToList();

            return _solutionLoader.FoundPackageFiles.Select(s=>s.ToLowerInvariant())
                .Except(repositoryRegisteredFiles)
                .ToList();
        }

        /// <summary>
        /// Returns missing package names
        /// </summary>
        /// <returns></returns>
        public List<string> FindMissingPackages()
        {
            List<string> allUsedPackages = _solutionLoader.Projects.SelectMany(p => p.ListPackages)
                .Select(p => string.Format("{0}.{1}", p.id, p.version).ToLowerInvariant()).Distinct().ToList();

            IEnumerable<string> missing = allUsedPackages.Except(_solutionLoader.NugetPackageFolders.Select(s => s.ToLowerInvariant()));

            return missing.ToList();
        }


        public List<string> FindAllPackagesWithMultipleVersions()
        {
            var grouped = from p in _solutionLoader.Projects.SelectMany(p => p.ListPackages)
                          group p by p.id
                          into pack
                          orderby pack.Key
                          select new
                                     {
                                         PackageName = pack.Key,
                                         CountVersions = pack.Select(i => i.version).Distinct().Count()
                                     };

            var packagesWithMultipleVersions = grouped.Where(g => g.CountVersions > 1).Select(g => g);

            return
                packagesWithMultipleVersions
                    .Select(g => string.Format("{0} has {1} versions:{2}{3}",
                                               g.PackageName,
                                               g.CountVersions,
                                               Environment.NewLine,
                                               GetProjectsAsString(g.PackageName, _solutionLoader.Projects)))
                    .ToList();
        }

        private static string GetProjectsAsString(string packageName, IEnumerable<VsProject> projects)
        {
            var sb = new StringBuilder();

            var projectsWithPackage =
                projects.Where(
                    p => p.ListPackages.Any(pf => pf.id.Equals(packageName, StringComparison.OrdinalIgnoreCase)))
                    .Select(
                        p =>
                        new
                            {
                                p.ProjectFile,
                                p.ListPackages.FirstOrDefault(pf => pf.id.Equals(packageName, StringComparison.OrdinalIgnoreCase)).version
                            });

            foreach (var project in  projectsWithPackage)
            {
                sb.AppendLine(string.Format("Project {0} has version {1}", project.ProjectFile, project.version));
            }

            return sb.ToString();
        }


        public List<string> FindWronglyReferencedNugetAssemblies()
        {
            var allErrors = new List<string>();
            //collect all assemblies form used nuget folders
            IEnumerable<string> allNugetPackageAssemblies = GetUsedNuGetAllAssemblies();

            List<FileInfo> filesOfNuGetAssemblies = allNugetPackageAssemblies.Select(f => new FileInfo(f)).ToList();

            foreach (VsProject project in _solutionLoader.Projects)
            {
                if (!File.Exists(project.ProjectFile))
                {
                    Trace.WriteLine(string.Format("Skipping, no project file found {0}", project.ProjectFile));
                    continue; 
                }   

                List<Reference> references;
                using (var xmlReader = new EnumerableXmlReader<Reference>(new NamespaceIgnorantXmlTextReader(File.OpenText(project.ProjectFile))))
                {
                    references = xmlReader.Stream().Where(r => !string.IsNullOrWhiteSpace(r.HintPath)).ToList();
                }

                IEnumerable<string> errors = CheckReferences(project, references, filesOfNuGetAssemblies);

                if (errors.Any()) allErrors.AddRange(errors);
            }

            return allErrors;
        }

        private IEnumerable<string> CheckReferences(VsProject project, IEnumerable<Reference> references, 
                                             IEnumerable<FileInfo> filesOfNuGetAssemblies)
        {
            var packageFolder = new FileInfo(_solutionLoader.NuGetRepositoryFile).Directory.FullName.ToLowerInvariant();

            List<FileInfo> filesOfReferencesInProject =
                references.Select(r => GetFileName(r.HintPath, project.ProjectPath)).ToList();
            
            var files = from inP in filesOfReferencesInProject
                        join inN in filesOfNuGetAssemblies on inP.Name.ToLowerInvariant() equals
                            inN.Name.ToLowerInvariant()
                        where !ComparerNuGetAssemblyPath(inP.FullName.ToLowerInvariant(), packageFolder)//>!inN.FullName.Equals(inP.FullName)
                        select new
                                   {
                                       ProjectReference = inP.FullName,
                                       NuGetPackage = inN.DirectoryName
                                   };

            return files.Select(f => string.Format("Project {0} links to {1} instead of to nuget package {2}",
                                                   project.ProjectFile.ToLowerInvariant().Replace(_solutionLoader.SolutionRoot.ToLowerInvariant(), ""),
                                                   f.ProjectReference.ToLowerInvariant().Replace(_solutionLoader.SolutionRoot.ToLowerInvariant(), ""),
                                                   GetPackageNameFromPath(f.NuGetPackage, packageFolder))).ToList();
        }

        private static bool ComparerNuGetAssemblyPath(string projectReference, string packageFolder)
        {
            //if reference is pointing to nuget assemblies then we are good; 
            //FindAllPackagesWithMultipleVersions should find the version mismatches
            return (projectReference.Contains(packageFolder));
        }

        private static string GetPackageNameFromPath(string path, string packageFolder)
        {
            var subPathpProjectReference = path.ToLowerInvariant().Replace(packageFolder, "");

            var indexOfFirstFolder = subPathpProjectReference.IndexOf("\\",1);

            var nugetPackageProject = subPathpProjectReference.Substring(0, indexOfFirstFolder);

            return nugetPackageProject.ToLowerInvariant();
        }

        private static FileInfo GetFileName(string filePath, string projectpath)
        {
            return new FileInfo(Path.GetFullPath(Path.Combine(projectpath, filePath)));
        }

        private IEnumerable<string> GetUsedNuGetAllAssemblies()
        {
            var allAssemblies = new List<string>();

            List<string> nugetFoldersInUse = _solutionLoader.Projects.SelectMany(p => p.ListPackages)
                .Select(p => string.Format("{0}.{1}", p.id, p.version)).ToList();

            foreach (
                DirectoryInfo d in
                    new FileInfo(_solutionLoader.NuGetRepositoryFile).Directory.GetDirectories().Where(
                        sd => nugetFoldersInUse.Contains(sd.Name, StringComparer.InvariantCultureIgnoreCase)))
            {
                string[] files = Directory.GetFiles(d.FullName, "*.dll", SearchOption.AllDirectories);

                if (files.Any())
                    allAssemblies.AddRange(files);
            }

            return allAssemblies;
        }
    }
}