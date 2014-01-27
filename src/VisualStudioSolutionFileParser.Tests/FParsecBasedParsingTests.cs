using System.Linq;
using NUnit.Framework;
using VisualStudioSolutionFileParser;

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
    }
}