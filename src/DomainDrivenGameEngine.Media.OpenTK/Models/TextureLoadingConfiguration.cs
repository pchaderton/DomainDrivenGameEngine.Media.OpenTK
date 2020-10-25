using System;
using System.Collections.Generic;
using OpenTK.Graphics.OpenGL4;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// The configuration to use for loading textures.
    /// </summary>
    public class TextureLoadingConfiguration
    {
        /// <summary>
        /// A default configuration which supports loading edge clamped, linearly filtered textures with generated mipmaps.
        /// </summary>
        public static readonly TextureLoadingConfiguration Default = new TextureLoadingConfiguration(new Dictionary<TextureParameterName, int>
            {
                { TextureParameterName.TextureBaseLevel, 0 },
                { TextureParameterName.TextureMaxLevel, 10 },
                { TextureParameterName.TextureWrapS, (int)TextureWrapMode.ClampToEdge },
                { TextureParameterName.TextureWrapR, (int)TextureWrapMode.ClampToEdge },
                { TextureParameterName.TextureWrapT, (int)TextureWrapMode.ClampToEdge },
                { TextureParameterName.TextureMinFilter, (int)TextureMinFilter.LinearMipmapLinear },
                { TextureParameterName.TextureMagFilter, (int)TextureMagFilter.Linear },
            },
            true);

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoadingConfiguration"/> class.
        /// </summary>
        /// <param name="textureParameters">A dictionary containing the parameters to apply to loaded textures.</param>
        /// <param name="generateMipmaps">A value indicating whether or not mipmaps should be generated for loaded textures.</param>
        public TextureLoadingConfiguration(IReadOnlyDictionary<TextureParameterName, int> textureParameters,
                                           bool generateMipmaps = true)
        {
            TextureParameters = textureParameters ?? throw new ArgumentNullException(nameof(textureParameters));
            GenerateMipmaps = generateMipmaps;
        }

        /// <summary>
        /// Gets a value indicating whether or not mipmaps should be generated for loaded textures.
        /// </summary>
        public bool GenerateMipmaps { get; }

        /// <summary>
        /// Gets a dictionary containing the parameters to apply to loaded textures.
        /// </summary>
        public IReadOnlyDictionary<TextureParameterName, int> TextureParameters { get; }
    }
}
