using System;
using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded mesh for use with OpenTK.
    /// </summary>
    public class LoadedMesh
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedMesh"/> class.
        /// </summary>
        /// <param name="vertexArrayId">The OpenGL vertex array ID to use for rendering the mesh.</param>
        /// <param name="vertexBufferId">The OpenGL vertex buffer ID to use for rendering the mesh.</param>
        /// <param name="vertexBufferLength">The length of the vertex buffer.</param>
        /// <param name="indexBufferId">The OpenGL index buffer ID to use for rendering the mesh.</param>
        /// <param name="indexBufferLength">The length of the index buffer.</param>
        /// <param name="textureReferences">The texture references to use for rendering the mesh.</param>
        /// <param name="defaultBlendMode">The default blend mode to use for the mesh.</param>
        /// <param name="defaultShaderReference">The reference to the default shader to use for the mesh.</param>
        public LoadedMesh(int vertexArrayId,
                          int vertexBufferId,
                          int vertexBufferLength,
                          int indexBufferId,
                          int indexBufferLength,
                          IReadOnlyCollection<IMediaReference<Texture>> textureReferences,
                          BlendMode defaultBlendMode = BlendMode.None,
                          IMediaReference<Shader> defaultShaderReference = null)
        {
            if (vertexArrayId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(vertexArrayId)} is required.");
            }

            if (vertexBufferId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(vertexBufferId)} is required.");
            }

            if (vertexBufferLength <= 0)
            {
                throw new ArgumentException($"A valid {nameof(vertexBufferLength)} is required.");
            }

            if (indexBufferId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(indexBufferId)} is required.");
            }

            if (indexBufferLength <= 0)
            {
                throw new ArgumentException($"A valid {nameof(indexBufferLength)} is required.");
            }

            VertexArrayId = vertexArrayId;
            VertexBufferId = vertexBufferId;
            VertexBufferLength = vertexBufferLength;
            IndexBufferId = indexBufferId;
            IndexBufferLength = indexBufferLength;
            TextureReferences = textureReferences ?? new IMediaReference<Texture>[] { };
            DefaultBlendMode = defaultBlendMode;
            DefaultShaderReference = defaultShaderReference;
        }

        /// <summary>
        /// Gets the default blend mode to use for the mesh.
        /// </summary>
        public BlendMode DefaultBlendMode { get; }

        /// <summary>
        /// Gets the reference to the default shader to use for the mesh.
        /// </summary>
        public IMediaReference<Shader> DefaultShaderReference { get; }

        /// <summary>
        /// Gets the OpenGL index buffer ID to use for rendering the mesh.
        /// </summary>
        public int IndexBufferId { get; }

        /// <summary>
        /// Gets the length of the index buffer.
        /// </summary>
        public int IndexBufferLength { get; }

        /// <summary>
        /// Gets the texture references to use for rendering the mesh.
        /// </summary>
        public IReadOnlyCollection<IMediaReference<Texture>> TextureReferences { get; }

        /// <summary>
        /// Gets the OpenGL vertex array ID to use for rendering the mesh.
        /// </summary>
        public int VertexArrayId { get; }

        /// <summary>
        /// Gets the OpenGL vertex buffer ID to use for rendering the mesh.
        /// </summary>
        public int VertexBufferId { get; }

        /// <summary>
        /// Gets the length of the vertex buffer.
        /// </summary>
        public int VertexBufferLength { get; }
    }
}
