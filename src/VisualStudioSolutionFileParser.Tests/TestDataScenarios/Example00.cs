using System.Linq;
using ApprovalTests;
using NUnit.Framework;
using VisualStudioSolutionFileParser.AST;
using VisualStudioSolutionFileParser.Tests.Helpers;

namespace VisualStudioSolutionFileParser.Tests.TestDataScenarios
{
    public class Example00
    {
        public SolutionFile ParseTestData()
        {
            var input = ManifestResourceHelper.ExtractManifestResourceToString("TestData.Example.00.sln");
            return Parser.Parse(input);
        }

        [Test]
        public void CanParse()
        {
            var result = ParseTestData();

            Assert.NotNull(result);
        }

        [Test]
        [Ignore("deferred until I settle on a method to pretty print the AST")]
        public void CanPrint()
        {
            var result = ParseTestData();

            Approvals.Verify(result);
        }

        [Test]
        public void Parsed_three_global_sections()
        {
            var result = ParseTestData();

            Assert.AreEqual(3, result.Item3.Length);
        }
    }
}