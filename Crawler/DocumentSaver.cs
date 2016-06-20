namespace Crawler
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Threading.Tasks;

    /// <summary>
    /// The document saver.
    /// </summary>
    public class DocumentSaver
    {
        /// <summary>
        /// The domain.
        /// </summary>
        private readonly Uri domain;

        /// <summary>
        /// The domain folder.
        /// </summary>
        private readonly string domainFolder;

        /// <summary>
        /// The windows illegal chars.
        /// </summary>
        private readonly char[] windowsIllegalChars = @"\/:".ToCharArray();

        /// <summary>
        /// Initializes a new instance of the <see cref="DocumentSaver"/> class.
        /// </summary>
        /// <param name="domain">
        /// The domain.
        /// </param>
        public DocumentSaver(Uri domain)
        {
            this.domain = domain;
            var illegalChars = Path.GetInvalidPathChars();
            this.domainFolder = new string(domain.ToString().Where(c => !illegalChars.Contains(c) && !this.windowsIllegalChars.Contains(c)).ToArray());
        }

        /// <summary>
        /// The save document.
        /// </summary>
        /// <param name="uri">
        /// The uri.
        /// </param>
        /// <param name="content">
        /// The content.
        /// </param>
        /// <returns>
        /// The <see cref="Task"/>.
        /// </returns>
        public async Task SaveDocument(Uri uri, string content)
        {
            var filename = this.domain.MakeRelativeUri(uri).ToString();

            var index = filename.LastIndexOfAny(@"\/".ToCharArray());

            var lastPart = (index >= 0) ? filename.Substring(index) : filename;

            if (!lastPart.Contains('.'))
            {
                if (filename.Length > 0 && filename.Last() != '/')
                {
                    filename += '/';
                }

                filename += "index.html";
            }

            filename = Path.Combine(this.domainFolder, filename);
            var file = new FileInfo(filename);
            file.Directory.Create();
            File.WriteAllText(file.FullName, content);
        }
    }
}
