using System;
using System.Collections.Generic;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// The configuration to use for loading models.
    /// </summary>
    public class ModelLoadingConfiguration
    {
        /// <summary>
        /// The default configuration to use for loading static models.
        /// </summary>
        public static readonly ModelLoadingConfiguration DefaultStatic =
            new ModelLoadingConfiguration(new[]
            {
                VertexAttribute.Position,
                VertexAttribute.Normal,
                VertexAttribute.Tangent,
                VertexAttribute.Color,
                VertexAttribute.TextureCoordinate,
            });

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLoadingConfiguration"/> class.
        /// </summary>
        /// <param name="enabledVertexAttributes">The in-order vertex attributes to apply to generated vertex buffers.</param>
        public ModelLoadingConfiguration(IReadOnlyCollection<VertexAttribute> enabledVertexAttributes)
        {
            EnabledVertexAttributes = enabledVertexAttributes ?? throw new ArgumentNullException(nameof(enabledVertexAttributes));
        }

        /// <summary>
        /// Gets the in-order vertex attributes to apply to generated vertex buffers.
        /// </summary>
        public IReadOnlyCollection<VertexAttribute> EnabledVertexAttributes { get; }
    }
}
