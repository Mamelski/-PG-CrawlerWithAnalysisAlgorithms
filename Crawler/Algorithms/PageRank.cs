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

        public double Dampening { get; set; } = 0.85;

        public double Convergence { get; set; } = 0.00001;

        private int N => this.graph.NumberOfNodes;

        private readonly Dictionary<Uri, double> previousPageRankValues = new Dictionary<Uri, double>();

        public int DoWork()
        {
            foreach (var uri in this.graph.Neighborhood)
            {
                uri.Value.PageRank = 1.0 / this.N;
                this.previousPageRankValues[uri.Key] = double.MaxValue / 100;
            }

            int iterations = 0;

            while (this.previousPageRankValues.All(kvp => Math.Abs(kvp.Value - this.graph.Neighborhood[kvp.Key].PageRank) > this.Convergence))
            {
                ++iterations;
                foreach (var url1 in this.graph.Neighborhood)
                {
                    this.previousPageRankValues[url1.Key] = url1.Value.PageRank;

                   url1.Value.PageRank = (1 - this.Dampening) / this.N;
                    var sum = 0.0;

                    foreach (var url2 in this.graph.Neighborhood)
                    {
                        if (url2.Value.Neighbours.Contains(url1.Value))
                        {
                            sum += url2.Value.PageRank / url2.Value.OutDegree;
                        }
                    }
                    url1.Value.PageRank += this.Dampening * sum;
                }
            }

            return iterations;
        }
    }
}