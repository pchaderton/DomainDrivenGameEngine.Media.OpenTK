namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// The configuration to use for loading music.
    /// </summary>
    public class MusicLoadingConfiguration
    {
        /// <summary>
        /// The default configuration to use for loading music, defaults to four 32kb buffers.
        /// </summary>
        public static readonly MusicLoadingConfiguration Default = new MusicLoadingConfiguration(4, 32 * 1024);

        /// <summary>
        /// Initializes a new instance of the <see cref="MusicLoadingConfiguration"/> class.
        /// </summary>
        /// <param name="bufferCount">The number of buffers to reserve for a piece of music.</param>
        /// <param name="bufferSize">The amount of data to stream into each buffer.</param>
        public MusicLoadingConfiguration(uint bufferCount, uint bufferSize)
        {
            BufferCount = bufferCount;
            BufferSize = bufferSize;
        }

        /// <summary>
        /// Gets the number of buffers to reserve for a piece of music.
        /// </summary>
        public uint BufferCount { get; }

        /// <summary>
        /// Gets the amount of data to stream into each buffer.
        /// </summary>
        public uint BufferSize { get; }
    }
}
