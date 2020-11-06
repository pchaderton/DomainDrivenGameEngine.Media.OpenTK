using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using OpenTK.Graphics.OpenGL4;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// The configuration to use for loading programs.
    /// </summary>
    public class ProgramLoadingConfiguration
    {
        /// <summary>
        /// A default configuration which supports vertex and fragment shaders loaded from .glsl files.
        /// </summary>
        public static readonly ProgramLoadingConfiguration Default = new ProgramLoadingConfiguration(new[] { ShaderType.VertexShader, ShaderType.FragmentShader });

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramLoadingConfiguration"/> class.
        /// </summary>
        /// <param name="shaderTypes">The shader types to load into a program.</param>
        /// <param name="shaderExtension">The file extension to use for identifying shaders to load, defaults to ".glsl".</param>
        public ProgramLoadingConfiguration(IEnumerable<ShaderType> shaderTypes, string shaderExtension = ".glsl")
        {
            ShaderTypes = new ReadOnlyCollection<ShaderType>(shaderTypes?.ToArray() ?? throw new ArgumentNullException(nameof(shaderTypes)));
            ShaderExtension = shaderExtension;

            if (ShaderTypes.Count == 0)
            {
                throw new Exception($"At least one entry in {nameof(shaderTypes)} is required.");
            }
        }

        /// <summary>
        /// Gets the file extension to use for identifying shaders to load.
        /// </summary>
        public string ShaderExtension { get; }

        /// <summary>
        /// Gets the shader types to load into a program.
        /// </summary>
        public IReadOnlyList<ShaderType> ShaderTypes { get; }
    }
}
