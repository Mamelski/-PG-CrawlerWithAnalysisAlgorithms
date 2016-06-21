﻿namespace Crawler.Model
{
    using System;
    using System.Collections.Concurrent;

    public class Node : IEquatable<Node>
    {
        public Node(Uri uri, NodeStatus status)
        {
            this.Uri = uri;
            this.Status = status;
        }

        /// <summary>
        /// Gets the uri.
        /// </summary>
        public Uri Uri { get; }

        public NodeStatus Status { get; }

        public ConcurrentBag<Node> Neighbours { get; } = new ConcurrentBag<Node>();

        public int InDegree { get; set; }

        public int OutDegree { get; set; }

        public bool Equals(Node other)
        {
            return this.Uri.Equals(other.Uri);
        }

        public override int GetHashCode()
        {
            return this.Uri.GetHashCode();
        }
    }
}
