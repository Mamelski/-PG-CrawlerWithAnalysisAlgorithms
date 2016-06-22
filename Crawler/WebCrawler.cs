namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

    using Crawler.DiscOperations;
    using Crawler.Model;

    /// <summary>
    /// The web crawler.
    /// </summary>
    public class WebCrawler
    {
        /// <summary>
        /// The log.
        /// </summary>
        private readonly Logger log;

        /// <summary>
        /// The graph.
        /// </summary>
        private readonly Graph graph = new Graph();

        private readonly ConcurrentBag<Uri> analyzedDocuments = new ConcurrentBag<Uri>();

        private Uri domain;

        private UriValidator uriValidator;

        private Disc disc;

        private readonly Regex linkRegex = new Regex(@"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="WebCrawler"/> class.
        /// </summary>
        /// <param name="log">
        /// The log.
        /// </param>
        public WebCrawler(Logger log)
        {
            this.log = log;
        }

        /// <summary>
        /// The start.
        /// </summary>
        /// <param name="startDomain">
        /// The start url.
        /// </param>
        public async Task<Graph> StartAnalyzingDomain(Uri startDomain)
        {
            this.domain = startDomain;
            this.uriValidator = new UriValidator(startDomain);
            this.disc = new Disc(startDomain);

            this.graph.Neighborhood.TryAdd(startDomain, new Node(startDomain, NodeStatus.Valid));
            this.analyzedDocuments.Add(startDomain);

            this.uriValidator.ParserRobots();

            var sw = new Stopwatch();
            sw.Start();
            await this.ParseDocument(this.domain);
            sw.Stop();

            Debug.WriteLine(sw.Elapsed);

            return this.graph;
        }

        public async Task ParseDocument(Uri uri)
        {
            string content;
            try
            {
                content = await new WebClient().DownloadStringTaskAsync(uri);
            }
            catch (WebException)
            {
                return;
            }

            var matches = this.linkRegex.Matches(content).Cast<Match>();
            var tasks = new ConcurrentBag<Task> { this.disc.SaveDocument(uri, content) };

            Parallel.ForEach(matches, match =>
                {
                    var nodeToVisit = this.uriValidator.ValidateUri(this.domain, match.Groups[1].Value);
                    try
                    {
                        if (nodeToVisit.Status == NodeStatus.Invalid)
                        {
                            return;
                        }
                        if (!this.analyzedDocuments.Contains(nodeToVisit.Uri))
                            {
                                this.analyzedDocuments.Add(nodeToVisit.Uri);
                                if (nodeToVisit.Status != NodeStatus.NotInDomain)
                                {
                                    tasks.Add(this.ParseDocument(nodeToVisit.Uri));
                                    Debug.WriteLine($"{Counter++} {nodeToVisit.Uri}");
                                }
                               
                            }

                            if (!this.graph.Neighborhood.ContainsKey(nodeToVisit.Uri))
                            {
                                while (!this.graph.Neighborhood.TryAdd(nodeToVisit.Uri, nodeToVisit));
                            }
                        

                        // TODO za dużo sąsiadów
                        if (!this.graph.Neighborhood[uri].Neighbours.Contains(nodeToVisit))
                        {
                            this.graph.Neighborhood[uri].Neighbours.Add(nodeToVisit);
                        }
                    }
                    catch (Exception ex)
                    {
                        this.log.AddErrorMessage($"Nie moża dodać adresu {match} do kolejki. Błąd: {ex.Message}");
                    }

                });
            await Task.WhenAll(tasks);
        }


        private int counter = 0;
        private object counterLockObject = new object();

        public int Counter
        {
            get
            {
                lock (counterLockObject)
                {
                    if (counter > 3000)
                    {
                        int a = 0;
                    }
                    return counter;
                }
            }
            set
            {
                lock (counterLockObject)
                {
                    counter = value;
                }
            }
        }
    }
}
