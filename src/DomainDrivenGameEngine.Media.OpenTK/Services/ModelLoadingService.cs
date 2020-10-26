using System;
using System.Collections.Generic;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Graphics.OpenGL4;
using SystemBuffer = System.Buffer;

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
        /// The <see cref="ModelLoadingConfiguration"/> to use when loading models.
        /// </summary>
        private readonly ModelLoadingConfiguration _configuration;

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
            : this(sources, textureLoadingService, shaderLoadingService, ModelLoadingConfiguration.DefaultStatic)
        {
            _textureReferenceService = textureLoadingService ?? throw new ArgumentNullException(nameof(textureLoadingService));
            _shaderReferenceService = shaderLoadingService ?? throw new ArgumentNullException(nameof(shaderLoadingService));
            _shaderReferencesByModelId = new Dictionary<long, IReadOnlyCollection<IMediaReference<Shader>>>();
            _textureReferencesByModelId = new Dictionary<long, IReadOnlyCollection<IMediaReference<Texture>>>();
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelLoadingService"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="IMediaSourceService{Model}"/>s to use for sourcing models.</param>
        /// <param name="textureLoadingService">The <see cref="IMediaLoadingService{Texture, LoadedTexture}"/> to use to reference textures.</param>
        /// <param name="shaderLoadingService">The <see cref="IMediaLoadingService{Shader, LoadedProgram}"/> to use to reference shader programs.</param>
        /// <param name="configuration">The <see cref="ModelLoadingConfiguration"/> to use when loading models.</param>
        public ModelLoadingService(IMediaSourceService<Model>[] sources,
                                   IMediaLoadingService<Texture, LoadedTexture> textureLoadingService,
                                   IMediaLoadingService<Shader, LoadedProgram> shaderLoadingService,
                                   ModelLoadingConfiguration configuration)
            : base(sources)
        {
            _textureReferenceService = textureLoadingService ?? throw new ArgumentNullException(nameof(textureLoadingService));
            _shaderReferenceService = shaderLoadingService ?? throw new ArgumentNullException(nameof(shaderLoadingService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
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

                // As more VertexAttribPointerTypes are used here, remember to add support for them in GetSizeOfVertexAttribute below.
                var arrayFactoriesWithTypeSizeTuples = new List<Tuple<Func<Array>, VertexAttribPointerType, int>>();
                foreach (var enabledVertexAttribute in _configuration.EnabledVertexAttributes)
                {
                    if (enabledVertexAttribute == VertexAttribute.Position)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => new[] { vertex.Position.X, vertex.Position.Y, vertex.Position.Z }).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  3));
                    }
                    else if (enabledVertexAttribute == VertexAttribute.Normal)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => new[] { vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z }).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  3));
                    }
                    else if (enabledVertexAttribute == VertexAttribute.Tangent)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => new[] { vertex.Tangent.X, vertex.Tangent.Y, vertex.Tangent.Z }).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  3));
                    }
                    else if (enabledVertexAttribute == VertexAttribute.Color)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => new[] { vertex.Color.Red, vertex.Color.Green, vertex.Color.Blue, vertex.Color.Alpha }).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  4));
                    }
                    else if (enabledVertexAttribute == VertexAttribute.TextureCoordinate)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => new[] { vertex.TextureCoordinate.X, vertex.TextureCoordinate.Y }).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  2));
                    }
                }

                var vertexSizeBytes = arrayFactoriesWithTypeSizeTuples.Sum(t => GetSizeOfVertexAttribute(t.Item2) * t.Item3);
                var vertexBuffer = new byte[mesh.Vertices.Count * vertexSizeBytes];
                var destinationOffset = 0;

                var attribPointerTypeSizeOffsetTuples = new List<Tuple<VertexAttribPointerType, int, int>>();

                foreach (var arrayFactoriesWithTypeSizeTuple in arrayFactoriesWithTypeSizeTuples)
                {
                    attribPointerTypeSizeOffsetTuples.Add(new Tuple<VertexAttribPointerType, int, int>(arrayFactoriesWithTypeSizeTuple.Item2, arrayFactoriesWithTypeSizeTuple.Item3, destinationOffset));
                    destinationOffset = CopyValuesToVertexBuffer(arrayFactoriesWithTypeSizeTuple.Item1(), sizeof(float), vertexBuffer, destinationOffset);
                }

                GL.BufferData(BufferTarget.ArrayBuffer, vertexBuffer.Length, vertexBuffer, BufferUsageHint.StaticDraw);

                var vertexBufferLength = vertexBuffer.Length;

                var indexBufferId = GL.GenBuffer();

                GL.BindBuffer(BufferTarget.ElementArrayBuffer, indexBufferId);

                var indices = mesh.Indices.ToArray();

                GL.BufferData(BufferTarget.ElementArrayBuffer, indices.Length * sizeof(uint), indices, BufferUsageHint.StaticDraw);

                var indexBufferLength = indices.Length;

                for (var i = 0; i < attribPointerTypeSizeOffsetTuples.Count; i++)
                {
                    var tuple = attribPointerTypeSizeOffsetTuples[i];
                    GL.EnableVertexAttribArray(i);
                    GL.VertexAttribPointer(i, tuple.Item2, tuple.Item1, false, tuple.Item2 * GetSizeOfVertexAttribute(tuple.Item1), tuple.Item3);
                }

                loadedMeshes.Add(new LoadedMesh(vertexArrayId,
                                                vertexBufferId,
                                                vertexBufferLength,
                                                indexBufferId,
                                                indexBufferLength,
                                                textureReferences.Length >= 1 ? textureReferences : null,
                                                mesh.DefaultBlendMode,
                                                shaderReference));
            }

            var loadedModel = new LoadedModel(loadedMeshes);

            if (referencedTextures.Count > 0)
            {
                _textureReferencesByModelId.Add(loadedModel.Id, referencedTextures);
            }

            if (referencedShaders.Count > 0)
            {
                _shaderReferencesByModelId.Add(loadedModel.Id, referencedShaders);
            }

            return loadedModel;
        }

        /// <inheritdoc/>
        protected override void UnloadImplementation(LoadedModel implementation)
        {
            if (_textureReferencesByModelId.TryGetValue(implementation.Id, out var referencedTextures))
            {
                foreach (var referencedTexture in referencedTextures)
                {
                    _textureReferenceService.Unreference(referencedTexture);
                }

                _textureReferencesByModelId.Remove(implementation.Id);
            }

            if (_shaderReferencesByModelId.TryGetValue(implementation.Id, out var referencedShaders))
            {
                foreach (var referencedShader in referencedShaders)
                {
                    _shaderReferenceService.Unreference(referencedShader);
                }

                _shaderReferencesByModelId.Remove(implementation.Id);
            }

            foreach (var mesh in implementation.Meshes)
            {
                GL.DeleteVertexArray(mesh.VertexArrayId);
                GL.DeleteBuffer(mesh.IndexBufferId);
                GL.DeleteBuffer(mesh.VertexBufferId);
            }
        }

        /// <summary>
        /// Gets the size of each element of a given <see cref="VertexAttribPointerType"/>.
        /// </summary>
        /// <param name="attributeType">The <see cref="VertexAttribPointerType"/> to get the size of.</param>
        /// <returns>The resulting size of each element of a given <see cref="VertexAttribPointerType"/>.</returns>
        private int GetSizeOfVertexAttribute(VertexAttribPointerType attributeType)
        {
            if (attributeType == VertexAttribPointerType.Float)
            {
                return sizeof(float);
            }

            throw new ArgumentException($"Unrecognized {nameof(VertexAttribPointerType)}: {attributeType.ToString()}");
        }

        /// <summary>
        /// Copies values from one array to a byte array for a vertex buffer.
        /// </summary>
        /// <param name="source">The source array to copy.</param>
        /// <param name="sizePerSourceElement">The size of each element in the array.</param>
        /// <param name="destination">The destination byte array.</param>
        /// <param name="destinationOffset">The offset to copy data to in the byte array.</param>
        /// <returns>The next offset to copy to.</returns>
        private int CopyValuesToVertexBuffer(Array source, int sizePerSourceElement, byte[] destination, int destinationOffset)
        {
            var sizeInBytes = sizePerSourceElement * source.Length;
            SystemBuffer.BlockCopy(source, 0, destination, destinationOffset, sizeInBytes);
            return destinationOffset + sizeInBytes;
        }
    }
}
