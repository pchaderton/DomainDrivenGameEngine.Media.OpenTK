using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using OpenTK.Audio.OpenAL;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded piece of music.
    /// </summary>
    public class LoadedMusic : IMediaImplementation<Music>, IDisposable
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedMusic"/> class.
        /// </summary>
        /// <param name="music">The source <see cref="Music"/> used for streaming the music into buffers.</param>
        /// <param name="bufferIds">The IDs of the OpenAL buffers to use for buffering this music.</param>
        /// <param name="bufferSize">The expected size of each buffer for this music.</param>
        /// <param name="format">The format of the music.</param>
        /// <param name="sampleRate">The sample rate of the music.</param>
        public LoadedMusic(Music music,
                           IEnumerable<int> bufferIds,
                           uint bufferSize,
                           ALFormat format,
                           uint sampleRate)
        {
            Music = music ?? throw new ArgumentNullException(nameof(music));
            BufferIds = new ReadOnlyCollection<int>(bufferIds?.ToArray() ?? throw new ArgumentNullException(nameof(bufferIds)));
            BufferSize = bufferSize;
            Format = format;
            SampleRate = sampleRate;
        }

        /// <summary>
        /// Gets the IDs of the OpenAL buffers to use for buffering this music.
        /// </summary>
        public IReadOnlyList<int> BufferIds { get; }

        /// <summary>
        /// Gets the expected size of each buffer for this music.
        /// </summary>
        public uint BufferSize { get; }

        /// <summary>
        /// Gets the source <see cref="Music"/> used for streaming the music into buffers.
        /// </summary>
        public Music Music { get; }

        /// <summary>
        /// Gets the format of the music.
        /// </summary>
        public ALFormat Format { get; }

        /// <summary>
        /// Gets the sample rate of the music.
        /// </summary>
        public uint SampleRate { get; }

        /// <summary>
        /// Disposes of this loaded music.
        /// </summary>
        public void Dispose()
        {
            Music.Dispose();
        }
    }
}
