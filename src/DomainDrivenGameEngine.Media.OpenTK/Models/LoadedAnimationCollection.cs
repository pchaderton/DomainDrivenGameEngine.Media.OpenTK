using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A collection of loaded animation data.
    /// </summary>
    public class LoadedAnimationCollection : ReadOnlyCollection<LoadedAnimation>, IMediaImplementation<AnimationCollection>
    {
        /// <summary>
        /// The animations in this collection keyed by name.
        /// </summary>
        private readonly IDictionary<string, LoadedAnimation> _animationsByName;

        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedAnimationCollection"/> class.
        /// </summary>
        /// <param name="animations">The animations in this animation set.</param>
        public LoadedAnimationCollection(IList<LoadedAnimation> animations)
            : base(animations ?? throw new ArgumentNullException(nameof(animations)))
        {
            _animationsByName = this.ToDictionary(a => a.Name);
        }

        /// <summary>
        /// Tries to get a loaded animation by name.
        /// </summary>
        /// <param name="name">The name of the animation.</param>
        /// <param name="outputAnimation">The output animation, or <c>null</c> if not found.</param>
        /// <returns><c>true</c> if the animation was found.</returns>
        public bool TryGetAnimationByName(string name, out LoadedAnimation outputAnimation)
        {
            return _animationsByName.TryGetValue(name, out outputAnimation);
        }
    }
}
