namespace Shark.PdfConvert
{
    using System;
    using System.Text;
    using System.IO;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    /// Pdf Conversion wrapping the WkHtmlToPdf tool
    /// </summary>
    public class PdfConvert
    {
        private static string BuildOptions(PdfConversionSettings config,
            string temporaryContentFilePath,
            string temporaryPdfFilePath,
            string temporaryCoverFilePath,
            string temporaryHeaderFilePath,
            string temporaryFooterFilePath)
        {
            StringBuilder options = new StringBuilder();

            // GLOBAL OPTIONS
            if (string.IsNullOrWhiteSpace(config.CustomWkHtmlArgs))
            {
                if (config.Grayscale) options.Append("--grayscale ");
                if (config.LowQuality) options.Append("--lowquality ");
                if (config.Margins.Bottom != null) options.AppendFormat("--margin-bottom {0} ", config.Margins.Bottom);
                if (config.Margins.Top != null) options.AppendFormat("--margin-top {0} ", config.Margins.Top);
                if (config.Margins.Left != null) options.AppendFormat("--margin-left {0} ", config.Margins.Left);
                if (config.Margins.Right != null) options.AppendFormat("--margin-right {0} ", config.Margins.Right);
                if (config.Size != PdfPageSize.Default) options.AppendFormat("--page-size {0}", config.Size.ToString());
                if (config.Orientation != PdfPageOrientation.Default) options.AppendFormat("--orientation {0} ", config.Orientation.ToString());
                if (string.IsNullOrWhiteSpace(config.Title) == false) options.AppendFormat("--title \"{0}\" ", config.Title.Replace("\"", ""));
            }
            else
            {
                options.Append(config.CustomWkHtmlArgs);
                options.Append(" ");
            }

            // COVER
            if (string.IsNullOrWhiteSpace(temporaryCoverFilePath) == false ||
                string.IsNullOrWhiteSpace(config.PageCoverUrl) == false)
            {
                options.AppendFormat("cover  \"{0}\" ",
                    string.IsNullOrWhiteSpace(config.PageCoverUrl) ? temporaryCoverFilePath : config.PageCoverUrl);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlCoverArgs) == false)
                {
                    options.Append(config.CustomWkHtmlCoverArgs);
                    options.Append(" ");
                }
            }

            // FOOTER
            if (string.IsNullOrWhiteSpace(temporaryFooterFilePath) == false ||
                string.IsNullOrWhiteSpace(config.PageFooterUrl) == false)
            {
                options.AppendFormat("--footer-html  \"{0}\" ",
                    string.IsNullOrWhiteSpace(config.PageFooterUrl) ? temporaryFooterFilePath : config.PageFooterUrl);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlFooterArgs) == false)
                {
                    options.Append(config.CustomWkHtmlFooterArgs);
                    options.Append(" ");
                }
            }

            // HEADER
            if (string.IsNullOrWhiteSpace(temporaryHeaderFilePath) == false ||
                string.IsNullOrWhiteSpace(config.PageHeaderUrl) == false)
            {
                options.AppendFormat("--header-html  \"{0}\" ",
                    string.IsNullOrWhiteSpace(config.PageHeaderUrl) ? temporaryHeaderFilePath : config.PageHeaderUrl);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlHeaderArgs) == false)
                {
                    options.Append(config.CustomWkHtmlHeaderArgs);
                    options.Append(" ");
                }
            }

            // TABLE OF CONTENTS
            if (config.GenerateToc)
            {
                options.Append("toc ");
                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlTocArgs) == false)
                {
                    options.Append(config.CustomWkHtmlTocArgs);
                    options.Append(" ");
                }
            }

            // PAGE
            if (config.ContentUrls.Any())
            {
                var count = config.ContentUrls.Count - 1;
                if (count < 0) count = 0;

                foreach (var url in config.ContentUrls.Take(count))
                {
                    options.AppendFormat("page \"{0}\" ", url);
                }

                options.AppendFormat("page \"{1}\" \"{0}\" ",
                        temporaryPdfFilePath,
                        config.ContentUrls.Last());
            } else
            {
                options.AppendFormat("page \"{1}\" \"{0}\" ",
                    temporaryPdfFilePath,
                    string.IsNullOrWhiteSpace(config.ContentUrl) ? temporaryContentFilePath : config.ContentUrl);
            }

            // PAGE OPTIONS
            if (string.IsNullOrWhiteSpace(config.CustomWkHtmlPageArgs))
            {
                if (config.Zoom != null) options.AppendFormat("--zoom {0} ", config.Zoom);
            }
            else
            {
                options.Append(config.CustomWkHtmlPageArgs);
            }

            return options.ToString();
        }

        /// <summary>
        /// Converts an HTML source input into a PDF output using the WkHTMLToPDF tool.
        /// The kitchen sink overload.
        /// </summary>
        /// <param name="config">Conversion</param>
        /// <param name="pdfOutputStream"></param>
        /// <param name="contentInputStream"></param>
        /// <param name="coverInputStream"></param>
        /// <param name="footerInputStream"></param>
        /// <param name="headerInputStream"></param>
        /// <param name="outputCallback"></param>
        public static void Convert(PdfConversionSettings config,
            Stream pdfOutputStream = null,
            Stream contentInputStream = null,
            Stream coverInputStream = null,
            Stream footerInputStream = null,
            Stream headerInputStream = null,
            Action<PdfConversionSettings, byte[]> outputCallback = null)
        {
            if (!File.Exists(config.PdfToolPath))
                throw new ArgumentException($"File '{config.PdfToolPath}' not found. Check if wkhtmltopdf application is installed and set the correct location before calling this method.");

            if (contentInputStream == null &&
                string.IsNullOrWhiteSpace(config.Content) &&
                string.IsNullOrWhiteSpace(config.ContentUrl) &&
                config.ContentUrls.Any() == false)
                throw new ArgumentException("You must specify an input stream, static Content, Content Url, or Content Urls.");

            if (pdfOutputStream == null &&
                string.IsNullOrWhiteSpace(config.OutputPath))
                throw new ArgumentException("You must specify an output stream or an output path.");

            string temporaryContentFilePath = Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");

            string temporaryPdfFilePath = string.IsNullOrWhiteSpace(config.OutputPath) ?
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.pdf") :
                config.OutputPath;

            // if no cover page html, url, or stream, don't set a temp path for the cover, footer, and/or header
            string temporaryCoverFilePath = coverInputStream == null && 
                    string.IsNullOrWhiteSpace(config.PageCoverHtml) &&
                    string.IsNullOrWhiteSpace(config.PageCoverUrl) ?
                null :
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");
            string temporaryFooterFilePath = footerInputStream == null && 
                    string.IsNullOrWhiteSpace(config.PageFooterHtml) &&
                    string.IsNullOrWhiteSpace(config.PageFooterUrl) ?
                null :
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");
            string temporaryHeaderFilePath = headerInputStream == null && 
                    string.IsNullOrWhiteSpace(config.PageHeaderHtml) &&
                    string.IsNullOrWhiteSpace(config.PageHeaderUrl) ?
                null :
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");

            var commandOptions = BuildOptions(config,
                temporaryContentFilePath,
                temporaryPdfFilePath,
                temporaryCoverFilePath,
                temporaryHeaderFilePath,
                temporaryFooterFilePath);

            try
            {
                // COVER
                if (coverInputStream != null)
                {
                    using (var tempCoverFileStream = new FileStream(temporaryCoverFilePath, FileMode.CreateNew))
                    {
                        if (coverInputStream.CanSeek) coverInputStream.Position = 0;
                        coverInputStream.CopyTo(tempCoverFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.PageCoverUrl) &&
                    string.IsNullOrWhiteSpace(config.PageCoverHtml) == false)
                {
                    // cover, if content is manually specified and no stream specified, then use the content
                    File.WriteAllText(temporaryCoverFilePath, config.PageCoverHtml);
                }

                // FOOTER
                if (footerInputStream != null)
                {
                    using (var tempFooterFileStream = new FileStream(temporaryFooterFilePath, FileMode.CreateNew))
                    {
                        if (footerInputStream.CanSeek) footerInputStream.Position = 0;
                        footerInputStream.CopyTo(tempFooterFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.PageFooterUrl) &&
                    string.IsNullOrWhiteSpace(config.PageFooterHtml) == false)
                {
                    // footer, if content is manually specified and no stream specified, then use the content
                    File.WriteAllText(temporaryFooterFilePath, config.PageFooterHtml);
                }

                // HEADER
                if (headerInputStream != null)
                {
                    using (var tempHeaderFileStream = new FileStream(temporaryHeaderFilePath, FileMode.CreateNew))
                    {
                        if (headerInputStream.CanSeek) headerInputStream.Position = 0;
                        headerInputStream.CopyTo(tempHeaderFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.PageHeaderUrl) &&
                    string.IsNullOrWhiteSpace(config.PageHeaderHtml) == false)
                {
                    // header, if content is manually specified and no stream specified, then use the content
                    File.WriteAllText(temporaryHeaderFilePath, config.PageHeaderHtml);
                }

                // CONTENT
                if (contentInputStream != null)
                {
                    // use the stream specified
                    using (var tempContentFileStream = new FileStream(temporaryContentFilePath, FileMode.CreateNew))
                    {
                        if (contentInputStream.CanSeek) contentInputStream.Position = 0;
                        contentInputStream.CopyTo(tempContentFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.ContentUrl) &&
                  string.IsNullOrWhiteSpace(config.Content) == false)
                {
                    // use the content specified
                    using (var tempContentFileStream = new FileStream(temporaryContentFilePath, FileMode.CreateNew))
                    {
                        byte[] contentAsBytes = Encoding.UTF8.GetBytes(config.Content);
                        tempContentFileStream.Write(contentAsBytes, 0, contentAsBytes.Length);
                    }
                }

                // start process
                using (var process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = config.ProcessOptionCreateNoWindow,
                        FileName = config.PdfToolPath,
                        Arguments = commandOptions,
                        UseShellExecute = config.ProcessOptionUseShellExecute,
                        RedirectStandardError = config.ProcessOptionRedirectStandardError
                    };

                    process.StartInfo = startInfo;
                    process.Start();

                    if (!process.WaitForExit(config.ExecutionTimeout))
                        throw new TimeoutException($"wkhtmltopdf execution time elapsed {config.ExecutionTimeout} ms.");

                    // check to make sure the generated file exists and the process didn't error
                    if (!File.Exists(temporaryPdfFilePath))
                    {
                        if (process.ExitCode != 0)
                        {
                            var error = startInfo.RedirectStandardError ?
                                process.StandardError.ReadToEnd() :
                                $"WkHTMLToPdf exited with code {process.ExitCode}.";
                            throw new InvalidDataException($"WkHTMLToPdf conversion of HTML data failed. Output: \r\n{error}");
                        }

                        throw new InvalidDataException($"WkHTMLToPdf conversion of HTML data failed. Output file '{temporaryPdfFilePath}' not found.");
                    }

                    if (pdfOutputStream != null)
                        using (Stream fs = new FileStream(temporaryPdfFilePath, FileMode.Open))
                        {
                            byte[] buffer = new byte[32 * 1024];
                            int read;

                            while ((read = fs.Read(buffer, 0, buffer.Length)) > 0)
                                pdfOutputStream.Write(buffer, 0, read);
                        }

                    outputCallback?.Invoke(config, File.ReadAllBytes(temporaryPdfFilePath));
                }
            }
            finally
            {
                // if they specified an outputpath, don't kill the "temp" file
                if (string.IsNullOrWhiteSpace(config.OutputPath) && File.Exists(temporaryPdfFilePath))
                    File.Delete(temporaryPdfFilePath);
                if (File.Exists(temporaryContentFilePath))
                    File.Delete(temporaryContentFilePath);
                if (File.Exists(temporaryHeaderFilePath))
                    File.Delete(temporaryHeaderFilePath);
                if (File.Exists(temporaryFooterFilePath))
                    File.Delete(temporaryFooterFilePath);
                if (File.Exists(temporaryCoverFilePath))
                    File.Delete(temporaryCoverFilePath);
            }
        }
    }
}