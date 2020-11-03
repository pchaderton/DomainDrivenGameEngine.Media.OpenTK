using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// An animation for a <see cref="LoadedModel"/>.
    /// </summary>
    public class LoadedAnimation
    {
        /// <summary>
        /// A lookup of channels in this animation keyed by name.
        /// </summary>
        private readonly IDictionary<string, LoadedChannel> _channelsByBoneName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedAnimation"/> class.
        /// </summary>
        /// <param name="name">The name of this animation.</param>
        /// <param name="channels">The channels in this animation.</param>
        /// <param name="durationInSeconds">The duration of the animation in seconds.</param>
        public LoadedAnimation(string name, IReadOnlyCollection<LoadedChannel> channels, double durationInSeconds)
        {
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Channels = channels ?? throw new ArgumentNullException(nameof(channels));
            DurationInSeconds = durationInSeconds;
            _channelsByBoneName = Channels.ToDictionary(lc => lc.BoneName);
        }

        /// <summary>
        /// Gets the channels in this animation.
        /// </summary>
        public IReadOnlyCollection<LoadedChannel> Channels { get; }

        /// <summary>
        /// Gets the duration of the animation in seconds.
        /// </summary>
        public double DurationInSeconds { get; }

        /// <summary>
        /// Gets the name of this animation.
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Tries to get a channel in the animation by bone name.
        /// </summary>
        /// <param name="boneName">The name of the bone to get the channel for.</param>
        /// <param name="outputChannel">The output channel if found, <c>null</c> otherwise.</param>
        /// <returns><c>true</c> if the channel was found.</returns>
        public bool TryGetChannelByBoneName(string boneName, out LoadedChannel outputChannel)
        {
            return _channelsByBoneName.TryGetValue(boneName, out outputChannel);
        }
    }
}
