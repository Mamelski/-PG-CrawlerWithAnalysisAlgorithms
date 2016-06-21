namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    using Crawler.Model;

    /// <summary>
    /// The uri validator.
    /// </summary>
    public class UriValidator
    {
        /// <summary>
        /// The domain.
        /// </summary>
        private readonly Uri domain;

        /// <summary>
        /// The forbidden.
        /// </summary>
        private readonly ConcurrentBag<Uri> forbiddenUris = new ConcurrentBag<Uri>();

        /// <summary>
        /// The user agent regex.
        /// </summary>
        private readonly Regex userAgentRegex = new Regex(@"User-agent\s*:\s*\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// The disallow regex.
        /// </summary>
        private readonly Regex disallowRegex = new Regex(@"Disallow\s*:\s*(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        /// <summary>
        /// Initializes a new instance of the <see cref="UriValidator"/> class.
        /// </summary>
        /// <param name="domain">
        /// The domain.
        /// </param>
        public UriValidator(Uri domain)
        {
            this.domain = domain;
        }

        /// <summary>
        /// The validate uri.
        /// </summary>
        /// <param name="currentDomain">
        /// The current domain.
        /// </param>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <returns>
        /// The <see cref="NodeStatus"/>.
        /// </returns>
        public Node ValidateUri(Uri currentDomain, string match)
        {
            if (match.Contains('#'))
            {
                match = match.Remove(match.IndexOf('#'));
            }

            if (match.Contains('?'))
            {
                match = match.Remove(match.IndexOf('?'));
            }

            Uri url;

            if (Uri.IsWellFormedUriString(match, UriKind.Relative))
            {
                url = new Uri(currentDomain, match);
            }
            else if (Uri.IsWellFormedUriString(match, UriKind.Absolute))
            {
                url = new Uri(match);
            }
            else
            {
                return new Node(null, NodeStatus.Invalid);
            }

            if (url.IsFile)
            {
                return new Node(url, NodeStatus.File);
            }

            if (url.IsAbsoluteUri && !currentDomain.IsBaseOf(url))
            {
                return new Node(url, NodeStatus.NotInDomain);
            }

            if (!url.IsAbsoluteUri)
            {
                url = new Uri(currentDomain, url);
            }

            if (this.forbiddenUris.Contains(url))
            {
                return new Node(url, NodeStatus.Forbidden);
            }

           return new Node(url, NodeStatus.Valid);
        }

        /// <summary>
        /// The parser robots.
        /// </summary>
        public void ParserRobots()
        {
            string robotsTxt;
            try
            {
                robotsTxt = new WebClient().DownloadString(this.domain + "/robots.txt");
            }
            catch (WebException)
            {
                // Nie ma robotów
                return;
            }

            var robotsLines = robotsTxt.Split('\n');

            for (var i = 0; i < robotsLines.Length; i++)
            {
                if (this.userAgentRegex.IsMatch(robotsLines[i]))
                {
                    for (i = i + 1; i < robotsLines.Length; i++)
                    {
                        var match = this.disallowRegex.Match(robotsLines[i]);

                        if (match.Success)
                        {
                            this.forbiddenUris.Add(
                                match.Value == "/"
                                    ? this.domain
                                    : new Uri(this.domain, new Uri(match.Value)));
                        }
                    }
                }
            }
        }
    }
}