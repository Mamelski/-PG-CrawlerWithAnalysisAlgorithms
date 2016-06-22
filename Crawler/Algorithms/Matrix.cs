namespace Crawler.Algorithms
{
    using System.Collections.Generic;

    /// <summary>
    /// The matrix.
    /// </summary>
    /// <typeparam name="TKey1">
    /// </typeparam>
    /// <typeparam name="TKey2">
    /// </typeparam>
    /// <typeparam name="TValue">
    /// </typeparam>
    public class Matrix<TKey1, TKey2, TValue>
    {
        /// <summary>
        /// The inner dictionary.
        /// </summary>
        private readonly Dictionary<TKey1, Dictionary<TKey2, TValue>> innerDictionary;

        /// <summary>
        /// Initializes a new instance of the <see cref="Matrix{TKey1,TKey2,TValue}"/> class.
        /// </summary>
        /// <param name="key1space">
        /// The key 1 space.
        /// </param>
        /// <param name="key2space">
        /// The key 2 space.
        /// </param>
        public Matrix(ICollection<TKey1> key1space, ICollection<TKey2> key2space)
        {
            this.innerDictionary = new Dictionary<TKey1, Dictionary<TKey2, TValue>>();
            foreach (var k1 in key1space)
            {
                this.innerDictionary[k1] = new Dictionary<TKey2, TValue>(key2space.Count);
            }
        }

        /// <summary>
        /// The this.
        /// </summary>
        /// <param name="key1">
        /// The key 1.
        /// </param>
        /// <param name="key2">
        /// The key 2.
        /// </param>
        /// <returns>
        /// The <see cref="double"/>.
        /// </returns>
        public TValue this[TKey1 key1, TKey2 key2]
        {
            get
            {
                return this.innerDictionary[key1][key2];
            }

            set
            {
                this.innerDictionary[key1][key2] = value;
            }
        }
    }
}