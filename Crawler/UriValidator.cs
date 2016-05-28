namespace Crawler
{
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
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
        public bool ValidateUri(out Uri uriToVisit, Uri currentDomain, Match match, ConcurrentDictionary<Uri, bool> isDocumentAnalyzed)
        {
            var matchValue = match.Value.Replace("<a href=", string.Empty).Replace("\"", string.Empty);

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
            if (!uriToVisit.ToString().Contains(this.domain.ToString()))
            {
                uriToVisit = null;
                return false;
            }

            // Czy jest plikiem
            if (uriToVisit.IsFile)
            {
                Counter++;
               // Debug.WriteLine($"{counter++} {uriToVisit}");
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

            Counter++;
            //Debug.WriteLine($"{counter++} {uriToVisit}");
            return true;
        }
    }
}