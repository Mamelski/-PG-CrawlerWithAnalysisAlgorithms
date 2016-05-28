namespace Crawler
{
    using System;
    using System.Windows;

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow
    {
        /// <summary>
        /// The web crawler.
        /// </summary>
        private readonly WebCrawler webCrawler;

        /// <summary>
        /// The logger.
        /// </summary>
        private readonly Logger logger;

        /// <summary>
        /// Initializes a new instance of the <see cref="MainWindow"/> class.
        /// </summary>
        public MainWindow()
        {
            this.InitializeComponent();
            this.logger = new Logger(this.loggingTextBlock);
            this.webCrawler = new WebCrawler(this.logger);
        }

        /// <summary>
        /// The start button_ click.
        /// </summary>
        /// <param name="sender">
        /// The sender.
        /// </param>
        /// <param name="e">
        /// The e.
        /// </param>
        private void StartButton_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(this.startUriTextBox.Text))
            {
                this.logger.AddErrorMessage("Adres początkowy nie może być pusty");
                return;
            }

            Uri uri;
            try
            {
                uri = new Uri(this.startUriTextBox.Text);
            }
            catch (UriFormatException)
            {
                this.logger.AddErrorMessage("Uri jest w niepoprawnym formacie");
                return;
            }

            this.webCrawler.Start(uri);
        }
    }
}
