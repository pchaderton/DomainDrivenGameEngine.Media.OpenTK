using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Audio.OpenAL;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading sound effects.
    /// </summary>
    public class SoundEffectImplementationService : BaseMediaImplementationService<SoundEffect, LoadedSoundEffect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundEffectImplementationService"/> class.
        /// </summary>
        public SoundEffectImplementationService()
        {
        }

        /// <inheritdoc/>
        public override LoadedSoundEffect LoadImplementation(IReadOnlyList<SoundEffect> media, IReadOnlyList<string> paths = null)
        {
            var soundEffect = media[0];

            var bufferId = AL.GenBuffer();

            AL.BufferData(bufferId,
                          soundEffect.Channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16,
                          soundEffect.Bytes,
                          soundEffect.SampleRate);

            return new LoadedSoundEffect(bufferId);
        }

        /// <inheritdoc/>
        public override void UnloadImplementation(LoadedSoundEffect implementation)
        {
            AL.DeleteBuffer(implementation.BufferId);
        }
    }
}
