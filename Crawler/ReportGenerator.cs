namespace Crawler
{
    using Crawler.Algorithms;

    using Model;

    /// <summary>
    /// The report generator.
    /// </summary>
    public class ReportGenerator
    {
        /// <summary>
        /// The create report.
        /// </summary>
        /// <param name="graph">
        /// The graph.
        /// </param>
        public void CreateReport(Graph graph)
        {
            foreach (var node in graph.Neighborhood)
            {
                foreach (var subnode in node.Value.Neighbours)
                {
                    // InDegree
                    if (subnode.Status != NodeStatus.Invalid)
                    {
                        ++graph.Neighborhood[subnode.Uri].InDegree;
                    }

                    // OutDegree
                    ++graph.Neighborhood[node.Key].OutDegree;
                }
            }

            var floyd = new FloydWarshall();
            floyd.DoWork(graph);
            var PageRank = new PageRank(graph);
            PageRank.DoWork();
        }
    }
}
