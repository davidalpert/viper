using System.Linq;
using System.Collections.Generic;
using System;
using System.IO;
using System.Reflection;

namespace VisualStudioSolutionFileParser.Tests.Helpers
{
    /// <summary>
    /// Helper class for accessing content compiled into an assembly with the Manifest Resource content setting.
    /// </summary>
    public class ManifestResourceHelper
    {
        /// <summary>
        /// Extracts an embedded Manifest Resource from the calling assembly.
        /// </summary>
        /// <param name="relativeManifestUri">A dot-separated relative.path.to.resource</param> 
        /// <param name="targetPath">The target path for the extracted resource.</param>
        /// <remarks>
        /// <para>If the directory doesn't exist it will be created.</para>
        /// <para>The <paramref name="relativeManifestUri"/> is relative to the 
        /// calling assembly's Name:</para>
        /// <example>
        /// To extract [calling.assembly]/SomeFolder/somefile.txt use 'SomeFolder.somefile.txt'
        /// </example>
        /// </remarks>
        public static void ExtractManifestResourceToDisk(string relativeManifestUri, string targetPath, bool overwrite = false)
        {
            if (targetPath == null) return; // nothing to do

            if (File.Exists(targetPath))
            {
                if (overwrite)
                    File.Delete(targetPath);
                else
                    return; // don't extract it more than once
            }

            var targetFolder = Path.GetDirectoryName(targetPath);
            if (targetFolder == null) return;
            Directory.CreateDirectory(targetFolder);

            WithExtractedManifestResourceStream(relativeManifestUri, resourceStream =>
            {
                using (Stream output = File.Create(targetPath))
                {
                    if (resourceStream != null)
                        resourceStream.CopyTo(output);
                }
            });
        }

        /// <summary>
        /// Extracts a Manifest Resource as a <see cref="string"/>.
        /// </summary>
        /// <param name="relativeManifestUri">A dot-separated relative.path.to.resource</param> 
        /// <remarks>
        /// <para>The <paramref name="relativeManifestUri"/> is relative to the calling assembly's Name:</para>
        /// <example>
        /// To extract [calling.assembly]/SomeFolder/somefile.txt use 'SomeFolder.somefile.txt'
        /// </example>
        /// </remarks>
        /// <returns>The contents of the <paramref name="relativeManifestUri"/> as a <see cref="string"/>.</returns>
        public static string ExtractManifestResourceToString(string relativeManifestUri)
        {
            string result = null;

            WithExtractedManifestResourceStream(relativeManifestUri, input =>
            {
                using (TextReader reader = new StreamReader(input))
                {
                    result = reader.ReadToEnd();
                }
            });

            return result;
        }

        /// <summary>
        /// Extracts and exposes a Manifest Resource as a <see cref="Stream"/>.
        /// </summary>
        /// <param name="relativeManifestUri">A dot-separated relative.path.to.resource</param> 
        /// <param name="doSomething">A lambda that can operate on the extracted resource as a stream</param>
        /// <remarks>
        /// <para>The <paramref name="relativeManifestUri"/> is relative to the calling assembly's Name:</para>
        /// <example>
        /// To extract [calling.assembly]/SomeFolder/somefile.txt use 'SomeFolder.somefile.txt'
        /// </example>
        /// </remarks>
        private static void WithExtractedManifestResourceStream(string relativeManifestUri, Action<Stream> doSomething)
        {
            var assembly = Assembly.GetCallingAssembly();

            var uri = string.Format("{0}.{1}", assembly.GetName().Name, relativeManifestUri);

            using (Stream resourceStream = assembly.GetManifestResourceStream(uri))
            {
                if (resourceStream == null)
                    throw new FileNotFoundException("There was no embedded resource at '" + uri + "'");

                doSomething(resourceStream);
            }
        }
    }
}