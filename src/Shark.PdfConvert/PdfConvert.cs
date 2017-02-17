namespace Shark.PdfConvert
{
    using System;
    using System.Text;
    using System.IO;
    using System.Diagnostics;

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
                options.Append(" ");
                options.Append(config.CustomWkHtmlArgs);
                options.Append(" ");
            }

            // COVER
            if (string.IsNullOrWhiteSpace(temporaryCoverFilePath) == false)
            {
                options.AppendFormat(" cover  \"{0}\" ", temporaryCoverFilePath);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlCoverArgs) == false)
                {
                    options.Append(" ");
                    options.Append(config.CustomWkHtmlCoverArgs);
                    options.Append(" ");
                }
            }

            // FOOTER
            if (string.IsNullOrWhiteSpace(temporaryFooterFilePath) == false)
            {
                options.AppendFormat(" --footer-html  \"{0}\" ", temporaryFooterFilePath);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlFooterArgs) == false)
                {
                    options.Append(" ");
                    options.Append(config.CustomWkHtmlFooterArgs);
                    options.Append(" ");
                }
            }

            // HEADER
            if (string.IsNullOrWhiteSpace(temporaryHeaderFilePath) == false)
            {
                options.AppendFormat(" --header-html  \"{0}\" ", temporaryHeaderFilePath);

                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlHeaderArgs) == false)
                {
                    options.Append(" ");
                    options.Append(config.CustomWkHtmlHeaderArgs);
                    options.Append(" ");
                }
            }

            // TABLE OF CONTENTS
            if (config.GenerateToc)
            {
                options.Append(" toc ");
                if (string.IsNullOrWhiteSpace(config.CustomWkHtmlTocArgs) == false)
                {
                    options.Append(" ");
                    options.Append(config.CustomWkHtmlTocArgs);
                    options.Append(" ");
                }
            }

            // PAGE
            options.AppendFormat(" page \"{1}\" \"{0}\"", temporaryPdfFilePath, temporaryContentFilePath);

            // PAGE OPTIONS
            if (string.IsNullOrWhiteSpace(config.CustomWkHtmlPageArgs))
            {
                if (config.Zoom != null) options.AppendFormat("--zoom {0} ", config.Zoom);
            }
            else
            {
                options.Append(" ");
                options.Append(config.CustomWkHtmlPageArgs);
                options.Append(" ");
            }

            return options.ToString();
        }

        /// <summary>
        /// Short cut method if you don't want to specify a stream for the content input, header, footer, etc
        /// and use the PdfExportModel.Content member for your content instead of init'ing your own stream.
        /// </summary>
        /// <param name="config"></param>
        /// <param name="pdfOutputStream"></param>
        /// <param name="outputCallback"></param>
        public static void Convert(PdfConversionSettings config,
            Stream pdfOutputStream,
            Stream coverInputStream = null,
            Stream footerInputStream = null,
            Stream headerInputStream = null,
            Action<PdfConversionSettings, byte[]> outputCallback = null)
        {
            using (var readerStream = new MemoryStream(Encoding.UTF8.GetBytes(config.Content)))
            {
                Convert(config, readerStream, pdfOutputStream, coverInputStream, footerInputStream, headerInputStream, outputCallback);
            }
        }

        /// <summary>
        /// Converts an HTML source input into a PDF output using the WkHTMLToPDF tool.
        /// The kitchen sink overload.
        /// </summary>
        /// <param name="config">Conversion</param>
        /// <param name="contentInputStream"></param>
        /// <param name="pdfOutputStream"></param>
        /// <param name="outputCallback"></param>
        public static void Convert(PdfConversionSettings config,
            Stream contentInputStream,
            Stream pdfOutputStream,
            Stream coverInputStream,
            Stream footerInputStream,
            Stream headerInputStream,
            Action<PdfConversionSettings, byte[]> outputCallback)
        {
            if (!File.Exists(config.PdfToolPath))
                throw new ArgumentException($"File '{config.PdfToolPath}' not found. Check if wkhtmltopdf application is installed and set the correct location before calling this method.");

            if (contentInputStream == null)
                throw new ArgumentException("You must specify an input stream.");

            if (pdfOutputStream == null)
                throw new ArgumentException("You must specify an output stream.");

            if (config.GenerateToc && coverInputStream == null)
                throw new ArgumentException("You must specify a cover stream when specifying Generate TOC");

            string temporaryContentFilePath = Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");
            string temporaryPdfFilePath = Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.pdf");
            string temporaryCoverFilePath = coverInputStream == null ?
                null :
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");
            string temporaryFooterFilePath = footerInputStream == null && string.IsNullOrWhiteSpace(config.PageFooterHtml) ?
                null :
                Path.Combine(config.TempFilesPath, $"{Guid.NewGuid()}.html");
            string temporaryHeaderFilePath = headerInputStream == null && string.IsNullOrWhiteSpace(config.PageHeaderHtml) ?
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
                        coverInputStream.Position = 0;
                        coverInputStream.CopyTo(tempCoverFileStream);
                    }
                }

                // FOOTER
                if (footerInputStream != null)
                {
                    using (var tempFooterFileStream = new FileStream(temporaryFooterFilePath, FileMode.CreateNew))
                    {
                        footerInputStream.Position = 0;
                        footerInputStream.CopyTo(tempFooterFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.PageFooterHtml) == false)
                {
                    // footer, if content is manually specified and no stream specified, then use the content
                    File.WriteAllText(temporaryFooterFilePath, config.PageFooterHtml);
                }

                // HEADER
                if (headerInputStream != null)
                {
                    using (var tempHeaderFileStream = new FileStream(temporaryHeaderFilePath, FileMode.CreateNew))
                    {
                        headerInputStream.Position = 0;
                        headerInputStream.CopyTo(tempHeaderFileStream);
                    }
                }
                else if (string.IsNullOrWhiteSpace(config.PageHeaderHtml) == false)
                {
                    // header, if content is manually specified and no stream specified, then use the content
                    File.WriteAllText(temporaryHeaderFilePath, config.PageHeaderHtml);
                }


                // CONTENT
                using (var tempContentFileStream = new FileStream(temporaryContentFilePath, FileMode.CreateNew))
                {
                    contentInputStream.Position = 0;
                    contentInputStream.CopyTo(tempContentFileStream);
                }

                // start process
                using (var process = new Process())
                {
                    ProcessStartInfo startInfo = new ProcessStartInfo()
                    {
                        CreateNoWindow = true,
                        FileName = config.PdfToolPath,
                        Arguments = commandOptions,
                        UseShellExecute = false,
                        RedirectStandardError = true
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
                            throw new InvalidDataException($"WkHTMLToPdf conversion of '{config.OutputFilename}' failed. Output: \r\n{error}");
                        }

                        throw new InvalidDataException($"WkHTMLToPdf conversion of '{config.OutputFilename}' failed. Output file '{temporaryPdfFilePath}' not found.");
                    }

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
                if (File.Exists(temporaryPdfFilePath))
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