using System.Collections.Generic;
using System.Diagnostics;
using NUnit.Framework;

namespace Meulekamp.NuGetReferenceChecker.UnitTests
{
    [TestFixture]
    public class ReferenceCheckerTests
    {
        private ReferenceChecker _checker;

        [TestFixtureSetUp]
        public void SetupFixture()
        {
           //_checker = new ReferenceChecker(new SolutionLoader(true, 
           //                  @"C:\Projects\SolutionFolderA\trunk"));
           _checker = new ReferenceChecker(new SolutionLoader());
        }

        [Test]
        public void FindAllNonUsedPackages()
        {
            List<string> errors = _checker.FindAllNonUsedPackages();
            errors.ForEach(s => Trace.WriteLine(s));
            Assert.IsEmpty(errors);
        }

        [Test]
        public void FindAllPackagesWithMultipleVersions()
        {
            List<string> errors = _checker.FindAllPackagesWithMultipleVersions();
            errors.ForEach(s => Trace.WriteLine(s));
            Assert.IsEmpty(errors);
        }

        [Test]
        public void FindMissingPackages()
        {
            List<string> errors = _checker.FindMissingPackages();
            errors.ForEach(s => Trace.WriteLine(s));
            Assert.IsEmpty(errors);
        }

        [Test]
        public void FindPackageFilesNotInRepositoryFile()
        {
            List<string> errors = _checker.FindPackageFilesNotInRepositoryFile();
            errors.ForEach(s => Trace.WriteLine(s));
            Assert.IsEmpty(errors);
        }

        [Test]
        public void FindWronglyReferencedNugetAssemblies()
        {
            List<string> errors = _checker.FindWronglyReferencedNugetAssemblies();
            errors.ForEach(s => Trace.WriteLine(s));
            Assert.IsEmpty(errors);
        }
    }
}