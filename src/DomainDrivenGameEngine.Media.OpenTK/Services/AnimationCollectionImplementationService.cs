using System.Collections.Generic;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Mathematics;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading animations for use with OpenTK 4.0+.
    /// </summary>
    public class AnimationCollectionImplementationService : IMediaImplementationService<AnimationCollection, LoadedAnimationCollection>
    {
        /// <inheritdoc/>
        public bool IsSourceStreamRequired => false;

        /// <inheritdoc/>
        public bool IsMediaCountSupported(uint count)
        {
            return count > 0;
        }

        /// <inheritdoc/>
        public LoadedAnimationCollection LoadImplementation(IReadOnlyCollection<AnimationCollection> media,
                                                            IReadOnlyCollection<string> paths = null)
        {
            // Process every animation for every provided media to return to the resulting collection.
            var loadedAnimations = new List<LoadedAnimation>();
            foreach (var animationCollection in media)
            {
                foreach (var animation in animationCollection)
                {
                    var loadedChannels = new List<LoadedChannel>();
                    foreach (var channel in animation.Channels)
                    {
                        var rotationKeyFrames = channel.RotationKeyFrames
                                                       .Select(kf => new KeyFrame<Quaternion>(kf.TimeInSeconds, new Quaternion(kf.Value.X, kf.Value.Y, kf.Value.Z, kf.Value.W)))
                                                       .ToList();
                        var offsetKeyFrames = channel.OffsetKeyFrames
                                                     .Select(kf => new KeyFrame<Vector3>(kf.TimeInSeconds, new Vector3(kf.Value.X, kf.Value.Y, kf.Value.Z)))
                                                     .ToList();
                        var scaleKeyFrames = channel.ScaleKeyFrames
                                                    .Select(kf => new KeyFrame<Vector3>(kf.TimeInSeconds, new Vector3(kf.Value.X, kf.Value.Y, kf.Value.Z)))
                                                    .ToList();

                        loadedChannels.Add(new LoadedChannel(channel.BoneName, rotationKeyFrames, offsetKeyFrames, scaleKeyFrames));
                    }

                    loadedAnimations.Add(new LoadedAnimation(animation.Name, loadedChannels, animation.DurationInSeconds));
                }
            }

            // De-duplicate by name and add to the final collection.
            return new LoadedAnimationCollection(loadedAnimations.GroupBy(a => a.Name).Select(g => g.Last()).ToList());
        }

        /// <inheritdoc/>
        public void UnloadImplementation(LoadedAnimationCollection implementation)
        {
        }
    }
}
