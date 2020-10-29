using System;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded sound effect for use with OpenTK.
    /// </summary>
    public class LoadedSoundEffect : IMediaImplementation<SoundEffect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedSoundEffect"/> class.
        /// </summary>
        /// <param name="bufferId">The OpenAL buffer ID referring to the loaded sound effect.</param>
        public LoadedSoundEffect(int bufferId)
        {
            if (bufferId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(bufferId)} is required.");
            }

            BufferId = bufferId;
        }

        /// <summary>
        /// Gets the OpenAL buffer ID referring to the loaded sound effect.
        /// </summary>
        public int BufferId { get; }
    }
}
