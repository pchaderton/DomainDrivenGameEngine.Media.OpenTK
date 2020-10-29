using System.IO;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A source for loading shaders.
    /// </summary>
    public class ShaderSourceService : BaseMediaSourceService<Shader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ShaderSourceService"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="ProgramLoadingConfiguration"/> to use for identifying what file extensions shaders have.</param>
        public ShaderSourceService(ProgramLoadingConfiguration configuration)
            : base(new[] { configuration.ShaderExtension })
        {
        }

        /// <inheritdoc/>
        public override Shader Load(Stream stream, string path, string extension)
        {
            // The media loading service will dispose of the source stream for us.
            using (var streamReader = new StreamReader(stream, leaveOpen: true))
            {
                return new Shader(streamReader.ReadToEnd());
            }
        }
    }
}
