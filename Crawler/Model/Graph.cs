namespace Crawler.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    /// <summary>
    /// The graph.
    /// </summary>
    public class Graph
    {
        /// <summary>
        /// The Neighborhood.
        /// </summary>
        public ConcurrentDictionary<Uri, Node> Neighborhood { get; } = new ConcurrentDictionary<Uri, Node>();

        public int NumberOfNodes => this.Neighborhood.Count;

        public int NumberOfEdges
        {
            get
            {
                return this.Neighborhood.Sum(node => node.Value.Neighbours.Count);
            }
        }
    }
}
