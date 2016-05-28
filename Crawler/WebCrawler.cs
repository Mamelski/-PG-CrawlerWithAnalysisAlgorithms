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
        private readonly ConcurrentBag<Node> graph = new ConcurrentBag<Node>();

        private readonly ConcurrentDictionary<Uri, bool> isDocumentAnalyzed = new ConcurrentDictionary<Uri, bool>();

        private Uri domain;

        private UriValidator uriValidator;

      

        private readonly Regex linkRegex = new Regex(@"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Compiled);

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
            this.uriValidator = new UriValidator(startDomain);
            this.domain = startDomain;
            await this.ParseDocument(this.domain);
        }

        public async Task ParseDocument(Uri uri)
        {
            // ++Counter;

            string content;
            try
            {
                content = new WebClient().DownloadString(uri);
            }
            catch (WebException ex)
            {
                return;
            }

            var matches = this.linkRegex.Matches(content).Cast<Match>();
            var tasks = new ConcurrentBag<Task>();

            Parallel.ForEach(matches, match =>
                    {
                        Uri uriToVisit;
                        if (this.uriValidator.ValidateUri(out uriToVisit, this.domain, match, this.isDocumentAnalyzed))
                        {
                            try
                            {
                                this.graph.Add(new Node(uriToVisit));
                                this.isDocumentAnalyzed[uriToVisit] = true;
                                tasks.Add(this.ParseDocument(uriToVisit));
                            }
                            catch (Exception ex)
                            {
                                this.log.AddErrorMessage($"Nie moża dodać adresu {match} do kolejki. Błąd: {ex.Message}");
                            }
                        }
                    });
            await Task.WhenAll(tasks);
            Debug.WriteLine(this.uriValidator.Counter);
        }
    }
}
