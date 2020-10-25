using System;
using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded model for use with OpenTK.
    /// </summary>
    public class LoadedModel : IMediaImplementation
    {
        /// <summary>
        /// A counter for the ID of the next generated model.
        /// </summary>
        private static int _idCounter = 0;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedModel"/> class.
        /// </summary>
        /// <param name="meshes">The meshes in this model.</param>
        public LoadedModel(IReadOnlyCollection<LoadedMesh> meshes)
        {
            Meshes = meshes ?? throw new ArgumentNullException(nameof(meshes));
            Id = ++_idCounter;
        }

        /// <summary>
        /// Gets the ID of this model.
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the meshes in this model.
        /// </summary>
        public IReadOnlyCollection<LoadedMesh> Meshes { get; }
    }
}
