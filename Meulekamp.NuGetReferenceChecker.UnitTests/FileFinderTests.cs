using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using NUnit.Framework;

namespace Meulekamp.NuGetReferenceChecker.UnitTests
{
    [TestFixture]
    public class FileFinderTests
    {
        [Test]
        public void FindSolutionFolder()
        {
            var finder = new FileFinder(Environment.CurrentDirectory);

            var solutionFolder = finder.FindSolutionFolder();
            
            Trace.WriteLine(solutionFolder);
            Assert.IsNotNull(solutionFolder);
        }

        [Test]
        public void FindSolutionFolderAndNotSln()
        {
            var finder = new FileFinder(Environment.CurrentDirectory);

            var solutionFolder = finder.FindSolutionFolder();

            Assert.That(solutionFolder, !Contains.Substring(".sln"));
        }

        [Test]
        public void FindPackageFiles()
        {
            var finder = new FileFinder(Environment.CurrentDirectory);

            var solutionFolder = finder.FindSolutionFolder();

            var packageFiles = finder.FindPackageFilesInSolution(solutionFolder);

            Assert.Greater(packageFiles.Count(), 0);
        }

        [Test]
        public void FindProjectFiles()
        {
            var finder = new FileFinder(Environment.CurrentDirectory);

            var solutionFolder = finder.FindSolutionFolder();

            var packageFiles = finder.FindProjectFilesInSolution(solutionFolder);

            Assert.Greater(packageFiles.Count(), 0);
        }

        [Test]
        public void FindNuGetRepositoryInSolution()
        {
            var finder = new FileFinder(Environment.CurrentDirectory);

            var solutionFolder = finder.FindSolutionFolder();

            var repositoryFile = finder.FindNuGetRepositoryInSolution(solutionFolder);

            Assert.IsTrue(File.Exists(repositoryFile));
        }
    }
}
