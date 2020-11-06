using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

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
            },
            0);

        /// <summary>
        /// The default configuration to use for loading animated models.
        /// </summary>
        public static readonly ModelLoadingConfiguration DefaultAnimated =
            new ModelLoadingConfiguration(new[]
            {
                VertexAttribute.Position,
                VertexAttribute.Normal,
                VertexAttribute.Tangent,
                VertexAttribute.Color,
                VertexAttribute.TextureCoordinate,
                VertexAttribute.BoneIndices,
                VertexAttribute.BoneWeights,
            },
            4);

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLoadingConfiguration"/> class.
        /// </summary>
        /// <param name="enabledVertexAttributes">The in-order vertex attributes to apply to generated vertex buffers.</param>
        /// <param name="enabledBoneCount">The number of bones that can affect each vertex.</param>
        public ModelLoadingConfiguration(IEnumerable<VertexAttribute> enabledVertexAttributes, byte enabledBoneCount)
        {
            EnabledVertexAttributes = new ReadOnlyCollection<VertexAttribute>(enabledVertexAttributes?.ToArray() ?? throw new ArgumentNullException(nameof(enabledVertexAttributes)));
            EnabledBoneCount = enabledBoneCount;
        }

        /// <summary>
        /// Gets the number of bones that can affect each vertex.
        /// </summary>
        public byte EnabledBoneCount { get; }

        /// <summary>
        /// Gets the in-order vertex attributes to apply to generated vertex buffers.
        /// </summary>
        public IReadOnlyList<VertexAttribute> EnabledVertexAttributes { get; }
    }
}
