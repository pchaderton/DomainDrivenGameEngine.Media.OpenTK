using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using DomainDrivenGameEngine.Media.Models;
using OpenTK.Mathematics;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded animation channel using OpenTK math types.
    /// </summary>
    public class LoadedChannel
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedChannel"/> class.
        /// </summary>
        /// <param name="boneName">The name of the bone that this channel is applied to.</param>
        /// <param name="rotationKeyFrames">The keyframes for this channel.</param>
        /// <param name="offsetKeyFrames">The offset keyframes for this channel.</param>
        /// <param name="scaleKeyFrames">The scaling keyframes for this channel.</param>
        public LoadedChannel(string boneName,
                             ReadOnlyCollection<KeyFrame<Quaternion>> rotationKeyFrames,
                             ReadOnlyCollection<KeyFrame<Vector3>> offsetKeyFrames,
                             ReadOnlyCollection<KeyFrame<Vector3>> scaleKeyFrames)
        {
            BoneName = boneName ?? throw new ArgumentNullException(nameof(boneName));
            RotationKeyFrames = rotationKeyFrames ?? throw new ArgumentNullException(nameof(rotationKeyFrames));
            OffsetKeyFrames = offsetKeyFrames ?? throw new ArgumentNullException(nameof(offsetKeyFrames));
            ScaleKeyFrames = scaleKeyFrames ?? throw new ArgumentNullException(nameof(scaleKeyFrames));
        }

        /// <summary>
        /// Gets the name of the bone that this channel is applied to.
        /// </summary>
        public string BoneName { get; }

        /// <summary>
        /// Gets the offset keyframes for this channel.
        /// </summary>
        public IReadOnlyList<KeyFrame<Vector3>> OffsetKeyFrames { get; }

        /// <summary>
        /// Gets the rotation keyframes for this channel.
        /// </summary>
        public IReadOnlyList<KeyFrame<Quaternion>> RotationKeyFrames { get; }

        /// <summary>
        /// Gets the scale keyframes for this channel.
        /// </summary>
        public IReadOnlyList<KeyFrame<Vector3>> ScaleKeyFrames { get; }
    }
}
