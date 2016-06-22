namespace Crawler.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Model;

    /// <summary>
    /// The floyd warshall.
    /// </summary>
    public class FloydWarshall
    {
        /// <summary>
        /// The dist.
        /// </summary>
        private Matrix<Uri, Uri, ulong> dist;

        /// <summary>
        /// The next.
        /// </summary>
        private Matrix<Uri, Uri, Uri> next;

        /// <summary>
        /// The do work.
        /// </summary>
        /// <param name="graph">
        /// The graph.
        /// </param>
        public void DoWork(Graph graph)
        {
            var keySpace = graph.Neighborhood.Keys.ToArray();
            this.dist = new Matrix<Uri, Uri, ulong>(keySpace, keySpace);
            this.next = new Matrix<Uri, Uri, Uri>(keySpace, keySpace);

            foreach (var u in keySpace)
            {
                foreach (var v in keySpace)
                {
                    if (graph.Neighborhood[u].Neighbours.Any(neighbour => neighbour.Uri.Equals(v)))
                    {
                        this.dist[u, v] = 1;
                        this.next[u, v] = v;
                    }
                    else
                    {
                        this.dist[u, v] = ulong.MaxValue;
                        this.next[u, v] = null;
                    }
                }
            }

            foreach (var k in keySpace)
            {
                foreach (var i in keySpace)
                {
                    foreach (var j in keySpace)
                    {
                        if (this.dist[i, k] + this.dist[k, j] < this.dist[i, j])
                        {
                            this.dist[i, j] = this.dist[i, k] + this.dist[k, j];
                            this.next[i, j] = this.next[i, k];
                        }
                    }
                }
            }

            this.BuildPaths(keySpace, graph);
            graph.Distance = this.dist;
        }

        /// <summary>
        /// The build paths.
        /// </summary>
        /// <param name="keySpace">
        /// The key space.
        /// </param>
        /// <param name="graph">
        /// The graph.
        /// </param>
        public void BuildPaths(Uri[] keySpace, Graph graph)
        {
            foreach (var url1 in keySpace)
            {
                foreach (var url2 in keySpace)
                {
                    graph.Neighborhood[url1].ShortestPaths.Add(url2, new List<Node>());
                    this.GetPath(url1, url2, graph);
                }
            }
        }

        /// <summary>
        /// The get path.
        /// </summary>
        /// <param name="u">
        /// The u.
        /// </param>
        /// <param name="v">
        /// The v.
        /// </param>
        /// <param name="graph">
        /// The graph.
        /// </param>
        public void GetPath(Uri u, Uri v, Graph graph)
        {
            var startNode = graph.Neighborhood[u];

            if (this.next[u, v] == null)
            {
                return;
            }

            while (u != v)
            {
                u = this.next[u, v];
                startNode.ShortestPaths[v].Add(graph.Neighborhood[u]);
            }
        }
    }
}