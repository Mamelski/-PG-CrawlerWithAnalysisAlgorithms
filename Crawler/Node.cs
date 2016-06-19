namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    /// <summary>
    /// The node.
    /// </summary>
    public class Node
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Node"/> class.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        public Node(Uri uri)
        {
            this.Uri = uri;
        }

        /// <summary>
        /// Gets the uri.
        /// </summary>
        public Uri Uri { get; }

        public  ConcurrentBag<Node> Neighbours { get; } = new ConcurrentBag<Node>();
    }
}
