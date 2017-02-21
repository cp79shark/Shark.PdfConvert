using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace Shark.PdfConvert.Samples
{
    public class Program
    {
        public static void Main(string[] args)
        {
            PdfConversionSettings config = new PdfConversionSettings
            {
                Title = "Streaming my HTML to PDF"
            };

            using (var fileStream = new FileStream(Path.GetTempFileName() + ".pdf", FileMode.Create))
            {
                var task = new System.Net.Http.HttpClient().GetStreamAsync("http://www.google.com");
                task.Wait();

                using (var inputStream = task.Result)
                {
                    PdfConvert.Convert(config, fileStream, inputStream);
                }
            }

            Console.WriteLine("Done");
            Console.ReadLine();
        }

        /// <summary>
        /// Shows reading from a website (providing an input stream instead of static content in the PdfConversionSettings.Content member
        /// Could also provide a stream for the header and footer, we provide static content here.
        /// Additionally you could provide a callback.
        /// </summary>
        /// <param name="tempFileName"></param>
        private static void NotQuiteTheKitchenSink(string tempFileName)
        {
            // the kitchen sink example
            var sinkData = new PdfConversionSettings()
            {
                ExecutionTimeout = 90000, // one minute, have noticed some odd behavior with this sample
                ProcessOptionRedirectStandardError = false,
                //PdfToolPath = @"C:\Program Files\wkhtmltopdf\bin\wkhtmltopdf.exe",
                Title = "Kitchen Sink Pdf Export",
                PageHeaderHtml = @"
<!DOCTYPE html>
<html><body>
<div style=""background-color: blue; color: white; text-align: center; width: 100vw;"">From the Interwebs</div>
</body></html>
",
                PageFooterHtml = @"
<!DOCTYPE html>
<html><head><script>
function subst() { var vars={}; var x=window.location.search.substring(1).split('&');
for (var i in x) {var z=x[i].split('=',2);vars[z[0]] = unescape(z[1]);}
var x=['frompage','topage','page','webpage','section','subsection','subsubsection'];
for (var i in x) { var y = document.getElementsByClassName(x[i]); for (var j=0; j<y.length; ++j) y[j].textContent = vars[x[i]]; } }
</script></head><body style=""border:0; margin: 0; "" onload=""subst()"">
<table style=""border-bottom: 1px solid black; width: 100%""><tr><td class=""section""></td>
<td style=""text-align:right"">Page <span class=""page""></span> of<span class=""topage""></span>
</td></tr></table></body></html>
",
                GenerateToc = true,
                ContentUrl = "http://www.lipsum.com/"
            };

            // write our http string
            using (var fs = new FileStream(tempFileName, FileMode.Create))
            {
                PdfConvert.Convert(sinkData, fs);
            }
        }
    }
}
