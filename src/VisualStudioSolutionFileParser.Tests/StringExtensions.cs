using System;
using System.Collections.Generic;
using System.Text;
using Sprache;
using VisualStudioSolutionFileParser.Tests.Helpers;

namespace VisualStudioSolutionFileParser.Tests
{
    internal static class StringExtensions
    {
        internal static string NormalizeLineEndings(this string input)
        {
            return input.Replace(@"
", "\n");
        }
    }
}
