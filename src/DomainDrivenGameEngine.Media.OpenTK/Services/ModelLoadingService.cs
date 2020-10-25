using System;
using System.Collections.Generic;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Graphics.OpenGL4;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading models for use with OpenTK 4.0+.
    /// </summary>
    /// <remarks>
    /// The domain assumes that vertex texture coordinates start from the top at Y=0, which is opposite of what
    /// OpenGL assumes, so to compensate for that vertex texture coordinates need to be inverted.
    /// </remarks>
    public class ModelLoadingService : BaseMediaLoadingService<Model, LoadedModel>
    {
        /// <summary>
        /// Any shaders referenced for a model that had an implementation loaded.
        /// </summary>
        private readonly IDictionary<long, IReadOnlyCollection<IMediaReference<Shader>>> _shaderReferencesByModelId;

        /// <summary>
        /// The <see cref="IMediaReferenceService{Shader}"/> to use to reference shader programs.
        /// </summary>
        private readonly IMediaReferenceService<Shader> _shaderReferenceService;

        /// <summary>
        /// Any textures referenced for a model that had an implementation loaded.
        /// </summary>
        private readonly IDictionary<long, IReadOnlyCollection<IMediaReference<Texture>>> _textureReferencesByModelId;

        /// <summary>
        /// The <see cref="IMediaReferenceService{Texture}"/> to use to reference textures.
        /// </summary>
        private readonly IMediaReferenceService<Texture> _textureReferenceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLoadingService"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="IMediaSourceService{Model}"/>s to use for sourcing models.</param>
        /// <param name="textureLoadingService">The <see cref="IMediaLoadingService{Texture, LoadedTexture}"/> to use to reference textures.</param>
        /// <param name="shaderLoadingService">The <see cref="IMediaLoadingService{Shader, LoadedProgram}"/> to use to reference shader programs.</param>
        public ModelLoadingService(IMediaSourceService<Model>[] sources,
                                   IMediaLoadingService<Texture, LoadedTexture> textureLoadingService,
                                   IMediaLoadingService<Shader, LoadedProgram> shaderLoadingService)
            : base(sources)
        {
            _textureReferenceService = textureLoadingService ?? throw new ArgumentNullException(nameof(textureLoadingService));
            _shaderReferenceService = shaderLoadingService ?? throw new ArgumentNullException(nameof(shaderLoadingService));
            _shaderReferencesByModelId = new Dictionary<long, IReadOnlyCollection<IMediaReference<Shader>>>();
            _textureReferencesByModelId = new Dictionary<long, IReadOnlyCollection<IMediaReference<Texture>>>();
        }

        /// <inheritdoc/>
        protected override LoadedModel LoadImplementation(params Model[] media)
        {
            var referencedTextures = new List<IMediaReference<Texture>>();
            var referencedShaders = new List<IMediaReference<Shader>>();
            var model = media[0];
            var loadedMeshes = new List<LoadedMesh>();
            foreach (var mesh in model.Meshes)
            {
                var textureReferenceCount = Math.Max(mesh.TexturePaths?.Count ?? 0, mesh.TextureReferences?.Count ?? 0);
                var textureReferences = Enumerable.Range(0, 3).Select(index => null as IMediaReference<Texture>).ToArray();

                if (mesh.TextureReferences != null)
                {
                    var textureReferenceIndex = 0;
                    foreach (var textureReference in mesh.TextureReferences)
                    {
                        if (textureReference != null)
                        {
                            textureReferences[textureReferenceIndex] = textureReference;
                        }

                        textureReferenceIndex++;
                    }
                }

                if (mesh.TexturePaths != null)
                {
                    var textureIndex = 0;
                    foreach (var texturePath in mesh.TexturePaths)
                    {
                        if (textureReferences[textureIndex] == null && !string.IsNullOrWhiteSpace(texturePath))
                        {
                            var textureReference = _textureReferenceService.Reference(texturePath);
                            textureReferences[textureIndex] = textureReference;
                            referencedTextures.Add(textureReference);
                        }

                        textureIndex++;
                    }
                }

                var shaderReference = mesh.DefaultShaderReference;
                if (shaderReference == null && mesh.DefaultShaderPaths != null && mesh.DefaultShaderPaths.Count > 0)
                {
                    shaderReference = _shaderReferenceService.Reference(paths: mesh.DefaultShaderPaths.ToArray());
                    referencedShaders.Add(shaderReference);
                }

                var vertexArrayId = GL.GenVertexArray();

                GL.BindVertexArray(vertexArrayId);

                var vertexBufferId = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ArrayBuffer, vertexBufferId);

                var vertexBuffer = new float[15 * mesh.Vertices.Count];
                var vertexCount = 0;
                foreach (var vertex in mesh.Vertices)
                {
                    var index = vertexCount * 15;
                    vertexBuffer[index + 0] = vertex.Position.X;
                    vertexBuffer[index + 1] = vertex.Position.Y;
                    vertexBuffer[index + 2] = vertex.Position.Z;
                    vertexBuffer[index + 3] = vertex.Normal.X;
                    vertexBuffer[index + 4] = vertex.Normal.Y;
                    vertexBuffer[index + 5] = vertex.Normal.Z;
                    vertexBuffer[index + 6] = vertex.Tangent.X;
                    vertexBuffer[index + 7] = vertex.Tangent.Y;
                    vertexBuffer[index + 8] = vertex.Tangent.Z;
                    vertexBuffer[index + 9] = vertex.Color.Red;
                    vertexBuffer[index + 10] = vertex.Color.Green;
                    vertexBuffer[index + 11] = vertex.Color.Blue;
                    vertexBuffer[index + 12] = vertex.Color.Alpha;
                    vertexBuffer[index + 13] = vertex.TextureCoordinate.X;
                    vertexBuffer[index + 14] = 1.0f - vertex.TextureCoordinate.Y;
                    vertexCount++;
                }

                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length * sizeof(float), vertexBuffer.ToArray(), BufferUsageHint.StaticDraw);

                var vertexBufferLength = vertexBuffer.Length;

                var indexBufferId = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);

                var indices = mesh.Indices.ToArray();

                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                var indexBufferLength = indices.Length;

                GL.VertexAttribPointer(0, 3, VertexAttribPointerType.Float, false, 15 * sizeof(float), 0);
                GL.VertexAttribPointer(1, 3, VertexAttribPointerType.Float, false, 15 * sizeof(float), 3 * sizeof(float));
                GL.VertexAttribPointer(2, 3, VertexAttribPointerType.Float, false, 15 * sizeof(float), 6 * sizeof(float));
                GL.VertexAttribPointer(3, 4, VertexAttribPointerType.Float, false, 15 * sizeof(float), 9 * sizeof(float));
                GL.VertexAttribPointer(4, 2, VertexAttribPointerType.Float, false, 15 * sizeof(float), 13 * sizeof(float));

                GL.EnableVertexAttribArray(0);
                GL.EnableVertexAttribArray(1);
                GL.EnableVertexAttribArray(2);
                GL.EnableVertexAttribArray(3);
                GL.EnableVertexAttribArray(4);

                loadedMeshes.Add(new LoadedMesh(vertexArrayId,
                                                vertexBufferId,
                                                vertexBufferLength,
                                                indexBufferId,
                                                indexBufferLength,
                                                textureReferences.Length >= 1 ? textureReferences : null,
                                                mesh.DefaultBlendMode,
                                                shaderReference));
            }

            return new LoadedModel(loadedMeshes);
        }

        /// <inheritdoc/>
        protected override void UnloadImplementation(LoadedModel implementation)
        {
            foreach (var mesh in implementation.Meshes)
            {
                GL.DeleteVertexArray(mesh.VertexArrayId);
                GL.DeleteBuffer(mesh.IndexBufferId);
                GL.DeleteBuffer(mesh.VertexBufferId);
            }
        }
    }
}
