using System.Collections.Generic;

namespace Shark.PdfConvert
{
    public class PdfConversionSettings
    {
        /// <summary>
        /// Static HTML content
        /// </summary>
        public string Content { get; set; }
        /// <summary>
        /// If you want the file to be output to a specified path (directory + filename)
        /// </summary>
        public string OutputPath { get; set; }
        /// <summary>
        /// PDF Document Title, wkhtmltopdf will use HTML document title otherwise
        /// </summary>
        public string Title { get; set; }
        /// <summary>
        /// Custom WkHtmlToPdf global options, if set none of the other global options will be used
        /// </summary>
        public string CustomWkHtmlArgs { get; set; }
        /// <summary>
        /// Custom WkHtmlToPdf cover options (applied only if cover content is specified)
        /// </summary>
        public string CustomWkHtmlCoverArgs { get; set; }
        /// <summary>
        /// Custom WkHtmlToPdf page options
        /// </summary>
        public string CustomWkHtmlPageArgs { get; set; }
        /// <summary>
        /// Custom WkHtmlToPdf toc options (applied only if GenerateToc is true)
        /// </summary>
        public string CustomWkHtmlTocArgs { get; set; }
        /// <summary>
        /// Optional Footer Arguments, you must provide PageFooterHtml, PageFooterUrl, or a stream
        /// </summary>
        public string CustomWkHtmlFooterArgs { get; set; }
        /// <summary>
        /// Optional Header Arguments, you must provide PageHeaderHtml, PageHeaderUrl, or a stream
        /// </summary>
        public string CustomWkHtmlHeaderArgs { get; set; }
        /// <summary>
        /// Get or set maximum execution time for PDF generation process (by default is null
        ///     that means a long timeout, we're not going to allow no timeout, so 5 minutes in MS)
        /// </summary>
        public int ExecutionTimeout { get; set; } = 300000;
        /// <summary>
        /// Gets or sets Table Of Contents generation flag
        /// </summary>
        public bool GenerateToc { get; set; }
        /// <summary>
        /// Gets or sets option to generate grayscale PDF
        /// </summary>
        public bool Grayscale { get; set; }
        /// <summary>
        /// Gets or sets option to generate low quality PDF
        /// </summary>
        public bool LowQuality { get; set; }
        /// <summary>
        /// Gets or sets PDF page margins (in mm)
        /// </summary>
        public PdfPageMargins Margins { get; set; } = new PdfPageMargins();
        /// <summary>
        /// Get or set PDF page orientation
        /// </summary>
        public PdfPageOrientation Orientation { get; set; } = PdfPageOrientation.Default;
        /// <summary>
        /// Get or set custom page footer HTML
        /// </summary>
        public string PageFooterHtml { get; set; }
        /// <summary>
        /// Get or set custom page header HTML
        /// </summary>
        public string PageHeaderHtml { get; set; }
        /// <summary>
        /// Get or set custom Cover Page HTML
        /// </summary>
        public string PageCoverHtml { get; set; }
        /// <summary>
        /// Gets or sets PDF page height (in mm)
        /// </summary>
        public float? PageHeight { get; set; }
        /// <summary>
        /// Gets or sets PDF page width (in mm)
        /// </summary>
        public float? PageWidth { get; set; }
        /// <summary>
        /// Get or set path where WkHtmlToPdf tool is located
        /// </summary>
        /// <remarks>
        ///  By default this property points to the folder where application assemblies are
        ///     located. If WkHtmlToPdf tool files are not present conversion will fail.
        /// </remarks>
        public string PdfToolPath { get; set; } = @"C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe";
        /// <summary>
        /// Suppress wkhtmltopdf debug/info log messages (by default is true)
        /// </summary>
        public bool Quiet { get; set; } = true;
        /// <summary>
        /// Get or set PDF page size
        /// </summary>
        public PdfPageSize Size { get; set; } = PdfPageSize.Default;
        /// <summary>
        /// Get or set location for temp files (if not specified location returned by System.IO.Path.GetTempPath
        ///     is used for temp files)
        /// </summary>
        /// <remarks>
        /// Temp files are used for providing cover page/header/footer HTML templates to
        ///     wkhtmltopdf tool when you don't specify URLs
        /// </remarks>
        public string TempFilesPath { get; set; } = System.IO.Path.GetTempPath();
        /// <summary>
        /// Gets or sets custom TOC header text (default: "Table of Contents")
        /// </summary>
        public string TocHeaderText { get; set; } = "Table of Contents";
        /// <summary>
        /// Get or set WkHtmlToPdf tool EXE file name ('wkhtmltopdf.exe' by default)
        /// </summary>
        public string WkHtmlToPdfExeName { get; set; } = "wkhtmltopdf.exe";
        /// <summary>
        /// Gets or sets zoom factor
        /// </summary>
        public float? Zoom { get; set; }
        /// <summary>
        /// If specified will override Content and any Stream passed to the Convert methods
        /// </summary>
        public string ContentUrl { get; set; }
        /// <summary>
        /// If specified will override Content, Stream, and Content Url
        /// Allows for multiple URLs. PDF will be generated in order of url list.
        /// </summary>
        public ICollection<string> ContentUrls { get; private set; } = new List<string>();
        /// <summary>
        /// If specified will override PageHeaderHtml and any Stream passed to the Convert methods
        /// </summary>
        public string PageHeaderUrl { get; set; }
        /// <summary>
        /// If specified will override PageFooterUrl and any Stream passed to the Convert methods
        /// </summary>
        public string PageFooterUrl { get; set; }
        /// <summary>
        /// If specified will override PageCoverHtml and any Stream passed to the Convert methods
        /// </summary>
        public string PageCoverUrl { get; set; }
        /// <summary>
        /// There may be a quirk with an application usage scenario where you need a new window
        /// </summary>
        public bool ProcessOptionCreateNoWindow { get; set; } = true;
        /// <summary>
        /// If you're on a platform other than Windows, you may want to have the wkhtmltopdf run via shell
        /// </summary>
        public bool ProcessOptionUseShellExecute { get; set; } = false;
        /// <summary>         
        /// I've noticed some odd behavior with this ProcessStartInfo option enabled, so I'm making the default false
        /// </summary>
        public bool ProcessOptionRedirectStandardError { get; set; } = false;
    }

    /// <summary>
    /// PDF page orientation
    /// </summary>
    public enum PdfPageOrientation
    {
        Default = 0,
        //
        // Summary:
        //     Landscape orientation
        Landscape = 1,
        //
        // Summary:
        //     Portrait orientation (default)
        Portrait = 2
    }

    /// <summary>
    /// Represents PDF page margins (unit size is mm)
    /// </summary>
    public class PdfPageMargins
    {
        /// <summary>
        /// Get or set bottom margin (in mm)
        /// </summary>
        public float? Bottom { get; set; }
        /// <summary>
        ///  Get or set left margin (in mm)
        /// </summary>
        public float? Left { get; set; } = 10; // wkhtmltopdf defaults
        /// <summary>
        /// Get or set right margin (in mm)
        /// </summary>
        public float? Right { get; set; } = 10; // wkhtmltopdf defaults
        /// <summary>
        /// Get or set top margin (in mm)
        /// </summary>
        public float? Top { get; set; }
    }

    /// <summary>
    /// All page sizes from http://doc.qt.io/archives/qt-4.8/qprinter.html#PaperSize-enum
    /// </summary>
    public enum PdfPageSize
    {
        Default = -1,
        A0 = 5, //841 x 1189 mm
        A1 = 6, //594 x 841 mm
        A2 = 7, //420 x 594 mm
        A3 = 8, //297 x 420 mm
        A4 = 0, //210 x 297 mm, 8.26 x 11.69 inches
        A5 = 9, //148 x 210 mm
        A6 = 10, //105 x 148 mm
        A7 = 11, //74 x 105 mm
        A8 = 12, //52 x 74 mm
        A9 = 13, //37 x 52 mm
        B0 = 14, //1000 x 1414 mm
        B1 = 15, //707 x 1000 mm
        B2 = 17, //500 x 707 mm
        B3 = 18, //353 x 500 mm
        B4 = 19, //250 x 353 mm
        B5 = 1, //176 x 250 mm, 6.93 x 9.84 inches
        B6 = 20, //125 x 176 mm
        B7 = 21, //88 x 125 mm
        B8 = 22, //62 x 88 mm
        B9 = 23, //33 x 62 mm
        B10 = 16, //31 x 44 mm
        C5E = 24, //163 x 229 mm
        Comm10E = 25, //105 x 241 mm, U.S. Common 10 Envelope
        DLE = 26, //110 x 220 mm
        Executive = 4, //7.5 x 10 inches, 190.5 x 254 mm
        Folio = 27, //210 x 330 mm
        Ledger = 28, //431.8 x 279.4 mm
        Legal = 3, //8.5 x 14 inches, 215.9 x 355.6 mm
        Letter = 2, //8.5 x 11 inches, 215.9 x 279.4 mm
        Tabloid = 29 //279.4 x 431.8 mm
    }
}
