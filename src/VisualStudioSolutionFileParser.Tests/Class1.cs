using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Sprache;
using VisualStudioSolutionFileParser.Tests.Helpers;

namespace VisualStudioSolutionFileParser.Tests
{
	[TestFixture]
	public class Class1
	{
		[Test]
		public void Header_contains_version_information()
		{
			var input =
@"Microsoft Visual Studio Solution File, Format Version 12.00
# Visual Studio 2012";

			var result = SolutionFileGrammar.Header.Parse(input);

			Assert.AreEqual("Visual Studio 2012", result.ProductName);
			Assert.AreEqual(12, result.MajorVersion);
			Assert.AreEqual(00, result.MinorVersion);
		}

		[Test]
		public void SolutionVersionNumber_is_Number_period_Number()
		{
			var input = @"12.00";

			var result = SolutionFileGrammar.SolutionVersionNumber.Parse(input);

			Assert.AreEqual(12, result.Major);
			Assert.AreEqual(00, result.Minor);
		}

		[Test]
		public void ProductName_is_pound_followed_by_text()
		{
			var input = @"# Visual Studio 2012";

			var result = SolutionFileGrammar.ProductName.Parse(input);

			Assert.AreEqual("Visual Studio 2012", result);
		}

		[Test]
		public void RoundBracketedString_is_string_surrounded_by_round_brackets()
		{
			var input = @"(something)";

			var result = SolutionFileGrammar.RoundBracketedString.Parse(input);

			Assert.AreEqual("something", result);
		}

        [Test]
        public void LoadSequence_is_a_recognized_SectionLoadSequence_value()
        {
            Assert.AreEqual(SectionLoadSequence.Unrecognized, SolutionFileGrammar.LoadSequence.Parse("unknownValue"));
            Assert.AreEqual(SectionLoadSequence.PreSolution, SolutionFileGrammar.LoadSequence.Parse("preSolution"));
            Assert.AreEqual(SectionLoadSequence.PostSolution, SolutionFileGrammar.LoadSequence.Parse("postSolution"));
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
            var result = SolutionFileGrammar.Solution.Parse(input);

            Assert.AreEqual(12, result.MajorVersion);
            Assert.AreEqual(0, result.MinorVersion);
            Assert.AreEqual("Visual Studio 2012", result.ProductName);
            Assert.AreEqual(1, result.GlobalSections.Count);
        }

        [Test]
        public void SolutionProperty_is_Name_equals_Value()
        {
            var input = "HideSolutionNode = FALSE";

            var result = SolutionFileGrammar.SolutionProperty.Parse(input);

            Assert.AreEqual("HideSolutionNode", result.Name);
            Assert.AreEqual("FALSE", result.Value);
        }

        [Test]
        public void GlobalSection_can_parse_SolutionPropertiesSection()
        {
            var input =
@"GlobalSection(SolutionProperties) = preSolution
	HideSolutionNode = FALSE
EndGlobalSection
";

            var result = SolutionFileGrammar.GlobalSection.Parse(input);

            Assert.IsInstanceOf(typeof(SolutionPropertiesSection), result);
            Assert.AreEqual(SectionLoadSequence.PreSolution, result.LoadSequence);

            var section = result as SolutionPropertiesSection;
            Assert.AreEqual(1, section.Properties.Count);
            Assert.AreEqual("SolutionProperties", section.Name);
        }

        [Test]
        public void BracketedGuid()
        {
            var input = "{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}";

            var result = SolutionFileGrammar.BracketedGuid.Parse(input);

            Assert.AreEqual(new Guid(input), result);
        }

        [Test]
        public void QuotedString()
        {
            var input = "\"serenity\"";

            var result = SolutionFileGrammar.QuotedString.Parse(input);

            Assert.AreEqual("serenity", result);
        }

        [Test]
        [Ignore("Not sure what it is doing wrong...")]
        public void Guid()
        {
            var input = "FAE04EC0-301F-11D3-BF4B-00C04F79EFBC";

            var result = SolutionFileGrammar.Guid.Parse(input);

            Assert.AreEqual(new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"), result);
        }

        [Test]
        public void QuotedGuid()
        {
            var input = "\"{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}\"";

            var result = SolutionFileGrammar.QuotedGuid.Parse(input);

            Assert.AreEqual(new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"), result);
        }

        [Test]
        public void Project()
        {
            var input =
@"Project(""{FAE04EC0-301F-11D3-BF4B-00C04F79EFBC}"") = ""HttpWebAdapters"", ""HttpWebAdapters\HttpWebAdapters.csproj"", ""{AE7D2A46-3F67-4986-B04B-7DCE79A549A5}""
EndProject
";
            var result = SolutionFileGrammar.Project.Parse(input);

            Assert.AreEqual(new Guid("FAE04EC0-301F-11D3-BF4B-00C04F79EFBC"), result.ProjectType);
            Assert.AreEqual(new Guid("AE7D2A46-3F67-4986-B04B-7DCE79A549A5"), result.ID);
            Assert.AreEqual("HttpWebAdapters", result.Name);
            Assert.AreEqual(@"HttpWebAdapters\HttpWebAdapters.csproj", result.Path);
        }

        [Test]
        public void SolutionFile_can_have_projects()
        {
            var input = ReadSampleFile("Example.00.sln");

            var result = SolutionFileGrammar.Solution.Parse(input);
        }

	    private string ReadSampleFile(string fileName)
	    {
	        string relativeManifestUri = String.Format("TestData.{0}", fileName);
	        return ManifestResourceHelper.ExtractManifestResourceToString(relativeManifestUri);
	    }
	}

	public class SolutionFileGrammar
	{
	    #region stable

		static readonly Parser<string> NewLine =
			Parse.String(Environment.NewLine).Text();

		static readonly Parser<string> Eof =
			Parse.Return("").End().XOr(
			NewLine.End()).Or(
			NewLine);

		public static readonly Parser<SolutionVersionNumber> SolutionVersionNumber =
			from rawMajor in Parse.Number.Token()
			from period in Parse.Char('.')
			from rawMinor in Parse.Number.Token()
			let major = int.Parse(rawMajor)
			let minor = int.Parse(rawMinor)
			select new SolutionVersionNumber(major, minor);

		public static readonly Parser<string> ProductName =
			from pound in Parse.Char('#').Token()
			from name in Parse.AnyChar.Until(NewLine.Or(Eof)).Text()
			select name;

		public static readonly Parser<SolutionFileHeader> Header =
			from ignore1 in Parse.String("Microsoft Visual Studio Solution File, Format Version").Token()
			from version in SolutionVersionNumber
			from name in ProductName
			select new SolutionFileHeader(version, name);

	    public static readonly Parser<SectionLoadSequence> LoadSequence =
	        from sequence in Parse.String("preSolution").Token().Return(SectionLoadSequence.PreSolution)
                         .Or(Parse.String("postSolution").Token().Return(SectionLoadSequence.PostSolution))
                         .Or(Parse.Return(SectionLoadSequence.Unrecognized))
	        select sequence;

        public static readonly Parser<string> RoundBracketedString =
			from left in Parse.Char('(').Token()
			from content in Parse.CharExcept(')').Many().Text()
			from right in Parse.Char(')').Token()
			select content;

        public static readonly Func<string,Parser<SolutionFileGlobalSection>> UnrecognizedGlobalSection = sectionName =>
            from ignore1 in Parse.Char('=').Token()
            from loadSequence in LoadSequence
            from content in Parse.AnyChar.Until(Parse.String("EndGlobalSection").Token()).Text()
            select new UnrecognizedGlobalSection(sectionName, content, loadSequence);

        public static readonly Parser<SolutionFileGlobalSection> SolutionConfigurationPlatformsGlobalSection = 
            from ignore1 in Parse.Char('=').Token()
            from loadSequence in LoadSequence
            from content in Parse.AnyChar.Until(Parse.String("EndGlobalSection").Token()).Text()
            select new SolutionConfigurationPlatformsGlobalSection(content, loadSequence);

        public static readonly Parser<SolutionFileGlobalSection> ProjectConfigurationPlatformsGlobalSection =
            from ignore1 in Parse.Char('=').Token()
            from loadSequence in LoadSequence
            from content in Parse.AnyChar.Until(Parse.String("EndGlobalSection").Token()).Text()
            select new ProjectConfigurationPlatformsGlobalSection(content, loadSequence);

	    public static readonly Parser<SolutionProperty> SolutionProperty =
	        from name in Parse.CharExcept('=').Many().Text().Token()
            from ignore in Parse.Char('=').Token()
	        from value in Parse.AnyChar.Until(NewLine.Or(Eof)).Text().Token()
            let sanitizedName = (name ?? string.Empty).Trim()
            let sanitizedValue = (value ?? string.Empty).Trim()
	        select new SolutionProperty(sanitizedName, sanitizedValue);

        public static readonly Parser<SolutionFileGlobalSection> SolutionPropertiesGlobalSection =
            from ignore1 in Parse.Char('=').Token()
            from loadSequence in LoadSequence
            from properties in SolutionProperty.AtLeastOnce()
            from end in Parse.AnyChar.Until(Parse.String("EndGlobalSection").Token()).Text()
            select new SolutionPropertiesSection(loadSequence, properties);

	    public static readonly Parser<SolutionFileGlobalSection> GlobalSection =
	        from start in Parse.String("GlobalSection").Token()
	        from section in RoundBracketedString.Then(s =>
	            {
	                return s == "SolutionProperties" ? SolutionPropertiesGlobalSection
                         : s == "SolutionConfigurationPlatforms" ? SolutionConfigurationPlatformsGlobalSection
                         : s == "ProjectConfigurationPlatforms" ? ProjectConfigurationPlatformsGlobalSection
	                                                 : UnrecognizedGlobalSection(s);
	            })
            // end tag is swallowed by the specific section parsers
	        select section;

	    public static readonly Parser<IEnumerable<SolutionFileGlobalSection>> Global =
	        from start in Parse.String("Global").Token()
	        from sections in GlobalSection.Many()
	        from end in Parse.String("EndGlobal").Token()
	        select sections;

		#endregion

	    public static readonly Parser<Guid> Guid =
	        from part1 in Parse.LetterOrDigit.Repeat(8)
	        from dash1 in Parse.Char('-')
	        from part2 in Parse.LetterOrDigit.Repeat(4)
	        from dash2 in Parse.Char('-')
	        from part3 in Parse.LetterOrDigit.Repeat(4)
	        from dash3 in Parse.Char('-')
	        from part4 in Parse.LetterOrDigit.Repeat(4)
	        from dash4 in Parse.Char('-')
	        from part5 in Parse.LetterOrDigit.Repeat(13)
            let guid = string.Format("{0}-{1}-{2}-{3}-{4}", part1, part2, part3, part4, part5)
	        select System.Guid.Parse(guid);

        public static readonly Parser<Guid> BracketedGuid =
            (from leading in Parse.Char('{')
             from value in Parse.CharExcept('}').Many().Text()
             from trailing in Parse.Char('}')
	         select new Guid(value)).Token();

        public static readonly Parser<string> QuotedString =
          (from leading in Parse.Char('"')
           from content in Parse.CharExcept('"').Many().Text()
           from trailing in Parse.Char('"')
           select content).Token();

        public static readonly Parser<Guid> QuotedGuid =
            (from unquoted in QuotedString
             select BracketedGuid.Parse(unquoted)).Token();

	    public static readonly Parser<ProjectDeclaration> Project =
            from start in Parse.String("Project")
            from left in Parse.Char('(').Token()
            from projectType in QuotedGuid
            from right in Parse.Char(')').Token()
            from ignore in Parse.Char('=').Token()
            from projectName in QuotedString
            from ignore2 in Parse.Char(',').Token()
            from projectPath in QuotedString
            from ignore3 in Parse.Char(',').Token()
            from projectId in QuotedGuid
            //from sections in SolutionFileProjectSection.Many().Optional()
            from end in Parse.String("EndProject")
            select new ProjectDeclaration(projectType, projectName, projectPath, projectId);

	    public static readonly Parser<SolutionFile> Solution =
	        from header in Header
            from projects in Project.Many().Optional()
	        from globals in Global
	        select new SolutionFile(header, globals);
	}

    public class ProjectDeclaration
    {
        public ProjectDeclaration(Guid projectType, string projectName, string projectPath, Guid projectId)
        {
            ID = projectId;
            Name = projectName;
            ProjectType = projectType;
            Path = projectPath;
        }

        public Guid ID { get; private set; }
        public string Name { get; private set; }
        public Guid ProjectType { get; private set; }
        public string Path { get; private set; }
    }

    public class SolutionProperty
    {
        public SolutionProperty(string name, string value)
        {
            Name = name;
            Value = value;
        }

        public string Name { get; private set; }
        public string Value { get; private set; }
    }

    public class SolutionPropertiesSection : SolutionFileGlobalSection
    {
        public SolutionPropertiesSection(SectionLoadSequence loadSequence, IEnumerable<SolutionProperty> properties) 
            : base("SolutionProperties", loadSequence)
        {
            Properties = properties.ToDictionary(p => p.Name, p => p.Value);
        }

        public IDictionary<string, string> Properties { get; private set; } 
    }

    public class UnrecognizedGlobalSection : SolutionFileGlobalSection
    {
        public UnrecognizedGlobalSection(string name, string content, SectionLoadSequence loadSequence) : base(name, loadSequence)
        {
            throw new ParseException(string.Format("Unrecognized GlobalSection({0})", name));
        }
    }

    public class SolutionConfigurationPlatformsGlobalSection : SolutionFileGlobalSection
    {
        public SolutionConfigurationPlatformsGlobalSection(string content, SectionLoadSequence loadSequence) 
            : base(typeof(SolutionConfigurationPlatformsGlobalSection).Name, loadSequence)
        {
        }
    }

    public class ProjectConfigurationPlatformsGlobalSection : SolutionFileGlobalSection
    {
        public ProjectConfigurationPlatformsGlobalSection(string content, SectionLoadSequence loadSequence) 
            : base(typeof(ProjectConfigurationPlatformsGlobalSection).Name, loadSequence)
        {
        }
    }

    public enum SectionLoadSequence
    {
        Unrecognized,
        PreSolution,
        PostSolution
    }

	public class SolutionFile
	{
		public SolutionFile(SolutionFileHeader header, IEnumerable<SolutionFileGlobalSection> globals)
		{
		    MajorVersion = header.MajorVersion;
		    MinorVersion = header.MinorVersion;
		    ProductName = header.ProductName;
		    GlobalSections = globals.ToDictionary(x => x.Name, x => x);
		}

		public int MajorVersion { get; private set; }
		public int MinorVersion { get; private set; }
		public string ProductName { get; private set; }
		public Dictionary<string, SolutionFileGlobalSection> GlobalSections { get; private set; }
	}

	public abstract class SolutionFileGlobalSection
	{
	    protected SolutionFileGlobalSection(string name, SectionLoadSequence loadSequence)
	    {
	        if (loadSequence == SectionLoadSequence.Unrecognized)
	            throw new ParseException(String.Format("'{0}' has an unsupported load sequence.", name));

			Name = name;
            LoadSequence = loadSequence;
		}

		public string Name { get; private set; }
	    public SectionLoadSequence LoadSequence { get; private set; }
	}

	public class SolutionVersionNumber
	{
		public SolutionVersionNumber(int major, int minor)
		{
			Major = major;
			Minor = minor;
		}

		public int Major { get; private set; }
		public int Minor { get; private set; }
	}

	public class SolutionFileHeader
	{
		public SolutionFileHeader(SolutionVersionNumber version, string yearReleased)
		{
			MajorVersion = version.Major;
			MinorVersion = version.Minor;
			ProductName = yearReleased;
		}

		public int MajorVersion { get; private set; }
		public int MinorVersion { get; private set; }
		public string ProductName { get; private set; }
	}
}
