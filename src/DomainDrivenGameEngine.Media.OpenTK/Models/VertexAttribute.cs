using DomainDrivenGameEngine.Media.Models;

namespace DomainDrivenGameEngine.Media.OpenTK.Models
{
    /// <summary>
    /// The attributes that exist on a <see cref="Vertex"/> that can be applied to a vertex buffer.
    /// </summary>
    public enum VertexAttribute
    {
        /// <summary>
        /// The position of a vertex, a three element float.
        /// </summary>
        Position,

        /// <summary>
        /// The normal of a vertex, a three element float.
        /// </summary>
        Normal,

        /// <summary>
        /// The tangent of a vertex, a three element float.
        /// </summary>
        Tangent,

        /// <summary>
        /// The color of a vertex, a four element float.
        /// </summary>
        Color,

        /// <summary>
        /// The texture coordinate of a vertex, a two element float.
        /// </summary>
        TextureCoordinate,
    }
}
