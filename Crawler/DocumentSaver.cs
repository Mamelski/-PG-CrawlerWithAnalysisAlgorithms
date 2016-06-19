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

        private Uri domain;

        public DocumentSaver(Uri domain)
        {
            this.domain = domain;
            if (!Directory.Exists("Test"))
            {
                Directory.CreateDirectory("Test");
            }
        }

        public async Task SaveDocument(Uri uri, string content)
        {
            var filename = domain.MakeRelativeUri(uri).ToString();

            var index = filename.LastIndexOfAny(@"\/".ToCharArray());

            var lastPart = (index >= 0) ? filename.Substring(index) : filename;

            if (!lastPart.Contains('.'))
            {
                if (filename.Length > 0 && filename.Last() != '/')
                    filename += '/';
                filename += "index.html";
            }

            filename = Path.Combine("Test", filename);
            System.IO.FileInfo file = new System.IO.FileInfo(filename);
            file.Directory.Create(); // If the directory already exists, this method does nothing.
            System.IO.File.WriteAllText(file.FullName, content);
        }
    }
}
