namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;
    using System.Threading.Tasks;

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
        private readonly ConcurrentDictionary<Uri, Node> graph = new ConcurrentDictionary<Uri, Node>();

        private readonly ConcurrentBag<Uri> analyzedDocuments = new ConcurrentBag<Uri>();

        private Uri domain;

        private UriValidator uriValidator;

        private DocumentSaver documentSaver;

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
        public async void StartAnalyzingDomain(Uri startDomain)
        {
            this.domain = startDomain;
            this.uriValidator = new UriValidator(startDomain);
            this.documentSaver = new DocumentSaver(startDomain);

            this.graph.TryAdd(startDomain, new Node(startDomain, NodeStatus.Valid));
            this.analyzedDocuments.Add(startDomain);

            // this.uriValidator.ParserRobots();

            var sw = new Stopwatch();
            sw.Start();
            await this.ParseDocument(this.domain);

            sw.Stop();
            Debug.WriteLine(sw.Elapsed);
            var ser = new GraphSerializer();
            ser.Serialize(this.graph);
            int a = 0;
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
            var tasks = new ConcurrentBag<Task> { this.documentSaver.SaveDocument(uri, content) };

            Parallel.ForEach(matches, match =>
                {
                    var nodeToVisit = this.uriValidator.ValidateUri(this.domain, match.Groups[1].Value);
                    try
                    {
                        if (nodeToVisit.Status != NodeStatus.Invalid )
                        {
                            if (!this.analyzedDocuments.Contains(nodeToVisit.Uri))
                            {
                                this.analyzedDocuments.Add(nodeToVisit.Uri);
                                if (nodeToVisit.Status != NodeStatus.NotInDomain)
                                {
                                    tasks.Add(this.ParseDocument(nodeToVisit.Uri));
                                    Debug.WriteLine($"{Counter++} {nodeToVisit.Uri}");
                                }
                               
                            }

                            if (!this.graph.ContainsKey(nodeToVisit.Uri))
                            {
                                while (!this.graph.TryAdd(nodeToVisit.Uri, nodeToVisit));
                            }
                        }

                        this.graph[uri].Neighbours.Add(nodeToVisit);

                        
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
