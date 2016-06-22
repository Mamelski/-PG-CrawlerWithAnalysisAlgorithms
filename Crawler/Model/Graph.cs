namespace Crawler.Model
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;

    using Crawler.Algorithms;

    /// <summary>
    /// The graph.
    /// </summary>
    public class Graph
    {

        /// <summary>
        /// The dist.
        /// </summary>
        public Matrix<Uri, Uri, ulong> Distance;

        /// <summary>
        /// The next.
        /// </summary>
        public Matrix<Uri, Uri, Uri> next;

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

        public double GetAverageDistance()
        {
            var count = 0;
            var value = 0.0;

            foreach (var u in this.Neighborhood.Keys)
            {
                foreach (var v in this.Neighborhood.Keys)
                {
                    if (this.Distance[u, v] != ulong.MaxValue)
                    {
                        ++count;
                        value += this.Distance[u, v];
                    }
                }
            }

            return value / count;
        }


        public int GetDiameter()
        {
            int diameter = int.MinValue;

            foreach (var u in this.Neighborhood)
            {
                foreach (var v in u.Value.ShortestPaths)
                {
                    diameter = Math.Max(diameter, this.Neighborhood[u.Key].ShortestPaths[v.Key].Count);
                }
            }
            return diameter;
        }

        public int GetRadius()
        {
            var radius = int.MaxValue;
            foreach (var u in this.Neighborhood.Where(u => u.Value.Status == NodeStatus.Valid))
            {
                var maxDistance = 0;
                foreach (var v in u.Value.ShortestPaths)
                {
                    maxDistance = Math.Max(maxDistance, this.Neighborhood[u.Key].ShortestPaths[v.Key].Count);
                }

                if (maxDistance != 0)
                {
                    radius = Math.Min(radius, maxDistance);
                }
            }
            return radius;
        }
    }
}
