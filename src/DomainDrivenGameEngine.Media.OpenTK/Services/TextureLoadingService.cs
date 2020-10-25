using System;
using System.Linq;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using DomainDrivenGameEngine.Media.Services;
using OpenTK.Graphics.OpenGL4;
using DomainPixelFormat = DomainDrivenGameEngine.Media.Models.PixelFormat;
using OpenTKPixelFormat = OpenTK.Graphics.OpenGL4.PixelFormat;

namespace DomainDrivenGameEngine.Media.OpenTK.Services
{
    /// <summary>
    /// A service for loading textures for use with OpenTK 4.0+.  Supports loading individual textures, cube map textures and packed textures from two, three or four sources.
    /// </summary>
    /// <remarks>
    /// When loading cube map textures, six textures must be provided in the following order: xpos, xneg, ypos, yneg, zpos, zneg.
    /// </remarks>
    public class TextureLoadingService : BaseMediaLoadingService<Texture, LoadedTexture>
    {
        /// <summary>
        /// The <see cref="TextureLoadingConfiguration"/> to use when loading textures.
        /// </summary>
        private readonly TextureLoadingConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoadingService"/> class.
        /// </summary>
        /// <param name="mediaSources">The <see cref="IMediaSourceService{Texture}"/>s to use for sourcing textures.</param>
        public TextureLoadingService(IMediaSourceService<Texture>[] mediaSources)
            : this(mediaSources, TextureLoadingConfiguration.Default)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoadingService"/> class.
        /// </summary>
        /// <param name="mediaSources">The <see cref="IMediaSourceService{Texture}"/>s to use for sourcing textures.</param>
        /// <param name="configuration">The <see cref="TextureLoadingConfiguration"/> to use when loading textures.</param>
        public TextureLoadingService(IMediaSourceService<Texture>[] mediaSources, TextureLoadingConfiguration configuration)
            : base(mediaSources, new uint[] { 1, 2, 3, 4, 6 })
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        protected override LoadedTexture LoadImplementation(params Texture[] media)
        {
            if (media.Length == 6)
            {
                return LoadCubeMapTexture(media[0], media[1], media[2], media[3], media[4], media[5]);
            }
            else if (media.Length >= 2 && media.Length <= 4)
            {
                return LoadPackedTexture(media[0],
                                         media[1],
                                         media.Length >= 3 ? media[2] : null,
                                         media.Length >= 4 ? media[3] : null);
            }

            return LoadSingleTexture(media[0]);
        }

        /// <inheritdoc/>
        protected override void UnloadImplementation(LoadedTexture implementation)
        {
            GL.DeleteTexture(implementation.TextureId);
        }

        /// <summary>
        /// Asserts that a pixel format is one this service supports loading.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="DomainPixelFormat"/> to assert against.</param>
        private void AssertSupportedPixelFormat(DomainPixelFormat pixelFormat)
        {
            if (pixelFormat == DomainPixelFormat.Rgb8 || pixelFormat == DomainPixelFormat.Rgba8)
            {
                return;
            }

            throw new ArgumentException($"{nameof(pixelFormat)} {pixelFormat.ToString()} is unsupported.");
        }

        /// <summary>
        /// Generates mip maps and configures texture parameters for the currently bound texture.
        /// </summary>
        /// <param name="mipmapTarget">The <see cref="GenerateMipmapTarget"/> to use for generating mip maps.</param>
        /// <param name="textureTarget">The <see cref="TextureTarget"/> to use for configuring parameters.</param>
        private void ConfigureBoundTexture(GenerateMipmapTarget mipmapTarget, TextureTarget textureTarget)
        {
            if (_configuration.GenerateMipmaps)
            {
                GL.GenerateMipmap(mipmapTarget);
            }

            foreach (var parameterKvp in _configuration.TextureParameters)
            {
                GL.TexParameter(textureTarget, parameterKvp.Key, parameterKvp.Value);
            }
        }

        /// <summary>
        /// Converts an array of bytes using <see cref="DomainPixelFormat"/> Rgb8 to Rgba8.
        /// </summary>
        /// <param name="bytes">The array of bytes to convert.</param>
        /// <returns>The converted array of bytes.</returns>
        private byte[] ConvertBytesToRgba8(byte[] bytes)
        {
            var newBytes = new byte[bytes.Length / 3 * 4];
            for (int newIndex = 0, oldIndex = 0; newIndex < newBytes.Length; newIndex += 4, oldIndex += 3)
            {
                newBytes[newIndex] = bytes[oldIndex];
                newBytes[newIndex + 1] = bytes[oldIndex + 1];
                newBytes[newIndex + 2] = bytes[oldIndex + 2];
                newBytes[newIndex + 3] = 255;
            }

            return newBytes;
        }

        /// <summary>
        /// Loads a set of six <see cref="Texture"/> objects into a single <see cref="LoadedTexture"/> for a cube map.
        /// </summary>
        /// <param name="xpos">The <see cref="Texture"/> to use for the positive x direction.</param>
        /// <param name="xneg">The <see cref="Texture"/> to use for the negative x direction.</param>
        /// <param name="ypos">The <see cref="Texture"/> to use for the positive y direction.</param>
        /// <param name="yneg">The <see cref="Texture"/> to use for the negative y direction.</param>
        /// <param name="zpos">The <see cref="Texture"/> to use for the positive z direction.</param>
        /// <param name="zneg">The <see cref="Texture"/> to use for the negative z direction.</param>
        /// <returns>A <see cref="LoadedTexture"/> object.</returns>
        private LoadedTexture LoadCubeMapTexture(Texture xpos, Texture xneg, Texture ypos, Texture yneg, Texture zpos, Texture zneg)
        {
            var textures = new[] { xpos, xneg, ypos, yneg, zpos, zneg };
            foreach (var texture in textures)
            {
                AssertSupportedPixelFormat(texture.Format);
            }

            var textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.TextureCubeMap, textureId);

            for (var index = 0; index < 6; index++)
            {
                var texture = textures[index];
                var bytes = texture.Bytes.ToArray();
                if (texture.Format == DomainPixelFormat.Rgb8)
                {
                    bytes = ConvertBytesToRgba8(bytes);
                }

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + index, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, OpenTKPixelFormat.Rgba, PixelType.UnsignedByte, bytes);
            }

            ConfigureBoundTexture(GenerateMipmapTarget.TextureCubeMap, TextureTarget.TextureCubeMap);

            return new LoadedTexture(textureId);
        }

        /// <summary>
        /// Loads multiple <see cref="Texture"/> objects into a <see cref="LoadedTexture"/> by packing each texture into a single color channel.
        /// </summary>
        /// <param name="redTexture">The <see cref="Texture"/> to use as the red channel.</param>
        /// <param name="greenTexture">The <see cref="Texture"/> to use as the green channel.</param>
        /// <param name="blueTexture">The <see cref="Texture"/> to use as the blue channel.</param>
        /// <param name="alphaTexture">The <see cref="Texture"/> to use as the alpha channel.</param>
        /// <returns>A <see cref="LoadedTexture"/> object.</returns>
        private LoadedTexture LoadPackedTexture(Texture redTexture, Texture greenTexture, Texture blueTexture, Texture alphaTexture)
        {
            var textures = new[] { redTexture, greenTexture, blueTexture, alphaTexture };
            var expectedWidth = redTexture.Width;
            var expectedHeight = redTexture.Height;
            foreach (var texture in textures)
            {
                AssertSupportedPixelFormat(texture.Format);
                if (expectedWidth != texture.Width || expectedHeight != texture.Height)
                {
                    throw new ArgumentException($"All textures used for a packed texture must be the same resolution.");
                }
            }

            var bytes = new byte[expectedWidth * expectedHeight * 4];
            for (var index = 0; index < 4; index++)
            {
                var sourceTexture = textures.ElementAtOrDefault(index);
                if (sourceTexture != null)
                {
                    var pixelFormatDetails = PixelFormatDetailsAttribute.GetPixelFormatDetails(sourceTexture.Format);
                    var bytesPerPixel = pixelFormatDetails.BytesPerPixel;
                    var sourceBytes = sourceTexture.Bytes.ToArray();
                    var sourceIndex = 0;
                    for (var destIndex = index; destIndex < bytes.Length; destIndex += 4)
                    {
                        bytes[destIndex] = sourceBytes[sourceIndex];
                        sourceIndex += bytesPerPixel;
                    }
                }
                else
                {
                    for (var destIndex = index; destIndex < bytes.Length; destIndex += 4)
                    {
                        bytes[destIndex] = 255;
                    }
                }
            }

            return LoadSingleTexture(new Texture(expectedWidth, expectedHeight, DomainPixelFormat.Rgba8, bytes));
        }

        /// <summary>
        /// Loads a single <see cref="Texture"/> into a <see cref="LoadedTexture"/>.
        /// </summary>
        /// <param name="texture">The texture to load from.</param>
        /// <returns>A <see cref="LoadedTexture"/> object.</returns>
        private LoadedTexture LoadSingleTexture(Texture texture)
        {
            AssertSupportedPixelFormat(texture.Format);

            var textureId = GL.GenTexture();

            GL.BindTexture(TextureTarget.Texture2D, textureId);

            byte[] bytes = texture.Bytes.ToArray();
            if (texture.Format == DomainPixelFormat.Rgb8)
            {
                bytes = ConvertBytesToRgba8(bytes);
            }

            // Don't generate extra storage for textures that are too small to properly utilize it.
            GL.TexStorage2D(TextureTarget2d.Texture2D, texture.Width >= 16 && texture.Height >= 16 ? 4 : 1, SizedInternalFormat.Rgba8, texture.Width, texture.Height);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, OpenTKPixelFormat.Rgba, PixelType.UnsignedByte, bytes);

            ConfigureBoundTexture(GenerateMipmapTarget.Texture2D, TextureTarget.Texture2D);

            return new LoadedTexture(textureId);
        }
    }
}
