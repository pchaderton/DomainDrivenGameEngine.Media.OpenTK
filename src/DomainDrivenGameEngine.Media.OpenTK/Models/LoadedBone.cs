using System;
using System.Collections.Generic;
using OpenTK.Mathematics;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A bone from a loaded model, prepared for use with OpenTK 4.0+.
    /// </summary>
    public class LoadedBone
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedBone"/> class.
        /// </summary>
        /// <param name="name">The bone name.</param>
        /// <param name="worldToBindMatrix">The matrix used to convert from world space to bind space.</param>
        /// <param name="children">The child bones under this bone.  Each bone in this collection will have their parent set.</param>
        public LoadedBone(string name, Matrix4 worldToBindMatrix, IReadOnlyCollection<LoadedBone> children)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            WorldToBindMatrix = worldToBindMatrix;
            Children = children ?? throw new ArgumentNullException(nameof(children));
            Parent = null;

            foreach (var child in Children)
            {
                child.Parent = this;
            }
        }

        /// <summary>
        /// Gets the child bones under this bone.
        /// </summary>
        public IReadOnlyCollection<LoadedBone> Children { get; }

        /// <summary>
        /// Gets the bone name.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the parent bone of this bone.
        /// </summary>
        public LoadedBone Parent { get; private set; }

        /// <summary>
        /// Gets the matrix used to convert from world space to bind space.
        /// </summary>
        public Matrix4 WorldToBindMatrix { get; }
    }
}
