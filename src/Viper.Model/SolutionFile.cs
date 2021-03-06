﻿using System;
using System.Collections;
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
        public SolutionFile(int fileFormatMajorVersion, int fileFormatMinorVersion, VisualStudioVersion productVersion, IEnumerable<IGlobalSection> globalSections = null)
        {
            ProductVersion = productVersion;
            FormatVersion = new SolutionFileFormatVersion(fileFormatMajorVersion, fileFormatMinorVersion);

            var sectionsByName =
                (globalSections ?? Enumerable.Empty<AbstractGlobalSection>())
                    .GroupBy(s => s.Name);

            GlobalSections = new MultiValueDictionary<string, IGlobalSection>(sectionsByName);
        }

        public SolutionFileFormatVersion FormatVersion { get; set; }
        public VisualStudioVersion ProductVersion { get; set; }
        public MultiValueDictionary<string, IGlobalSection> GlobalSections { get; set; }
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

    public interface IGlobalSection
    {
        string Name { get; }
        SectionLoadSequence LoadSequence { get; }
    }

    public abstract class AbstractGlobalSection : IGlobalSection
    {
        protected AbstractGlobalSection(string name, SectionLoadSequence loadSequence)
        {
            Name = name;
            LoadSequence = loadSequence;
        }

        public string Name { get; private set; }
        public SectionLoadSequence LoadSequence { get; private set; }
    }

    public class UnrecognizedGlobalSection : AbstractGlobalSection
    {
        public UnrecognizedGlobalSection(string name, string content, SectionLoadSequence loadSequence) 
            : base(name, loadSequence)
        {
            Content = content;
        }

        public string Content { get; private set; }
    }

    public class DictionaryBasedGlobalSection : Dictionary<string,string>, IGlobalSection
    {
        public DictionaryBasedGlobalSection(string name, SectionLoadSequence loadSequence, IDictionary<string,string> values = null)
            : base(values ?? Enumerable.Empty<Tuple<string,string>>().ToDictionary(x => x.Item1, x => x.Item2))
        {
            Name = name;
            LoadSequence = loadSequence;
        }

        public string Name { get; private set; }
        public SectionLoadSequence LoadSequence { get; private set; }
    }

    public class SolutionPropertiesGlobalSection : DictionaryBasedGlobalSection
    {
        public SolutionPropertiesGlobalSection(string name, SectionLoadSequence loadSequence, IDictionary<string, string> values = null)
            : base(name, loadSequence, values)
        {
        }

        public bool? HideSolutionNode
        {
            get
            {
                return ContainsKey("HideSolutionNode")
                           ? this["HideSolutionNode"] == "TRUE"
                           : (bool?) null;
            }
            set
            {
                if (value == null)
                {
                    if (ContainsKey("HideSolutionNode"))
                        Remove("HideSolutionNode");
                }
                else
                {
                    this["HideSolutionNode"] = value.Value ? "TRUE" : "FALSE";
                }
            }
        }
    }
}
