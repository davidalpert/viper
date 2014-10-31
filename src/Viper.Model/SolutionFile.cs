using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Viper.Model
{
    /// <example>
    /// Microsoft Visual Studio Solution File, Format Version 12.00
    /// # Visual Studio 2012
    /// Global
    ///     GlobalSection(SolutionProperties) = preSolution
    ///         HideSolutionNode = FALSE
    ///     EndGlobalSection
    /// EndGlobal
    /// </example>
    public class SolutionFile
    {
        public SolutionFile(int fileFormatMajorVersion, int fileFormatMinorVersion, VisualStudioVersion productVersion, IEnumerable<SolutionGlobalSection> globalSections = null)
        {
            ProductVersion = productVersion;
            FormatVersion = new SolutionFileFormatVersion(fileFormatMajorVersion, fileFormatMinorVersion);

            var sectionsByName =
                (globalSections ?? Enumerable.Empty<SolutionGlobalSection>())
                    .GroupBy(s => s.Name);

            GlobalSections = new MultiValueDictionary<string, SolutionGlobalSection>(sectionsByName);
        }

        public SolutionFileFormatVersion FormatVersion { get; set; }
        public VisualStudioVersion ProductVersion { get; set; }
        public MultiValueDictionary<string, SolutionGlobalSection> GlobalSections { get; set; }
    }

    public enum VisualStudioVersion
    {
        Unrecognized,

        [Description("Visual Studio 2010")]
        VS2010,

        [Description("Visual Studio 2012")]
        VS2012,

        [Description("Visual Studio 2012")]
        VS2013
    }

    public static class VisualStudioVersionExtensions
    {
        public static string ToVersionString(this VisualStudioVersion version)
        {
            var fi = version.GetType().GetField(version.ToString());
            var attributes = (DescriptionAttribute[])fi.GetCustomAttributes(typeof(DescriptionAttribute), false);
            return (attributes.Length > 0) ? attributes[0].Description : version.ToString();
        }
    }

    public class SolutionFileFormatVersion
    {
        public SolutionFileFormatVersion(int major, int minor)
        {
            Major = major;
            Minor = minor;
        }

        public int Major { get; private set; }
        public int Minor { get; private set; }
    }

    public enum SectionLoadSequence
    {
        Unrecognized,
        PreSolution,
        PostSolution
    }

    public abstract class SolutionGlobalSection
    {
        protected SolutionGlobalSection(string name, SectionLoadSequence loadSequence)
        {
            Name = name;
            LoadSequence = loadSequence;
        }

        public string Name { get; private set; }
        public SectionLoadSequence LoadSequence { get; private set; }
    }

    public class UnrecognizedGlobalSection : SolutionGlobalSection
    {
        public UnrecognizedGlobalSection(string name, string content, SectionLoadSequence loadSequence) : base(name, loadSequence)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }
}
