using System;
using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Audio.OpenAL;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading music for use with OpenTK 4.0+.
    /// </summary>
    /// <remarks>
    /// If no configuration is provided, uses a default configuration which assumes each music
    /// uses four 32kb buffers for streaming audio data.
    /// </remarks>
    public class MusicImplementationService : BaseMediaImplementationService<Music, LoadedMusic>
    {
        /// <summary>
        /// The <see cref="MusicLoadingConfiguration"/> to use for loading programs.
        /// </summary>
        private readonly MusicLoadingConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicImplementationService"/> class.
        /// </summary>
        public MusicImplementationService()
            : this(MusicLoadingConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicImplementationService"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="MusicLoadingConfiguration"/> to use for loading music.</param>
        public MusicImplementationService(MusicLoadingConfiguration configuration)
            : base(isSourceStreamRequired: true)
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        public override LoadedMusic LoadImplementation(IReadOnlyList<Music> media, IReadOnlyList<string> paths = null)
        {
            var music = media[0];

            var format = music.Channels == 2
                ? ALFormat.Stereo16
                : ALFormat.Mono16;

            // Prepare the initial buffers for the music so it can start playing immediately.
            var bufferIds = new List<int>();
            for (var i = 0; i < _configuration.BufferCount; i++)
            {
                var bufferId = AL.GenBuffer();
                var buffer = new byte[_configuration.BufferSize];
                Array.Fill<byte>(buffer, 0);
                music.Stream.Read(buffer, 0, (int)_configuration.BufferCount);

                AL.BufferData(bufferId, format, buffer, music.SampleRate);

                bufferIds.Add(bufferId);
            }

            return new LoadedMusic(music.Stream,
                                   bufferIds,
                                   _configuration.BufferSize,
                                   format,
                                   (uint)music.SampleRate);
        }

        /// <inheritdoc/>
        public override void UnloadImplementation(LoadedMusic implementation)
        {
            foreach (var bufferId in implementation.BufferIds)
            {
                AL.DeleteBuffer(bufferId);
            }

            // We need to manually dispose of the stream here as this service tells loading services to maintain it.
            implementation.Stream.Dispose();
        }
    }
}
