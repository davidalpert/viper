using System;
using System.Linq;
using NUnit.Framework;
using VisualStudioSolutionFileParser;
using VisualStudioSolutionFileParser.AST;

namespace VisualStudioSolutionFileParser.Tests
{
    public class FParsecBasedParsingTests
    {
        [Test]
        public void Header()
        {
            var input = @"Microsoft Visual Studio Solution File, Format Version";

            var result = Parser.Run(Parser.header, input);

            Assert.IsAssignableFrom(typeof(string), result);
            Assert.AreEqual(input, result);
        }

        [Test]
        public void FileVersion_is_Number_period_Number()
        {
            var input = @"12.00";

            var result = Parser.Run(Parser.fileVersion, input);

            Assert.IsAssignableFrom(typeof(AST.FileVersion), result);
            Assert.AreEqual(12, result.Item1);
            Assert.AreEqual(00, result.Item2);
        }

        [Test]
        public void ProductName_is_pound_followed_by_text()
        {
            var input = @"# Visual Studio 2012";

            var result = Parser.Run(Parser.productName, input);

            Assert.IsAssignableFrom(typeof(string), result);
            Assert.AreEqual("Visual Studio 2012", result);
        }

        [Test]
        public void FileHeading_is_header_version_pound_product_name()
        {
            var input =
                @"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012";

            var result = Parser.Parse(input).Heading;

            Assert.IsAssignableFrom(typeof (AST.FileHeading), result);
            Assert.AreEqual("Visual Studio 2012", result.Item1);
            Assert.AreEqual(12, result.Item2.Item1);
            Assert.AreEqual(00, result.Item2.Item2);
        }

        [Test]
        public void GlobalSection_can_parse_an_unrecognized_section()
        {
            var input =
@"GlobalSection(SomeNewSection) = preSolution
    HideSolutionNode = FALSE
EndGlobalSection
";
            var result = Parser.Run(Parser.globalSection,input);

            Assert.IsAssignableFrom(typeof(AST.GlobalSection.UnrecognizedGlobalSection), result);

            var unrecognizedSection = result as AST.GlobalSection.UnrecognizedGlobalSection;
            Assert.IsNotNull(unrecognizedSection);
            Assert.AreEqual("SomeNewSection", unrecognizedSection.Item1);
            Assert.AreEqual(LoadSequence.PreSolution, unrecognizedSection.Item2);
            Assert.AreEqual("    HideSolutionNode = FALSE\n", unrecognizedSection.Item3.Value);
        }

        [Test]
        public void SolutionProperty_is_Name_equals_Value()
        {
            var input = "HideSolutionNode = FALSE";

            var result = Parser.Run(Parser.solutionProperty, input);

            Assert.IsAssignableFrom(typeof(AST.SolutionProperty), result);
            Assert.AreEqual("HideSolutionNode", result.Item1);
            Assert.AreEqual("FALSE", result.Item2);
        }

        [Test]
        public void GlobalSection_can_parse_SolutionPropertiesNode()
        {
            var input =
@"GlobalSection(SolutionProperties) = preSolution 
    HideSolutionNode = FALSE
EndGlobalSection
";
            var result = Parser.Run(Parser.globalSection,input);

            Assert.IsInstanceOf(typeof(AST.GlobalSection.SolutionPropertiesNode), result);

            var section = result as AST.GlobalSection.SolutionPropertiesNode;
            Assert.IsNotNull(section);
            Assert.AreEqual(LoadSequence.PreSolution, section.Item1);
            Assert.AreEqual(1, section.Item2.Length);
        }

        /*
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

            //Assert.AreEqual(12, result.Heading.Version.Major);
            //Assert.AreEqual(0, result.Heading.Version.Minor);
            //Assert.AreEqual("Visual Studio 2012", result.Heading.ProductName);
            //Assert.AreEqual(1, result.GlobalSections.Length);
        }

        [Test]
        public void Guid_is_32_hex_with_hypens()
        {
            var input = "64A5EC94-C0AA-4BC1-8009-A214999C08B8";
            var expected = Guid.Parse("{" + input + "}");

            var result = Parser.Run(Parser.guid, input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ProjectNode_is_type_name_path_guid()
        {
            var input = @"
Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""HttpWebAdapters"", ""HttpWebAdapters\HttpWebAdapters.csproj"", ""{AE7D2A46-3F67-4986-B04B-7DCE79A549A5}""
EndProject
";
            var result = Parser.Run(Parser.projectNode, input);
        }
         */
    }
}