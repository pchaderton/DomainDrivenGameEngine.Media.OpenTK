using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Graphics.OpenGL4;
using OpenTK.Mathematics;
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
    public class ModelImplementationService : BaseMediaImplementationService<Model, LoadedModel>
    {
        /// <summary>
        /// Any animation collections referenced for a model that had an implementation loaded.
        /// </summary>
        private readonly IDictionary<long, IMediaReference<AnimationCollection>> _animationCollectionReferencesByModelId;

        /// <summary>
        /// The <see cref="IMediaReferenceService{AnimationCollection}"/> to use to reference animation collections.
        /// </summary>
        private readonly IMediaReferenceService<AnimationCollection> _animationCollectionReferenceService;

        /// <summary>
        /// The <see cref="ModelLoadingConfiguration"/> to use when loading models.
        /// </summary>
        private readonly ModelLoadingConfiguration _configuration;

        /// <summary>
        /// The <see cref="IFileAccessService"/> to use for accessing files and manipulating paths.
        /// </summary>
        private readonly IFileAccessService _fileAccessService;

        /// <summary>
        /// Any textures referenced for a model that had an implementation loaded.
        /// </summary>
        private readonly IDictionary<long, IReadOnlyCollection<IMediaReference<Texture>>> _textureReferencesByModelId;

        /// <summary>
        /// The <see cref="IMediaReferenceService{Texture}"/> to use to reference textures.
        /// </summary>
        private readonly IMediaReferenceService<Texture> _textureReferenceService;

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelImplementationService"/> class.
        /// </summary>
        /// <param name="textureLoadingService">The <see cref="IMediaLoadingService{Texture, LoadedTexture}"/> to use to reference textures.</param>
        /// <param name="animationCollectionLoadingService">The <see cref="IMediaLoadingService{AnimationCollection, LoadedAnimationCollection}"/> to use to reference animation collections.</param>
        /// <param name="fileAccessService">The <see cref="IFileAccessService"/> to use for accessing files and manipulating paths.</param>
        public ModelImplementationService(IMediaLoadingService<Texture, LoadedTexture> textureLoadingService,
                                          IMediaLoadingService<AnimationCollection, LoadedAnimationCollection> animationCollectionLoadingService,
                                          IFileAccessService fileAccessService)
            : this(textureLoadingService,
                   animationCollectionLoadingService,
                   fileAccessService,
                   ModelLoadingConfiguration.DefaultStatic)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ModelImplementationService"/> class.
        /// </summary>
        /// <param name="textureLoadingService">The <see cref="IMediaLoadingService{Texture, LoadedTexture}"/> to use to reference textures.</param>
        /// <param name="animationCollectionLoadingService">The <see cref="IMediaLoadingService{AnimationCollection, LoadedAnimationCollection}"/> to use to reference animation collections.</param>
        /// <param name="fileAccessService">The <see cref="IFileAccessService"/> to use for accessing files and manipulating paths.</param>
        /// <param name="configuration">The <see cref="ModelLoadingConfiguration"/> to use when loading models.</param>
        public ModelImplementationService(IMediaLoadingService<Texture, LoadedTexture> textureLoadingService,
                                          IMediaLoadingService<AnimationCollection, LoadedAnimationCollection> animationCollectionLoadingService,
                                          IFileAccessService fileAccessService,
                                          ModelLoadingConfiguration configuration)
        {
            _textureReferenceService = textureLoadingService ?? throw new ArgumentNullException(nameof(textureLoadingService));
            _animationCollectionReferenceService = animationCollectionLoadingService ?? throw new ArgumentNullException(nameof(animationCollectionLoadingService));
            _fileAccessService = fileAccessService ?? throw new ArgumentNullException(nameof(fileAccessService));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _textureReferencesByModelId = new Dictionary<long, IReadOnlyCollection<IMediaReference<Texture>>>();
            _animationCollectionReferencesByModelId = new Dictionary<long, IMediaReference<AnimationCollection>>();
        }

        /// <inheritdoc/>
        public override LoadedModel LoadImplementation(IReadOnlyList<Model> media, IReadOnlyList<string> paths = null)
        {
            var referencedTextures = new List<IMediaReference<Texture>>();
            var referencedShaders = new List<IMediaReference<Shader>>();
            var model = media[0];
            var path = paths != null && paths.Count > 0 ? paths[0] : null;
            var loadedMeshes = new List<LoadedMesh>();

            var embeddedTextureReferences = new List<IMediaReference<Texture>>();
            if (model.EmbeddedTextures != null)
            {
                foreach (var embeddedTexture in model.EmbeddedTextures)
                {
                    var embeddedTextureReference = _textureReferenceService.Reference(embeddedTexture);
                    embeddedTextureReferences.Add(embeddedTextureReference);
                    referencedTextures.Add(embeddedTextureReference);
                }
            }

            var animationCollectionReference = model.EmbeddedAnimationCollection != null
                ? _animationCollectionReferenceService.Reference(model.EmbeddedAnimationCollection)
                : null;

            foreach (var mesh in model.Meshes)
            {
                var textureReferences = new List<IMediaReference<Texture>>();
                var textureUsageTypes = new List<TextureUsageType>();

                foreach (var texture in mesh.MeshTextures)
                {
                    if (texture.Reference != null)
                    {
                        textureReferences.Add(texture.Reference);
                    }
                    else if (texture.EmbeddedTextureIndex != null)
                    {
                        textureReferences.Add(embeddedTextureReferences[(int)texture.EmbeddedTextureIndex.Value]);
                    }
                    else
                    {
                        var referencePath = texture.Path;
                        if (!_fileAccessService.IsPathFullyQualified(referencePath))
                        {
                            referencePath = !string.IsNullOrWhiteSpace(path)
                                ? _fileAccessService.GetFullyQualifiedRelativePath(path, referencePath)
                                : referencePath;
                        }

                        var textureReference = _textureReferenceService.Reference(referencePath);
                        textureReferences.Add(textureReference);
                        referencedTextures.Add(textureReference);
                    }

                    textureUsageTypes.Add(texture.UsageType);
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
                    else if (enabledVertexAttribute == VertexAttribute.BoneIndices)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => Enumerable.Range(0, _configuration.EnabledBoneCount).Select(i => vertex.BoneIndices != null && vertex.BoneIndices.Count > i ? vertex.BoneIndices[i] : 0)).ToArray(),
                                                                                                                  VertexAttribPointerType.Int,
                                                                                                                  _configuration.EnabledBoneCount));
                    }
                    else if (enabledVertexAttribute == VertexAttribute.BoneWeights)
                    {
                        arrayFactoriesWithTypeSizeTuples.Add(new Tuple<Func<Array>, VertexAttribPointerType, int>(() => mesh.Vertices.SelectMany(vertex => Enumerable.Range(0, _configuration.EnabledBoneCount).Select(i => vertex.BoneIndices != null && vertex.BoneWeights.Count > i ? vertex.BoneWeights[i] : 0.0f)).ToArray(),
                                                                                                                  VertexAttribPointerType.Float,
                                                                                                                  _configuration.EnabledBoneCount));
                    }
                }

                var vertexSizeBytes = arrayFactoriesWithTypeSizeTuples.Sum(t => GetSizeOfVertexAttribute(t.Item2) * t.Item3);
                var vertexBuffer = new byte[mesh.Vertices.Count * vertexSizeBytes];
                var destinationOffset = 0;

                var attribPointerTypeSizeOffsetTuples = new List<Tuple<VertexAttribPointerType, int, int>>();

                foreach (var arrayFactoriesWithTypeSizeTuple in arrayFactoriesWithTypeSizeTuples)
                {
                    attribPointerTypeSizeOffsetTuples.Add(new Tuple<VertexAttribPointerType, int, int>(arrayFactoriesWithTypeSizeTuple.Item2, arrayFactoriesWithTypeSizeTuple.Item3, destinationOffset));
                    destinationOffset = CopyValuesToVertexBuffer(arrayFactoriesWithTypeSizeTuple.Item1(), GetSizeOfVertexAttribute(arrayFactoriesWithTypeSizeTuple.Item2), vertexBuffer, destinationOffset);
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
                    if (IsIntegerVertexAttribute(tuple.Item1))
                    {
                        // Integer based types need to handled strictly or else they get converted into floats, which can be a bad time when using them for bone indices.
                        GL.VertexAttribIPointer(i, tuple.Item2, VertexAttribIntegerType.Int, tuple.Item2 * GetSizeOfVertexAttribute(tuple.Item1), (IntPtr)tuple.Item3);
                    }
                    else
                    {
                        GL.VertexAttribPointer(i, tuple.Item2, tuple.Item1, false, tuple.Item2 * GetSizeOfVertexAttribute(tuple.Item1), tuple.Item3);
                    }
                }

                loadedMeshes.Add(new LoadedMesh(vertexArrayId,
                                                vertexBufferId,
                                                vertexBufferLength,
                                                indexBufferId,
                                                indexBufferLength,
                                                new ReadOnlyCollection<IMediaReference<Texture>>(textureReferences.ToArray()),
                                                new ReadOnlyCollection<TextureUsageType>(textureUsageTypes.ToArray()),
                                                mesh.DefaultBlendMode));
            }

            var loadedSkeleton = model.SkeletonRoot != null
                ? GetLoadedSkeleton(model.SkeletonRoot, Matrix4.Identity)
                : null;

            var loadedModel = new LoadedModel(new ReadOnlyCollection<LoadedMesh>(loadedMeshes.ToArray()),
                                              loadedSkeleton,
                                              animationCollectionReference);

            if (referencedTextures.Count > 0)
            {
                _textureReferencesByModelId.Add(loadedModel.Id, referencedTextures);
            }

            return loadedModel;
        }

        /// <inheritdoc/>
        public override void UnloadImplementation(LoadedModel implementation)
        {
            if (_animationCollectionReferencesByModelId.TryGetValue(implementation.Id, out var referencedAnimationCollection))
            {
                _animationCollectionReferenceService.Unreference(referencedAnimationCollection);
                _animationCollectionReferencesByModelId.Remove(implementation.Id);
            }

            if (_textureReferencesByModelId.TryGetValue(implementation.Id, out var referencedTextures))
            {
                foreach (var referencedTexture in referencedTextures)
                {
                    _textureReferenceService.Unreference(referencedTexture);
                }

                _textureReferencesByModelId.Remove(implementation.Id);
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
            else if (attributeType == VertexAttribPointerType.Int)
            {
                return sizeof(int);
            }

            throw new ArgumentException($"Unrecognized {nameof(VertexAttribPointerType)}: {attributeType.ToString()}");
        }

        /// <summary>
        /// Checks to see if a <see cref="VertexAttribPointerType"/> is an integer based type, which needs to be handled differently than floats.
        /// </summary>
        /// <param name="attributeType">The <see cref="VertexAttribPointerType"/> to check.</param>
        /// <returns><c>true</c> if it is an integer based type.</returns>
        private bool IsIntegerVertexAttribute(VertexAttribPointerType attributeType)
        {
            return attributeType == VertexAttribPointerType.Int;
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

        /// <summary>
        /// Converts a domain <see cref="Bone"/> and its children to <see cref="LoadedBone"/> objects.
        /// </summary>
        /// <param name="bone">The <see cref="Bone"/> to convert.</param>
        /// <param name="parentOffsetMatrix">The <see cref="Matrix4"/> which represents the bone parent's total offset matrix.</param>
        /// <returns>The converted <see cref="LoadedBone"/>.</returns>
        private LoadedBone GetLoadedSkeleton(Bone bone, Matrix4 parentOffsetMatrix)
        {
            var boneLocalOffsetMatrix = new Matrix4(bone.OffsetMatrix.M11,
                                                    bone.OffsetMatrix.M12,
                                                    bone.OffsetMatrix.M13,
                                                    bone.OffsetMatrix.M14,
                                                    bone.OffsetMatrix.M21,
                                                    bone.OffsetMatrix.M22,
                                                    bone.OffsetMatrix.M23,
                                                    bone.OffsetMatrix.M24,
                                                    bone.OffsetMatrix.M31,
                                                    bone.OffsetMatrix.M32,
                                                    bone.OffsetMatrix.M33,
                                                    bone.OffsetMatrix.M34,
                                                    bone.OffsetMatrix.M41,
                                                    bone.OffsetMatrix.M42,
                                                    bone.OffsetMatrix.M43,
                                                    bone.OffsetMatrix.M44);
            boneLocalOffsetMatrix.Transpose();

            var boneGlobalOffsetMatrix = parentOffsetMatrix * boneLocalOffsetMatrix;

            var worldToBindMatrix = Matrix4.Invert(boneGlobalOffsetMatrix);

            var loadedChildren = new List<LoadedBone>();
            foreach (var child in bone.Children)
            {
                loadedChildren.Add(GetLoadedSkeleton(child, boneGlobalOffsetMatrix));
            }

            return new LoadedBone(bone.Name,
                                  worldToBindMatrix,
                                  new ReadOnlyCollection<LoadedBone>(loadedChildren.ToArray()));
        }
    }
}
