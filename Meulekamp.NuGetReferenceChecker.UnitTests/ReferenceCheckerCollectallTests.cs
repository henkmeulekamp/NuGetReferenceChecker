using NUnit.Framework;

namespace Meulekamp.NuGetReferenceChecker.UnitTests
{
    [TestFixture]
    public class SolutionLoaderTests
    {

        [Test]
        public void CollectAll()
        {
            var checker = new SolutionLoader(true);
            
            Assert.IsNotEmpty(checker.Projects);
        }
    }
}