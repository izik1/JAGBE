using System;

namespace JAGBE.UI
{
    /// <summary>
    /// A class for keeping track of 2d OpenGL textures
    /// </summary>
    internal struct Texture2D : IEquatable<Texture2D>
    {
        /// <summary>
        /// Gets the identifier.
        /// </summary>
        /// <value>The identifier.</value>
        public int Id { get; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>The width.</value>
        public int Width { get; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>The height.</value>
        public int Height { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Texture2D"/> struct.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="width">The width.</param>
        /// <param name="height">The height.</param>
        public Texture2D(int id, int width, int height)
        {
            this.Id = id;
            this.Width = width;
            this.Height = height;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object"/>, is equal to this instance.
        /// </summary>
        /// <param name="obj">The <see cref="object"/> to compare with this instance.</param>
        /// <returns>
        /// <c>true</c> if the specified <see cref="object"/> is equal to this instance; otherwise, <c>false</c>.
        /// </returns>
        public override bool Equals(object obj) => obj is Texture2D && Equals((Texture2D)obj);

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>
        /// A hash code for this instance, suitable for use in hashing algorithms and data structures
        /// like a hash table.
        /// </returns>
        public override int GetHashCode() => base.GetHashCode(); //FIXME: implement better

        /// <summary>
        /// Indicates whether the current object is equal to another object of the same type.
        /// </summary>
        /// <param name="other">An object to compare with this object.</param>
        /// <returns>
        /// <see langword="true"/> if the current object is equal to the <paramref name="other"/>
        /// parameter; otherwise, <see langword="false"/>.
        /// </returns>
        public bool Equals(Texture2D other) => other.Id == this.Id && other.Width == this.Width && other.Height == this.Height;
    }
}
