using System.Linq;
using NUnit.Framework;
using VisualStudioSolutionFileParser;
using VisualStudioSolutionFileParser.AST;

namespace VisualStudioSolutionFileParser.Tests
{
    public class FParsecBasedParsingTests
    {
        [Test]
        public void Header_contains_version_information()
        {
            var input =
                @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012";

            var result = Parser.Parse(input).Heading;

            Assert.AreEqual("Visual Studio 2012", result.ProductName);
            Assert.AreEqual(12, result.Version.Major);
            Assert.AreEqual(00, result.Version.Minor);
        }

        [Test]
        public void SolutionVersionNumber_is_Number_period_Number()
        {
            var input = @"12.00";

            var result = Parser.Run(Parser.fileVersion, input);

            Assert.AreEqual(12, result.Major);
            Assert.AreEqual(00, result.Minor);
        }

        [Test]
        public void ProductName_is_pound_followed_by_text()
        {
            var input = @"# Visual Studio 2012";

            var result = Parser.Run(Parser.productName, input);

            Assert.AreEqual("Visual Studio 2012", result);
        }

        [Test]
        public void RoundBracketedString_is_string_surrounded_by_round_brackets()
        {
            var input = @"(something)";

            var result = Parser.Run(Parser.roundBracketedString, input);

            Assert.AreEqual("something", result);
        }

        [Test]
        public void SolutionProperty_is_Name_equals_Value()
        {
            var input = "HideSolutionNode = FALSE";

            var result = Parser.Run(Parser.solutionProperty, input);

            Assert.AreEqual("HideSolutionNode", result.Name);
            Assert.AreEqual("FALSE", result.Value);
        }

        [Test]
        public void GlobalSectionStart_can_parse_start_of_a_global_section()
        {
            var input =
@"GlobalSection(SolutionProperties) = preSolution
    HideSolutionNode = FALSE
EndGlobalSection
";
            var result = Parser.Run(Parser.globalSectionStart,input);

            Assert.AreEqual("SolutionProperties", result.Item1);
            Assert.AreEqual(LoadSequence.PreSolution, result.Item2);
        }

        [Test]
        public void GlobalSection_can_parse_SolutionPropertiesSection()
        {
            var input =
@"GlobalSection(SolutionProperties) = preSolution 
    HideSolutionNode = FALSE
EndGlobalSection
";
            var result = Parser.Run(Parser.globalSection,input);

            Assert.IsInstanceOf(typeof(GlobalSection), result);

            var section = result as GlobalSection;
            Assert.AreEqual("SolutionProperties", section.Name);
            Assert.AreEqual(LoadSequence.PreSolution, result.LoadSequence);
            Assert.AreEqual(1, section.Properties.Length);
            Assert.AreEqual("HideSolutionNode", section.Properties[0].Name);
            Assert.AreEqual("FALSE", section.Properties[0].Value);
        }

        [Test]
        public void EmptySolutionFile_is_Header_followed_by_GlobalSections()
        {
            var input =
@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012
Global
    GlobalSection(SolutionProperties) = preSolution
        HideSolutionNode = FALSE
    EndGlobalSection
EndGlobal
";
            var result = Parser.Parse(input);

            Assert.AreEqual(12, result.Heading.Version.Major);
            Assert.AreEqual(0, result.Heading.Version.Minor);
            Assert.AreEqual("Visual Studio 2012", result.Heading.ProductName);
            Assert.AreEqual(1, result.GlobalSections.Length);
        }
    }
}