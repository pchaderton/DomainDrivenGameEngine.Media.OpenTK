using System;
using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded model for use with OpenTK.
    /// </summary>
    public class LoadedModel : IMediaImplementation<Model>
    {
        /// <summary>
        /// A counter for the ID of the next generated model.
        /// </summary>
        private static int _idCounter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedModel"/> class.
        /// </summary>
        /// <param name="meshes">The meshes in this model.</param>
        /// <param name="skeletonRoot">The skeleton root for the model.</param>
        /// <param name="animationCollectionReference">The reference to the animation collection that was loaded with this model.</param>
        public LoadedModel(IReadOnlyCollection<LoadedMesh> meshes,
                           Bone skeletonRoot = null,
                           IMediaReference<AnimationCollection> animationCollectionReference = null)
        {
            Meshes = meshes ?? throw new ArgumentNullException(nameof(meshes));
            SkeletonRoot = skeletonRoot;
            AnimationCollectionReference = animationCollectionReference;
            Id = ++_idCounter;
        }

        /// <summary>
        /// Gets the reference to the animation collection that was loaded with this model.
        /// </summary>
        public IMediaReference<AnimationCollection> AnimationCollectionReference { get; }

        /// <summary>
        /// Gets the ID of this model.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the meshes in this model.
        /// </summary>
        public IReadOnlyCollection<LoadedMesh> Meshes { get; }

        /// <summary>
        /// Gets the skeleton root of this model.
        /// </summary>
        public Bone SkeletonRoot { get; }
    }
}
