namespace Crawler.Algorithms
{
    using System;
    using System.Collections.Generic;
    using System.Linq;

    using Crawler.Model;

    public class PageRank
    {
        private readonly Graph graph;

        public PageRank(Graph graph)
        {
            this.graph = graph;
        }

        public double Damping { get; set; } = 0.85;

        public int MaxSteps { get; set; } = 10;

        public double Convergence { get; set; } = 1e-4;

        private int N => this.graph.NumberOfNodes;

        private readonly Dictionary<Uri, double> previousPageRankValues = new Dictionary<Uri, double>();

        public void DoWork()
        {
            foreach (var uri in this.graph.Neighborhood)
            {
                uri.Value.PageRank = 1.0 / this.N;
                this.previousPageRankValues[uri.Key] = double.MaxValue / 100;
            }

            while (this.previousPageRankValues.All(kvp => Math.Abs(kvp.Value - this.graph.Neighborhood[kvp.Key].PageRank) > this.Convergence))
            {
                foreach (var url1 in this.graph.Neighborhood)
                {
                    this.previousPageRankValues[url1.Key] = url1.Value.PageRank;

                   url1.Value.PageRank = (1 - this.Damping) / this.N;
                    var sum = 0.0;

                    foreach (var url2 in this.graph.Neighborhood)
                    {
                        if (url2.Value.Neighbours.Contains(url1.Value))
                        {
                            sum += url2.Value.PageRank / url2.Value.OutDegree;
                        }
                    }
                    url1.Value.PageRank += this.Damping * sum;
                }
            }
        }
    }
}