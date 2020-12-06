using System.Collections.Generic;
using System.Linq;
using DomainDrivenGameEngine.Media.Loaders;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using OpenTK.Audio.OpenAL;

namespace DomainDrivenGameEngine.Media.OpenTK.Loaders
{
    /// <summary>
    /// A service for loading sound effects.
    /// </summary>
    public class SoundEffectLoader : BaseMediaLoader<SoundEffect, LoadedSoundEffect>
    {
        /// <inheritdoc/>
        public override LoadedSoundEffect Load(IReadOnlyList<SoundEffect> media, IReadOnlyList<string> paths = null)
        {
            var soundEffect = media[0];

            var bufferId = AL.GenBuffer();

            AL.BufferData(bufferId,
                          soundEffect.Channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16,
                          soundEffect.Bytes.ToArray(),
                          soundEffect.SampleRate);

            return new LoadedSoundEffect(bufferId);
        }

        /// <inheritdoc/>
        public override void Unload(LoadedSoundEffect implementation)
        {
            AL.DeleteBuffer(implementation.BufferId);
        }
    }
}
