﻿using System.Linq;
using ApprovalTests;
using NUnit.Framework;
using VisualStudioSolutionFileParser.AST;
using VisualStudioSolutionFileParser.Tests.Helpers;

namespace VisualStudioSolutionFileParser.Tests.TestDataScenarios
{
    public class EmptySolution
    {
        public SolutionFile ParseTestData()
        {
            var input = ManifestResourceHelper.ExtractManifestResourceToString("TestData.EmptySolution.sln");
            return Parser.Parse(input);
        }

        [Test]
        public void CanParse()
        {
            var result = ParseTestData();

            Assert.NotNull(result);
        }

        [Test]
        public void CanPrint()
        {
            var result = ParseTestData();

            Approvals.Verify(result);
        }
    }
}