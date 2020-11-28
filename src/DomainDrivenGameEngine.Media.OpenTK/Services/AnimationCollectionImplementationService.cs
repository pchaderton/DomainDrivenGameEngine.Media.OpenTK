using System.Collections.Generic;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;

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
        public LoadedAnimationCollection LoadImplementation(IReadOnlyList<AnimationCollection> media,
                                                            IReadOnlyList<string> paths = null)
        {
            // Process every animation for every provided media to return to the resulting collection.
            var loadedAnimations = media.SelectMany(ac => ac).ToList();

            // De-duplicate by name and add to the final collection.
            return new LoadedAnimationCollection(loadedAnimations.GroupBy(a => a.Name).Select(g => g.First()).ToList());
        }

        /// <inheritdoc/>
        public void UnloadImplementation(LoadedAnimationCollection implementation)
        {
        }
    }
}
