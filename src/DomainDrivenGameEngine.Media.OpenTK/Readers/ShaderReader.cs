using System.IO;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Readers;

namespace DomainDrivenGameEngine.Media.OpenTK.Readers
{
    /// <summary>
    /// A reader for reading shaders.
    /// </summary>
    public class ShaderReader : BaseMediaReader<Shader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderReader"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProgramLoadingConfiguration"/> to use for identifying what file extensions shaders have.</param>
        public ShaderReader(ProgramLoadingConfiguration configuration)
            : base(new[] { configuration.ShaderExtension })
        {
        }

        /// <inheritdoc/>
        public override Shader Read(Stream stream, string path, string extension)
        {
            // The media loading service will dispose of the source stream for us.
            using (var streamReader = new StreamReader(stream, leaveOpen: true))
            {
                return new Shader(streamReader.ReadToEnd(), stream);
            }
        }
    }
}
