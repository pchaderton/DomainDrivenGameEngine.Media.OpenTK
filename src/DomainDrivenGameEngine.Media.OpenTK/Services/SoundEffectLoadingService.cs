using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Audio.OpenAL;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading sound effects.
    /// </summary>
    public class SoundEffectLoadingService : BaseMediaLoadingService<SoundEffect, LoadedSoundEffect>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SoundEffectLoadingService"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="IMediaSourceService{SoundEffect}"/>s to use for sourcing sound effects.</param>
        public SoundEffectLoadingService(IMediaSourceService<SoundEffect>[] sources)
            : base(sources)
        {
        }

        /// <inheritdoc/>
        protected override LoadedSoundEffect LoadImplementation(params SoundEffect[] media)
        {
            var soundEffect = media.First();

            var bufferId = AL.GenBuffer();

            AL.BufferData(bufferId,
                          soundEffect.Channels == 2 ? ALFormat.Stereo16 : ALFormat.Mono16,
                          soundEffect.Bytes,
                          soundEffect.SampleRate);

            return new LoadedSoundEffect(bufferId);
        }

        /// <inheritdoc/>
        protected override void UnloadImplementation(LoadedSoundEffect implementation)
        {
            AL.DeleteBuffer(implementation.BufferId);
        }
    }
}
