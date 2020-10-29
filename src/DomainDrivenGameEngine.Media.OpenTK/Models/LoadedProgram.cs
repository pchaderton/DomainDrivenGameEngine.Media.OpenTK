using System;
using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// A loaded program for use with OpenTK.
    /// </summary>
    public class LoadedProgram : IMediaImplementation<Shader>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LoadedProgram"/> class.
        /// </summary>
        /// <param name="programId">The OpenGL program ID for this implementation.</param>
        public LoadedProgram(int programId)
        {
            if (programId <= 0)
            {
                throw new ArgumentException($"A valid {nameof(programId)} is required.");
            }

            ProgramId = programId;
        }

        /// <summary>
        /// Gets the OpenGL program ID for this implementation.
        /// </summary>
        public int ProgramId { get; }
    }
}
