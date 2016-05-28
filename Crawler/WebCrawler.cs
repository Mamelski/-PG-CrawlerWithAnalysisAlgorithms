namespace Crawler
{
    using System;
    using System.Collections;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    using System.Runtime.InteropServices.WindowsRuntime;
    using System.Text;
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
        
        private readonly ConcurrentStack<Uri> urisToVisit = new ConcurrentStack<Uri>();

        private Uri domain;

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
        /// <param name="domain">
        /// The start url.
        /// </param>
        public async void Start(Uri domain)
        {
            int i = 0;
            this.domain = domain;
            var webClient = new WebClient();

            var content = webClient.DownloadString(domain);
            this.GetNewLinks(content);

            while (!this.urisToVisit.IsEmpty)
            {
                Uri nextUri = null;
                while (nextUri == null)
                {
                    this.urisToVisit.TryPop(out nextUri);
                }
                Debug.WriteLine($"{i++}");

                try
                {
                    content = webClient.DownloadString(nextUri);
                    //content = await GetDocument(nextUri);
                    //if (string.IsNullOrEmpty(content))
                    //{
                    //    continue;
                    //}
                }
                catch (WebException)
                {
                    this.log.AddErrorMessage($"Błąd przy pobieraniu dokumentu {nextUri}.");
                    continue;
                }

                this.GetNewLinks(content);
                
            }
            int a=0;
        }

        public void GetNewLinks(string content)
        {
            //TODO sprawdzaj domenę
            var regexLink = new Regex(@"<a\s+(?:[^>]*?\s+)?href=""([^""]*)""", RegexOptions.Compiled);

            var matches = regexLink.Matches(content).Cast<Match>();

            Parallel.ForEach(
               matches,
                match =>
                    {

                        Uri uriToVisit;
                        if (this.ValidateUri(out uriToVisit, this.domain, (Match)match))
                        {
                            try
                            {
                                this.urisToVisit.Push(uriToVisit);
                                this.graph.Add(new Node(uriToVisit));

                            }
                            catch (Exception ex)
                            {
                                this.log.AddErrorMessage(
                                    $"Nie moża dodać adresu {match} do kolejki. Błąd: {ex.Message}");
                            }
                        }
                    });
        }

        private bool ValidateUri(out Uri uriToVisit, Uri currentDomain, Match match)
        {
            var matchValue = match.Value.
                        Replace("<a href=", string.Empty).
                        Replace("\"", string.Empty);

            // #
            if (matchValue.Contains('#'))
            {
                matchValue = matchValue.Remove(matchValue.IndexOf('#'));
            }

            // ?
            if (matchValue.Contains('?'))
            {
                matchValue = matchValue.Remove(matchValue.IndexOf('?'));
            }

            // Puste Uri
            if (string.IsNullOrEmpty(matchValue))
            {
                uriToVisit = null;
                return false;
            }

            // Przekształcenie na pełną formę
            if (Uri.IsWellFormedUriString(matchValue, UriKind.Relative))
            {
                uriToVisit = new Uri(currentDomain, matchValue);
            }
            else if (Uri.IsWellFormedUriString(matchValue, UriKind.Absolute))
            {
                uriToVisit = new Uri(matchValue);
            }
            else
            {
                uriToVisit = null;
                return false;
            }

            // Sprawdzenie czy Uri jest w domenie, którą przeglądamy
            if (!this.domain.IsBaseOf(uriToVisit))
            {
                uriToVisit = null;
                return false;
            }

            // Czy jest plikiem
            if (uriToVisit.IsFile)
            {
                uriToVisit = null;
                return false;
            }
       

            // Sprawdzenie czy dane Uri już istnieje
            foreach (var n in this.graph)
            {
                if (n.Uri == uriToVisit)
                {
                    uriToVisit = null;
                    return false;
                }
            }

            return true;
        }

        private async Task<string> GetDocument(Uri urlAddress)
        {
            string str = null;
            try
            {
                using (HttpResponseMessage response = await new HttpClient().GetAsync(urlAddress))
                {
                    if (response.IsSuccessStatusCode)
                        using (HttpContent content = response.Content)
                        {
                            if (content.Headers.ContentType.MediaType.ToLower().Contains("html") || content.Headers.ContentType.MediaType.ToLower().Contains("text"))
                                str = await content.ReadAsStringAsync();
                        }
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"For url: {urlAddress} exception: {ex} occured!");
            }

            return str;
        }
    }
}
