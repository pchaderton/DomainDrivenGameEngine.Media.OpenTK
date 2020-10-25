using System;
using System.Collections.Generic;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Graphics.OpenGL4;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading shader programs for use with OpenTK 4.0+.
    /// </summary>
    /// <remarks>
    /// If no configuration is provided, uses a default configuration which assumes each program
    /// needs a vertex shader and a fragment shader.
    /// </remarks>
    public class ProgramLoadingService : BaseMediaLoadingService<Shader, LoadedProgram>
    {
        /// <summary>
        /// The <see cref="ProgramLoadingConfiguration"/> to use for loading programs.
        /// </summary>
        private readonly ProgramLoadingConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramLoadingService"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="IMediaSourceService{Shader}"/>s to use for sourcing shaders.</param>
        public ProgramLoadingService(IMediaSourceService<Shader>[] sources)
            : this(sources, ProgramLoadingConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ProgramLoadingService"/> class.
        /// </summary>
        /// <param name="sources">The <see cref="IMediaSourceService{Shader}"/> s to use for sourcing shaders.</param>
        /// <param name="configuration">The <see cref="ProgramLoadingConfiguration"/> to use for loading programs.</param>
        public ProgramLoadingService(IMediaSourceService<Shader>[] sources, ProgramLoadingConfiguration configuration)
            : base(sources, new uint[] { (uint)configuration.ShaderTypes.Count })
        {
            _configuration = configuration;
        }

        /// <inheritdoc/>
        protected override LoadedProgram LoadImplementation(params Shader[] media)
        {
            var shaderIds = new List<int>();
            var shaderIndex = 0;
            foreach (var shaderType in _configuration.ShaderTypes)
            {
                shaderIds.Add(CompileShader(media[shaderIndex++], shaderType));
            }

            var programId = GL.CreateProgram();

            foreach (var shaderId in shaderIds)
            {
                GL.AttachShader(programId, shaderId);
            }

            GL.LinkProgram(programId);

            foreach (var shaderId in shaderIds)
            {
                GL.DetachShader(programId, shaderId);
                GL.DeleteShader(shaderId);
            }

            return new LoadedProgram(programId);
        }

        /// <inheritdoc/>
        protected override void UnloadImplementation(LoadedProgram implementation)
        {
            GL.DeleteProgram(implementation.ProgramId);
        }

        /// <summary>
        /// Compiles a shader.
        /// </summary>
        /// <param name="shader">The <see cref="Shader"/> to compile.</param>
        /// <param name="shaderType">The <see cref="ShaderType"/> to compile it to.</param>
        /// <returns>The ID of the compiled OpenGL shader.</returns>
        private int CompileShader(Shader shader, ShaderType shaderType)
        {
            var shaderId = GL.CreateShader(shaderType);

            GL.ShaderSource(shaderId, shader.Source);

            GL.GetShader(shaderId, ShaderParameter.CompileStatus, out int isCompiled);

            if (isCompiled == 0)
            {
                var exception = new Exception("Failed to compile shader.");

                exception.Data["ShaderInfoLog"] = GL.GetShaderInfoLog(shaderId);

                throw exception;
            }

            return shaderId;
        }
    }
}
