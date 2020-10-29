using System;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded texture for use with OpenTK.
    /// </summary>
    public class LoadedTexture : IMediaImplementation<Texture>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedTexture"/> class.
        /// </summary>
        /// <param name="textureId">The OpenGL texture ID for this texture.</param>
        public LoadedTexture(int textureId)
        {
            if (textureId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(textureId)} is required.");
            }

            TextureId = textureId;
        }

        /// <summary>
        /// Gets the OpenGL texture ID for this texture.
        /// </summary>
        public int TextureId { get; }
    }
}
