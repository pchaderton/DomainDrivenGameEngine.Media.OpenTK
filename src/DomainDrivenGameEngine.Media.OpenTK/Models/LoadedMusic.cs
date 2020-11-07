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
    public class LoadedMusic : IMediaImplementation<Music>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedMusic"/> class.
        /// </summary>
        /// <param name="stream">The stream used for streaming the music into buffers.</param>
        /// <param name="bufferIds">The IDs of the OpenAL buffers to use for buffering this music.</param>
        /// <param name="bufferSize">The expected size of each buffer for this music.</param>
        /// <param name="format">The format of the music.</param>
        /// <param name="sampleRate">The sample rate of the music.</param>
        public LoadedMusic(Stream stream,
                           IEnumerable<int> bufferIds,
                           uint bufferSize,
                           ALFormat format,
                           uint sampleRate)
        {
            Stream = stream ?? throw new ArgumentNullException(nameof(stream));
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
        /// Gets the stream used for streaming the music into buffers.
        /// </summary>
        public Stream Stream { get; }

        /// <summary>
        /// Gets the format of the music.
        /// </summary>
        public ALFormat Format { get; }

        /// <summary>
        /// Gets the sample rate of the music.
        /// </summary>
        public uint SampleRate { get; }
    }
}
