using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DomainDrivenGameEngine.Media.Loaders;
using DomainDrivenGameEngine.Media.Models;
using DomainDrivenGameEngine.Media.OpenTK.Models;
using OpenTK.Graphics.OpenGL4;

namespace DomainDrivenGameEngine.Media.OpenTK.Loaders
{
    /// <summary>
    /// A service for loading textures for use with OpenTK 4.0+.  Supports loading individual textures, cube map textures and packed textures from two, three or four sources.
    /// </summary>
    /// <remarks>
    /// When loading cube map textures, six textures must be provided in the following order: xpos, xneg, ypos, yneg, zpos, zneg.
    /// </remarks>
    public class TextureLoader : BaseMediaLoader<Texture, LoadedTexture>
    {
        /// <summary>
        /// The <see cref="TextureLoadingConfiguration"/> to use when loading textures.
        /// </summary>
        private readonly TextureLoadingConfiguration _configuration;

        /// <summary>
        /// Initializes a new instance of the <see cref="TextureLoader"/> class.
        /// </summary>
        /// <param name="configuration">The <see cref="TextureLoadingConfiguration"/> to use when loading textures.</param>
        public TextureLoader(TextureLoadingConfiguration configuration)
            : base(new uint[] { 1, 2, 3, 4, 6 })
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        /// <inheritdoc/>
        public override LoadedTexture Load(IReadOnlyList<Texture> media, IReadOnlyList<string> paths = null)
        {
            if (media.Count == 6)
            {
                return LoadCubeMapTexture(media[0],
                                          media[1],
                                          media[2],
                                          media[3],
                                          media[4],
                                          media[5]);
            }
            else if (media.Count >= 2 && media.Count <= 4)
            {
                return LoadPackedTexture(media[0],
                                         media[1],
                                         media.Count > 2 ? media[2] : null,
                                         media.Count > 3 ? media[3] : null);
            }

            return LoadSingleTexture(media[0]);
        }

        /// <inheritdoc/>
        public override void Unload(LoadedTexture implementation)
        {
            GL.DeleteTexture(implementation.TextureId);
        }

        /// <summary>
        /// Asserts that a pixel format is one this service supports loading.
        /// </summary>
        /// <param name="pixelFormat">The <see cref="TextureFormat"/> to assert against.</param>
        private void AssertSupportedPixelFormat(TextureFormat pixelFormat)
        {
            if (pixelFormat == TextureFormat.Rgb24 || pixelFormat == TextureFormat.Rgba32)
            {
                return;
            }

            throw new ArgumentException($"{nameof(pixelFormat)} {pixelFormat.ToString()} is unsupported.");
        }

        /// <summary>
        /// Generates mip maps and configures texture parameters for the currently bound texture.
        /// </summary>
        /// <param name="textureTarget">The <see cref="TextureTarget"/> to use for configuring parameters.</param>
        /// <param name="textureParameters">The parameters to apply.</param>
        private void ConfigureBoundTexture(TextureTarget textureTarget, IReadOnlyDictionary<TextureParameterName, int> textureParameters)
        {
            foreach (var parameterKvp in textureParameters)
            {
                GL.TexParameter(textureTarget, parameterKvp.Key, parameterKvp.Value);
            }
        }

        /// <summary>
        /// Converts an array of bytes using <see cref="TextureFormat"/> Rgb24 to Rgba32.
        /// </summary>
        /// <param name="bytes">The array of bytes to convert.</param>
        /// <returns>The converted array of bytes.</returns>
        private byte[] ConvertBytesToRgba32(byte[] bytes)
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
                byte[] bytes = null;
                using (var memoryStream = new MemoryStream())
                {
                    texture.Stream.CopyTo(memoryStream);
                    bytes = memoryStream.ToArray();
                }

                if (texture.Format == TextureFormat.Rgb24)
                {
                    bytes = ConvertBytesToRgba32(bytes);
                }

                GL.TexImage2D(TextureTarget.TextureCubeMapPositiveX + index, 0, PixelInternalFormat.Rgba, texture.Width, texture.Height, 0, PixelFormat.Rgba, PixelType.UnsignedByte, bytes);
            }

            if (_configuration.GenerateMipmaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.TextureCubeMap);
            }

            ConfigureBoundTexture(TextureTarget.TextureCubeMap, _configuration.CubeMapTextureParameters);

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
            var firstTexture = textures.First(t => t != null);
            var expectedWidth = firstTexture.Width;
            var expectedHeight = firstTexture.Height;
            foreach (var texture in textures)
            {
                if (texture == null)
                {
                    continue;
                }

                AssertSupportedPixelFormat(texture.Format);
                if (expectedWidth != texture.Width || expectedHeight != texture.Height)
                {
                    throw new ArgumentException($"All textures used for a packed texture must be the same resolution.");
                }
            }

            var bytes = new byte[expectedWidth * expectedHeight * 4];

            var defaultColorValues = new byte[]
            {
                _configuration.DefaultPackedTextureColor.R,
                _configuration.DefaultPackedTextureColor.G,
                _configuration.DefaultPackedTextureColor.B,
                _configuration.DefaultPackedTextureColor.A,
            };

            for (var index = 0; index < 4; index++)
            {
                var sourceTexture = textures.Length > index ? textures[index] : null;
                if (sourceTexture != null)
                {
                    byte[] sourceBytes = null;
                    using (var memoryStream = new MemoryStream())
                    {
                        sourceTexture.Stream.CopyTo(memoryStream);
                        sourceBytes = memoryStream.ToArray();
                    }

                    var bytesPerPixel = sourceTexture.Format == TextureFormat.Rgba32 ? 4 : 3;
                    var sourceIndex = 0;
                    for (var destIndex = index; destIndex < bytes.Length; destIndex += 4)
                    {
                        bytes[destIndex] = sourceBytes[sourceIndex];
                        sourceIndex += bytesPerPixel;
                    }
                }
                else
                {
                    var defaultValue = defaultColorValues[index];
                    for (var destIndex = index; destIndex < bytes.Length; destIndex += 4)
                    {
                        bytes[destIndex] = defaultValue;
                    }
                }
            }

            return LoadSingleTexture(new Texture(expectedWidth, expectedHeight, TextureFormat.Rgba32, new ReadOnlyCollection<byte>(bytes)));
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

            byte[] bytes = null;
            using (var memoryStream = new MemoryStream())
            {
                texture.Stream.CopyTo(memoryStream);
                bytes = memoryStream.ToArray();
            }

            if (texture.Format == TextureFormat.Rgb24)
            {
                bytes = ConvertBytesToRgba32(bytes);
            }

            // Don't generate extra storage for textures that are too small to properly utilize it.
            GL.TexStorage2D(TextureTarget2d.Texture2D, texture.Width >= 16 && texture.Height >= 16 ? 4 : 1, SizedInternalFormat.Rgba8, texture.Width, texture.Height);
            GL.TexSubImage2D(TextureTarget.Texture2D, 0, 0, 0, texture.Width, texture.Height, PixelFormat.Rgba, PixelType.UnsignedByte, bytes);

            if (_configuration.GenerateMipmaps)
            {
                GL.GenerateMipmap(GenerateMipmapTarget.Texture2D);
            }

            ConfigureBoundTexture(TextureTarget.Texture2D, _configuration.TextureParameters);

            return new LoadedTexture(textureId);
        }
    }
}
