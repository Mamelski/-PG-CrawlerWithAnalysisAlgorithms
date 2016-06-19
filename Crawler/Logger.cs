namespace Crawler
{
    using System;
    using System.Windows.Controls;
    using System.Windows.Documents;
    using System.Windows.Media;

    /// <summary>
    /// The logger.
    /// </summary>
    public class Logger
    {
        /// <summary>
        /// The logging text block.
        /// </summary>
        private readonly TextBlock loggingTextBlock;

        /// <summary>
        /// Initializes a new instance of the <see cref="Logger"/> class.
        /// </summary>
        /// <param name="loggingTextBlock">
        /// The logging text block.
        /// </param>
        public Logger(TextBlock loggingTextBlock)
        {
            this.loggingTextBlock = loggingTextBlock;
        }

        /// <summary>
        /// The add error message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void AddErrorMessage(string message)
        {
            //this.loggingTextBlock.Inlines.Add(new Run("[Error]" + message + Environment.NewLine)
            //{
            //    Foreground = Brushes.Red
            //});
        }

        /// <summary>
        /// The add success message.
        /// </summary>
        /// <param name="message">
        /// The message.
        /// </param>
        public void AddSuccessMessage(string message)
        {
            this.loggingTextBlock.Inlines.Add(new Run("[Success]" + message + Environment.NewLine)
            {
                Foreground = Brushes.Green
            });
        }
    }
}
