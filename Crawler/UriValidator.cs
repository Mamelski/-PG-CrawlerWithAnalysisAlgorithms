namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text.RegularExpressions;

    /// <summary>
    /// The uri validator.
    /// </summary>
    public class UriValidator
    {
        /// <summary>
        /// The domain.
        /// </summary>
        private readonly Uri domain;

        private List<Uri> Forbidden = new List<Uri>();

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

        /// <summary>
        /// The validate uri.
        /// </summary>
        /// <param name="uriToVisit">
        /// The uri to visit.
        /// </param>
        /// <param name="currentDomain">
        /// The current domain.
        /// </param>
        /// <param name="match">
        /// The match.
        /// </param>
        /// <param name="graph">
        /// The graph.
        /// </param>
        /// <returns>
        /// The <see cref="bool"/>.
        /// </returns>
        public bool ValidateUri(out Uri uriToVisit, Uri currentDomain, string match, ConcurrentDictionary<Uri, bool> isDocumentAnalyzed)
        {
            var matchValue = match.Replace("<a href=", string.Empty).Replace("\"", string.Empty);

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
            if (uriToVisit.IsAbsoluteUri && !this.domain.IsBaseOf(uriToVisit))
            {
                uriToVisit = null;
                return false;
            }

            // Czy jest plikiem
            if (uriToVisit.IsFile)
            {
                //Counter++;
               Debug.WriteLine($"{Counter++} {uriToVisit}");
                uriToVisit = null;
                return false;
            }

            if (isDocumentAnalyzed.ContainsKey(uriToVisit) && isDocumentAnalyzed[uriToVisit])
            {
                uriToVisit = null;
                return false;
            }

            // Sprawdzenie czy dane Uri już istnieje
            //foreach (var node in graph)
            //{
            //    if (node.Uri == uriToVisit)
            //    {
            //        uriToVisit = null;
            //        return false;
            //    }
            //}

            //Counter++;
            Debug.WriteLine($"{Counter++} {uriToVisit}");
            return true;
        }

        private readonly Regex userAgentRegex = new Regex(@"User-agent\s*:\s*\*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
        private readonly Regex disallowRegex = new Regex(@"Disallow\s*:\s*(.+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);

        public void ParserRobots()
        {
            var robotsTxt = new WebClient().DownloadString(this.domain + "/robots.txt");
            var robotsLines = robotsTxt.Split('\n');

            for (var i = 0; i < robotsLines.Length; i++)
            {
                if (this.userAgentRegex.IsMatch(robotsLines[i]))
                {
                    for (i = i + 1; i < robotsLines.Length; i++)
                    {
                        var match = disallowRegex.Match(robotsLines[i]);

                        if (match.Success)
                        {
                            this.Forbidden.Add(
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